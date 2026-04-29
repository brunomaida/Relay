## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks.Multi_Packet_IsHealthy()
       mov       rcx,[rcx+8]
       cmp       [rcx],ecx
       jmp       qword ptr [7FF7C74313F0]; Relay.MultiSink.get_IsHealthy()
; Total bytes of code 12
```
```assembly
; Relay.MultiSink.get_IsHealthy()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+20]
       xor       esi,esi
       cmp       dword ptr [rbx+8],0
       jle       short M01_L03
       mov       rdi,offset MT_Relay.Benchmarks.ByteCounterPipe
M01_L00:
       mov       rcx,[rbx+rsi*8+10]
       cmp       [rcx],rdi
       jne       short M01_L02
M01_L01:
       mov       eax,1
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       mov       rax,[rcx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L01
       inc       esi
       cmp       [rbx+8],esi
       jg        short M01_L00
M01_L03:
       xor       eax,eax
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 83
```

