## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi_IsHealthy()
       mov       rcx,[rcx+8]
       cmp       [rcx],ecx
       jmp       qword ptr [7FF7C6D51A30]; Relay.MultiSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].get_IsHealthy()
; Total bytes of code 12
```
```assembly
; Relay.MultiSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].get_IsHealthy()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+18]
       xor       esi,esi
       cmp       dword ptr [rbx+8],0
       jle       short M01_L03
       mov       rdi,offset MT_Relay.Benchmarks.SinkPipe
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

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi2_IsHealthy()
       mov       rcx,[rcx+10]
       cmp       [rcx],ecx
       jmp       qword ptr [7FF7C6D42060]; Relay.Multi2Sink`3[[Relay.Benchmarks.Entry64, Relay.Benchmarks],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
; Total bytes of code 12
```
```assembly
; Relay.Multi2Sink`3[[Relay.Benchmarks.Entry64, Relay.Benchmarks],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rcx,[rbx+18]
       mov       rax,offset MT_Relay.Benchmarks.SinkPipe
       cmp       [rcx],rax
       jne       short M01_L01
M01_L00:
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M01_L01:
       mov       rax,[rcx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L00
       mov       rcx,[rbx+20]
       mov       rax,[rcx]
       mov       rax,[rax+40]
       add       rsp,20
       pop       rbx
       jmp       qword ptr [rax+20]
; Total bytes of code 72
```

