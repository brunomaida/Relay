G1 - Packet consumer idle clock check
Classification: recommended small cleanup. It affects idle CPU/power and flush polling overhead, not payload-copy throughput.

Where:

src/Relay/SpscQueueSink.Packet.cs:160
src/Relay/MpscQueueSink.Packet.cs:180
Current shape:

bool flushDue = Volatile.Read(ref _flushRequested) == 1
             || HfClock.NowTicks >= flushDeadline;
if (checkDeadline && flushDue)
{
    Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    TryRecoverBackend();
    TryDrainToPrev();
    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
}
In the spin phase, checkDeadline is false for most iterations, but the packet loops still call HfClock.NowTicks unless _flushRequested == 1. That contradicts the topology intent: "spin phase: every 8 iters". The typed loops already use the cheaper shape.

What to do:

Align packet loops with the typed loops:

bool flushNow    = Volatile.Read(ref _flushRequested) == 1;
bool deadlineHit = checkDeadline && HfClock.NowTicks >= flushDeadline;

if (flushNow || deadlineHit)
{
    if (flushNow) Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    if (deadlineHit)
    {
        TryRecoverBackend();
        TryDrainToPrev();
    }
    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
}
If exact current packet semantics must be preserved for TryRecoverBackend on manual flush, keep those calls unconditional inside the block; the important performance part is deadlineHit = checkDeadline && ....

Expected gain:

Up to 8 avoided Stopwatch.GetTimestamp calls in the first 10 empty spin iterations when there is no explicit flush request. Using the cost map's ~20c per HfClock.NowTicks, that is up to ~160c saved per empty spin cycle. Memory gain: none.

G2 - SharedMemorySink contiguous-write fast path
Classification: conditional; worth implementing only if SharedMemorySink is a real high-rate path.

Where:

src/Relay/Sinks/SharedMemorySink.cs:88 (Accept)
src/Relay/Sinks/SharedMemorySink.cs:119 (WriteRing)
Current shape:

After reserving with CAS, every payload uses a stackalloc byte[4] length buffer plus two modular WriteRing calls, even when the frame is fully contiguous in the MMF data area.

What to do:

Add a common-case branch after the CAS reservation:

byte* data = _ptr + HEADER_SIZE;
if (oldIdx + frameLen <= _dataCapacity)
{
    BinaryPrimitives.WriteInt32BigEndian(new Span<byte>(data + oldIdx, 4), payload.Length);
    payload.CopyTo(new Span<byte>(data + oldIdx + 4, payload.Length));
    return true;
}

// existing wrapped path
Span<byte> lenBuf = stackalloc byte[4];
BinaryPrimitives.WriteInt32BigEndian(lenBuf, payload.Length);
WriteRing(data, _dataCapacity, oldIdx, lenBuf);
WriteRing(data, _dataCapacity, (oldIdx + 4) % _dataCapacity, payload);
return true;
Expected gain:

Measured current cost is ~11.79 ns for 64B and ~13.96 ns for 256B, 0 B allocated. The unavoidable part is the CAS plus payload copy; the avoidable part is stackalloc + two modular WriteRing loops in the non-wrap case. A realistic target is a small single-digit ns gain (~5-15c) on contiguous writes, gated by BDN. Memory gain: none.

G3 - UDP batching is a protocol-level option, not a transparent fix
Classification: conditional; do not implement as a default replacement.

Where:

src/Relay/Sinks/UdpSink.cs:41
Current shape:

UdpSink.WriteToBackend emits one UDP datagram per payload, so the consumer pays a syscall per delivered record. The static model estimates ~2000c per record. The existing BDN push benchmark measures producer-side enqueue/saturation, not delivered datagrams, so it cannot prove wire throughput.

What to do first:

Add a delivered-count benchmark or receiver-side counter before changing code. If the wire protocol can accept multiple logical payloads in one UDP datagram, add a separate opt-in sink that batches length-delimited payloads up to MTU / maxPayload. Do not silently change UdpSink datagram semantics.

Expected gain if protocol allows batching:

Syscall cost can be amortized from ~2000c per payload to roughly ~2000c per batch plus framing/copy cost. Memory impact would be one reusable send buffer. Without protocol permission, expected safe gain is zero.