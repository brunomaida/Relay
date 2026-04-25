using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

/// <summary>MmfSink write/readback, capacity exhaustion, and fallback behaviour.</summary>
public sealed class MmfSinkTests
{
    private static readonly int EntrySize = Unsafe.SizeOf<Entry64>();

    [Fact]
    public void MmfSink_WritesAllEntries_ReadbackMatches()
    {
        var path = Path.GetTempFileName();
        File.Delete(path); // MMF creates the file
        try
        {
            const int count = 16;
            long maxBytes = EntrySize * count;

            using (var pipe = new MmfSink<Entry64>(path, maxBytes, ringCapacity: 64, flushInterval: 50))
            {
                pipe.Start();
                for (int i = 0; i < count; i++)
                    pipe.Enqueue(new Entry64 { A = i, B = i * 10 });
                pipe.Stop(drainTimeoutMs: 2_000);
            }

            var bytes = File.ReadAllBytes(path);
            bytes.Length.Should().BeGreaterThanOrEqualTo((int)maxBytes);

            for (int i = 0; i < count; i++)
            {
                long a = BitConverter.ToInt64(bytes, i * EntrySize);
                long b = BitConverter.ToInt64(bytes, i * EntrySize + 8);
                a.Should().Be(i);
                b.Should().Be(i * 10);
            }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void MmfSink_CapacityExhausted_IsHealthyFalse()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        try
        {
            long maxBytes = EntrySize * 4;

            using var pipe = new MmfSink<Entry64>(path, maxBytes, ringCapacity: 8, flushInterval: 25);
            pipe.Start();

            for (int i = 0; i < 4; i++)
                pipe.Enqueue(new Entry64 { A = i });

            // Give the consumer time to drain all 4 entries to MMF.
            Thread.Sleep(200);

            pipe.IsHealthy.Should().BeFalse("MMF is full — no more room for EntrySize bytes");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void MmfSink_Full_FallsBackToNext()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        try
        {
            long maxBytes = EntrySize * 2;

            var fallback = new CountingPipe();
            using var pipe = new MmfSink<Entry64>(path, maxBytes, ringCapacity: 8, flushInterval: 25);
            RelayBuilder.Start<Entry64, MmfSink<Entry64>>(pipe).To(fallback).Build();
            pipe.Start();

            for (int i = 0; i < 2; i++)
                pipe.Enqueue(new Entry64 { A = i });

            Thread.Sleep(200); // let MMF fill

            pipe.Enqueue(new Entry64 { A = 99 }); // IsHealthy=false → fallback
            pipe.Enqueue(new Entry64 { A = 100 });

            pipe.Stop(drainTimeoutMs: 1_000);

            fallback.Accepted.Should().Be(2);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void MmfSink_ThrowsWhenMaxBytesBelowEntrySize()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        try
        {
            Action act = () => new MmfSink<Entry64>(path, EntrySize - 1);
            act.Should().Throw<ArgumentException>();
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private sealed class CountingPipe : DispatchSink<Entry64>
    {
        public int Accepted { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
