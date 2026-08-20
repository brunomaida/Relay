### Docs
- CLAUDE.md hygiene: Project Layout tree replaced with a pointer to `docs/TOPOLOGY.md` (tree had drifted ~20 files stale); removed the stale Phase-1 `Status` section (SinkChainBuilder, SinkChain.Packet.cs, ForkSink.Packet.cs all shipped) while preserving the still-true MPSC-no-concrete-backends note; deduplicated the two conflicting commit-gate lines down to the `Relay.sln --filter` gate; collapsed the duplicated Model Routing section to a pointer at global `fact-workflow-and-planning`; fixed provenance tag on the Multi2/Multi BDN claim (previously cited MultiEnqueue/MultiPacketEnqueue bench-history, which measures 3.25/3.38 ns, not the documented 3.31/3.18 ns) to cite `docs/reports/2026-07-02-resource-cost-map-relay.md` instead.

### Perf
- N/A — documentation only
