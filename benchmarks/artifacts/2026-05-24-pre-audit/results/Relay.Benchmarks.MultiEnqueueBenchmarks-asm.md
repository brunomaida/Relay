## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MultiEnqueueBenchmarks.Multi_Enqueue()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+8]
       lea       rsi,[rcx+18]
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E4519C0]; Relay.MultiSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].get_IsHealthy()
       test      eax,eax
       je        short M00_L03
       mov       rdi,[rbx+18]
       mov       ebp,[rdi+8]
       test      ebp,ebp
       jle       short M00_L01
       add       rdi,10
M00_L00:
       mov       rcx,[rdi]
       mov       rdx,rsi
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E47DE30]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       add       rdi,8
       dec       ebp
       jne       short M00_L00
M00_L01:
       cmp       byte ptr [rbx+10],0
       jne       short M00_L03
M00_L02:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       rdx,rsi
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       jmp       qword ptr [7FFB0E47DE30]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 109
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
       mov       rdi,offset MT_Relay.Benchmarks.CounterPipe
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
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rsi,rdx
M02_L00:
       mov       rax,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rax,rcx
       jne       short M02_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M02_L01:
       test      ecx,ecx
       je        short M02_L04
       cmp       byte ptr [rbx+10],0
       jne       short M02_L04
M02_L02:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L03:
       mov       rcx,rbx
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M02_L04
       mov       rcx,rbx
       mov       rdx,rsi
       call      qword ptr [rdi+28]
       mov       ecx,eax
       jmp       short M02_L01
M02_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M02_L02
       jmp       short M02_L00
; Total bytes of code 99
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MultiEnqueueBenchmarks.Multi2_Enqueue()
       push      rsi
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       lea       rsi,[rcx+18]
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E451FF0]; Relay.Multi2Sink`3[[Relay.Benchmarks.Entry64, Relay.Benchmarks],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
       test      eax,eax
       je        short M00_L01
       mov       rcx,[rbx+18]
       mov       rdx,rsi
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E47DD28]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       mov       rcx,[rbx+20]
       mov       rdx,rsi
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E47DD28]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       cmp       byte ptr [rbx+10],0
       jne       short M00_L01
M00_L00:
       add       rsp,28
       pop       rbx
       pop       rsi
       ret
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L00
       mov       rdx,rsi
       add       rsp,28
       pop       rbx
       pop       rsi
       jmp       qword ptr [7FFB0E47DD28]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 96
```
```assembly
; Relay.Multi2Sink`3[[Relay.Benchmarks.Entry64, Relay.Benchmarks],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rcx,[rbx+18]
       mov       rax,offset MT_Relay.Benchmarks.CounterPipe
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
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rsi,rdx
M02_L00:
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rdi,rcx
       jne       short M02_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M02_L01:
       test      ecx,ecx
       je        short M02_L04
       cmp       byte ptr [rbx+10],0
       jne       short M02_L04
M02_L02:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M02_L04
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M02_L01
M02_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M02_L02
       jmp       short M02_L00
; Total bytes of code 103
```

