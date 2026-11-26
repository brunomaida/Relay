---
adr: none
---
### Tests
- Added SWAN flow tests under `tests/Relay.Tests/SwanRoutes/` for the frozen SWAN hot routes of Relay (SWAN S5.5d): one test per route builds the route's own sink chain, calls its first code hop in the body and asserts the exit's effect (file bytes, deleted files, peer-received frames, HTTP batches, predicate decisions). Deadline-gated hops (backend recovery, drain to `Prev`) are driven by their condition; every wait is a bounded, asserted `SpinWait.SpinUntil`, including after each `Stop`.
- 41 of the 48 listed routes are linked in `.swan/swan.links.json` (34 new tests, 7 existing tests linked as-is); 7 routes carry owner-signed H3 exclusions (5 type-level `ForkSink`/`MultiSink` hops, 2 exits whose write is unreachable after the consumer's final `FlushBackend`).
- Committed the `.swan/` v2 slice of Relay as a byte copy of the SWAN fixture.

### Perf
- N/A: test and documentation changes only, no runtime code path changed.
