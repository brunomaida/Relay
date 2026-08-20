using System.Runtime.InteropServices;

namespace Relay.Tests.Memory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Payload1
{
    public byte Value;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
internal struct Payload8
{
    public long Value;
}

[StructLayout(LayoutKind.Sequential, Size = 64)]
internal struct Payload64
{
    public long Value;
}

[StructLayout(LayoutKind.Sequential, Size = 256)]
internal struct Payload256
{
    public long Value;
}
