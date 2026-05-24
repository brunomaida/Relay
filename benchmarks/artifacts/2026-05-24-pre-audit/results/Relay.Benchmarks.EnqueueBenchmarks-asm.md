## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.EnqueueBenchmarks.Depth1_Healthy()
       mov       rdx,[rcx+8]
       add       rcx,28
       mov       rax,rcx
       cmp       [rdx],dl
       mov       rcx,[rax]
       mov       [rdx+18],rcx
       cmp       byte ptr [rdx+10],0
       jne       short M00_L01
M00_L00:
       ret
M00_L01:
       mov       rcx,[rdx+8]
       test      rcx,rcx
       je        short M00_L00
       mov       rdx,rax
       jmp       qword ptr [7FFB0E47E178]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 45
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rsi,rdx
M01_L00:
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rdi,rcx
       jne       short M01_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M01_L01:
       test      ecx,ecx
       je        short M01_L04
       cmp       byte ptr [rbx+10],0
       jne       short M01_L04
M01_L02:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L04
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M01_L01
M01_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M01_L02
       jmp       short M01_L00
; Total bytes of code 103
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.EnqueueBenchmarks.Depth2_AcceptReject()
       mov       rdx,[rcx+10]
       add       rcx,28
       mov       rax,rcx
       mov       rcx,[rdx+8]
       test      rcx,rcx
       je        short M00_L00
       mov       rdx,rax
       jmp       qword ptr [7FFB0E49E178]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M00_L00:
       ret
; Total bytes of code 30
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,rdx
M01_L00:
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rdi,rcx
       jne       short M01_L02
M01_L01:
       mov       rcx,offset MT_Relay.Benchmarks.RejectPipe
       cmp       rdi,rcx
       jne       short M01_L03
       xor       ebp,ebp
       jmp       short M01_L04
M01_L02:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L05
       jmp       short M01_L01
M01_L03:
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ebp,eax
M01_L04:
       test      ebp,ebp
       jne       short M01_L06
M01_L05:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M01_L07
       jmp       short M01_L00
M01_L06:
       cmp       byte ptr [rbx+10],0
       jne       short M01_L05
M01_L07:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 112
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.EnqueueBenchmarks.Depth2_HeadUnhealthy()
       mov       rdx,[rcx+18]
       add       rcx,28
       mov       rax,rcx
       mov       rcx,[rdx+8]
       test      rcx,rcx
       je        short M00_L00
       mov       rdx,rax
       jmp       qword ptr [7FFB0E4AE160]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M00_L00:
       ret
; Total bytes of code 30
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,offset MT_Relay.Benchmarks.CounterPipe
M01_L00:
       mov       rbp,[rbx]
       cmp       rbp,rdi
       jne       short M01_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M01_L01:
       test      ecx,ecx
       je        short M01_L04
       cmp       byte ptr [rbx+10],0
       jne       short M01_L04
M01_L02:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M01_L03:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L05
M01_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M01_L02
       jmp       short M01_L00
M01_L05:
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M01_L01
; Total bytes of code 105
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.EnqueueBenchmarks.Depth3_AllUnhealthy()
       mov       rdx,[rcx+20]
       add       rcx,28
       mov       rax,rcx
       mov       rcx,[rdx+8]
       test      rcx,rcx
       je        short M00_L00
       mov       rdx,rax
       jmp       qword ptr [7FFB0E49E160]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M00_L00:
       ret
; Total bytes of code 30
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,offset MT_Relay.Benchmarks.DeadPipe
M01_L00:
       mov       rbp,[rbx]
       cmp       rbp,rdi
       jne       short M01_L02
M01_L01:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M01_L04
       jmp       short M01_L00
M01_L02:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L01
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rbp,rcx
       jne       short M01_L05
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M01_L03:
       test      ecx,ecx
       je        short M01_L01
       cmp       byte ptr [rbx+10],0
       jne       short M01_L01
M01_L04:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M01_L05:
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M01_L03
; Total bytes of code 120
```

