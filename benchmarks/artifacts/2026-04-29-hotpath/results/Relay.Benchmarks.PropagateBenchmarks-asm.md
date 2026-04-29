## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Default()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+8]
       lea       rsi,[rcx+28]
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rdi,rcx
       jne       short M00_L02
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M00_L00:
       test      ecx,ecx
       je        short M00_L03
       cmp       byte ptr [rbx+10],0
       jne       short M00_L03
M00_L01:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L02:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L03
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M00_L00
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L01
       mov       rdx,rsi
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       jmp       qword ptr [7FF7C6D7E0A0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 119
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
; Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Propagate_NoNext()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       lea       rsi,[rcx+28]
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.PropagateCounterPipe
       cmp       rdi,rcx
       jne       short M00_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M00_L00:
       test      ecx,ecx
       je        short M00_L01
       cmp       byte ptr [rbx+10],0
       je        short M00_L02
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       short M00_L04
M00_L02:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L01
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M00_L00
M00_L04:
       mov       rdx,rsi
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       jmp       qword ptr [7FF7C6D7E0B8]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 119
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
       mov       rcx,offset MT_Relay.Benchmarks.PropagateCounterPipe
       cmp       rdi,rcx
       jne       short M01_L03
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M01_L01:
       test      ecx,ecx
       je        short M01_L02
       cmp       byte ptr [rbx+10],0
       je        short M01_L04
M01_L02:
       mov       rbp,[rbx+8]
       test      rbp,rbp
       je        short M01_L04
       jmp       short M01_L05
M01_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L02
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       ecx,eax
       jmp       short M01_L01
M01_L04:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M01_L05:
       mov       rbx,rbp
       jmp       short M01_L00
; Total bytes of code 110
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PropagateBenchmarks.Depth2_Propagate_Fork()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+18]
       lea       rsi,[rcx+28]
       mov       rax,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.PropagateCounterPipe
       cmp       rax,rcx
       jne       short M00_L02
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M00_L00:
       test      ecx,ecx
       je        short M00_L01
       cmp       byte ptr [rbx+10],0
       je        short M00_L03
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       rdx,rsi
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       jmp       qword ptr [7FF7C6D6E0A0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M00_L02:
       mov       rcx,rbx
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M00_L01
       mov       rcx,rbx
       mov       rdx,rsi
       call      qword ptr [rdi+28]
       mov       ecx,eax
       jmp       short M00_L00
M00_L03:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 115
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
       mov       rcx,offset MT_Relay.Benchmarks.PropagateCounterPipe
       cmp       rdi,rcx
       je        short M01_L01
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L04
M01_L01:
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rdi,rcx
       je        short M01_L02
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       jmp       short M01_L03
M01_L02:
       mov       rax,[rsi]
       mov       [rbx+18],rax
       mov       eax,1
M01_L03:
       test      eax,eax
       je        short M01_L04
       cmp       byte ptr [rbx+10],0
       je        short M01_L05
M01_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       jne       short M01_L00
M01_L05:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 114
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PropagateBenchmarks.Depth2_Fork_Wrapped()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+20]
       lea       rsi,[rcx+28]
       mov       rax,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rax,rcx
       jne       short M00_L02
       mov       rcx,[rsi]
       mov       [rbx+18],rcx
       mov       ecx,1
M00_L00:
       test      ecx,ecx
       je        short M00_L03
       cmp       byte ptr [rbx+10],0
       jne       short M00_L03
M00_L01:
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L02:
       mov       rcx,rbx
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M00_L03
       mov       rcx,rbx
       mov       rdx,rsi
       call      qword ptr [rdi+28]
       mov       ecx,eax
       jmp       short M00_L00
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L01
       mov       rdx,rsi
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       jmp       qword ptr [7FF7C6D8E0A0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
; Total bytes of code 115
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
       mov       rax,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.CounterPipe
       cmp       rax,rcx
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
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M01_L04
       mov       rcx,rbx
       mov       rdx,rsi
       call      qword ptr [rdi+28]
       mov       ecx,eax
       jmp       short M01_L01
M01_L04:
       mov       rbx,[rbx+8]
       test      rbx,rbx
       je        short M01_L02
       jmp       short M01_L00
; Total bytes of code 99
```

