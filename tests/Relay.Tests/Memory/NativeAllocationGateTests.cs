using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Relay.Tests.Memory;

/// <summary>
/// Self-proving source-scan gate: no raw <c>NativeMemory.*</c> call — nor a <c>using static</c>
/// import of it — may appear anywhere under <c>src\Relay</c> except inside
/// <see cref="Relay.Memory.NativeBuffer"/>, the single sanctioned entry point for native allocation
/// (see <c>NativeBuffer.cs</c>'s own XML doc for the Wave-leak rationale).
/// </summary>
/// <remarks>
/// "Self-proving" means the detector is not trusted just because it currently reports zero
/// violations against clean source — a regex that matches nothing is indistinguishable from a
/// correct regex until it is shown to catch a real violation. <see cref="FindViolations"/> is the
/// single function both the repo-wide scan and the fixture-based positive-control tests call, so a
/// broken detector fails the positive controls, not just silently passes the scan.
/// </remarks>
public sealed class NativeAllocationGateTests
{
    private static readonly string ExemptRelativePath = Path.Combine("Memory", "NativeBuffer.cs");

    // Real token match: "NativeMemory." preceded by a non-identifier char (or start of line).
    private static readonly Regex NativeMemoryCallPattern = new(@"\bNativeMemory\.", RegexOptions.Compiled);

    // "using static ...NativeMemory;" bypasses the dotted-call pattern above entirely (call sites
    // read as bare "AllocZeroed(...)"), so it must be banned as its own shape.
    private static readonly Regex UsingStaticPattern =
        new(@"using\s+static\s+System\.Runtime\.InteropServices\.NativeMemory\s*;", RegexOptions.Compiled);

    /// <summary>
    /// Scans <paramref name="sourceText"/> line by line and returns every line (after stripping
    /// <c>//</c>/<c>///</c> line comments) that references <c>NativeMemory.</c> as a real token or
    /// imports it via <c>using static</c>.
    /// </summary>
    /// <remarks>
    /// Line-comment stripping only — no block-comment (<c>/* */</c>) or string-literal awareness.
    /// Not needed for this codebase (no such usage exists), and a hand-rolled C# tokenizer would be
    /// disproportionate for a source-scan test.
    /// </remarks>
    private static List<(int LineNumber, string Text)> FindViolations(string sourceText)
    {
        var violations = new List<(int, string)>();
        string[] lines = sourceText.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string codeOnly = StripLineComment(lines[i]);
            if (NativeMemoryCallPattern.IsMatch(codeOnly) || UsingStaticPattern.IsMatch(codeOnly))
                violations.Add((i + 1, lines[i].Trim()));
        }

        return violations;
    }

    private static string StripLineComment(string line)
    {
        int idx = line.IndexOf("//", StringComparison.Ordinal);
        return idx >= 0 ? line[..idx] : line;
    }

    private static string ResolveSourceRoot()
    {
        AssemblyMetadataAttribute? attribute = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "RelaySourceRoot");

        if (attribute?.Value is not { Length: > 0 } rawPath)
            throw new InvalidOperationException(
                "RelaySourceRoot AssemblyMetadataAttribute not found on the test assembly. " +
                "Expected <AssemblyMetadata Include=\"RelaySourceRoot\" Value=\"...\\src\\Relay\" /> in Relay.Tests.csproj.");

        string fullPath = Path.GetFullPath(rawPath);
        if (!Directory.Exists(fullPath))
            throw new InvalidOperationException(
                $"RelaySourceRoot resolved to '{fullPath}', which does not exist.");

        return fullPath;
    }

    [Fact]
    public void SourceRoot_Resolves()
    {
        string root = ResolveSourceRoot();

        Directory.Exists(root).Should().BeTrue();
        File.Exists(Path.Combine(root, "Memory", "NativeBuffer.cs")).Should().BeTrue(
            "the resolved root should be src\\Relay, which must contain NativeBuffer.cs");
    }

    [Fact]
    public void NativeMemory_IsNotCalledOutsideNativeBuffer()
    {
        string root = ResolveSourceRoot();
        var offendingFiles = new List<string>();

        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(root, file);

            if (relativePath.Split(Path.DirectorySeparatorChar).Any(seg => seg is "obj" or "bin"))
                continue;
            if (string.Equals(relativePath, ExemptRelativePath, StringComparison.OrdinalIgnoreCase))
                continue;

            string text = File.ReadAllText(file);
            if (FindViolations(text).Count > 0)
                offendingFiles.Add(relativePath);
        }

        offendingFiles.Should().BeEmpty(
            "NativeBuffer.cs must be the only sanctioned call site for NativeMemory.* under src\\Relay");
    }

    [Fact]
    public void Detector_FlagsKnownViolationFixture()
    {
        // Positive control — proves FindViolations actually catches a violation. Without this test,
        // NativeMemory_IsNotCalledOutsideNativeBuffer passing could mean either "the code is clean"
        // or "the detector never fires" — the two are indistinguishable without this fixture.
        const string fixture = """
            internal static unsafe class RogueAllocator
            {
                internal static void* Allocate(nuint bytes)
                {
                    return NativeMemory.AllocZeroed(bytes, (nuint)sizeof(int));
                }
            }
            """;

        FindViolations(fixture).Should().NotBeEmpty(
            "the detector must flag a raw NativeMemory.* call — this is the positive control proving the gate is not a no-op regex");
    }

    [Fact]
    public void Detector_FlagsUsingStaticImportFixture()
    {
        // Second positive control, specific to the using-static bypass shape called out in the brief:
        // once imported this way, call sites read as bare "AllocZeroed(...)" with no "NativeMemory."
        // token at all, so this must be caught by the import line itself.
        const string fixture = """
            using static System.Runtime.InteropServices.NativeMemory;

            internal static class RogueAllocator
            {
                internal static unsafe void* Allocate(nuint bytes) => AllocZeroed(bytes);
            }
            """;

        FindViolations(fixture).Should().NotBeEmpty(
            "a using-static import of NativeMemory bypasses the dotted-call pattern and must be flagged on its own");
    }

    [Fact]
    public void Detector_IgnoresXmlDocReferences()
    {
        // Mirrors the real false-positive case in SpscRingBuffer.cs:19.
        const string fixture = """
            /// Backing memory is allocated via <see cref="NativeMemory.AlignedAlloc(nuint, nuint)"/> with
            /// 64-byte alignment.
            internal sealed class Foo
            {
            }
            """;

        FindViolations(fixture).Should().BeEmpty(
            "an XML-doc <see cref> reference is not a real call and must not trip the scanner");
    }

    [Fact]
    public void Detector_FlagsRealCallButNotAdjacentXmlDoc()
    {
        // Proves comment-stripping doesn't over-strip: a doc-comment reference on one line must be
        // ignored while a real call two lines below is still caught, at the correct line number.
        const string fixture = """
            /// Backing memory is allocated via <see cref="NativeMemory.AlignedAlloc(nuint, nuint)"/> with
            /// 64-byte alignment.
            internal sealed unsafe class Foo
            {
                internal void* Allocate(nuint bytes) => NativeMemory.AllocZeroed(bytes);
            }
            """;

        var violations = FindViolations(fixture);
        violations.Should().HaveCount(1);
        violations[0].LineNumber.Should().Be(5);
    }
}
