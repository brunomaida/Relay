## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.FilterSinkBenchmarks.Filter_Pass()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,60
       mov       rbx,[rcx+8]
       lea       rsi,[rcx+18]
       mov       rdi,[rbx+20]
       vmovdqu   ymm0,ymmword ptr [rsi]
       vmovdqu   ymmword ptr [rsp+20],ymm0
       vmovdqu   ymm0,ymmword ptr [rsi+20]
       vmovdqu   ymmword ptr [rsp+40],ymm0
       mov       rcx,7FF7C6CEB8A0
       cmp       [rdi+18],rcx
       jne       short M00_L03
M00_L00:
       mov       rcx,[rbx+18]
       mov       rdx,rsi
       cmp       [rcx],ecx
       call      qword ptr [7FF7C6D8DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M00_L01:
       cmp       byte ptr [rbx+10],0
       jne       short M00_L04
M00_L02:
       vzeroupper
       add       rsp,60
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L03:
       lea       rdx,[rsp+20]
       mov       rcx,[rdi+8]
       call      qword ptr [rdi+18]
       test      eax,eax
       je        short M00_L01
       jmp       short M00_L00
M00_L04:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       rdx,rsi
       call      qword ptr [7FF7C6D8DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       jmp       short M00_L02
; Total bytes of code 126
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M01_L00:
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,68
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rbp,offset MT_Relay.FilterSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]]
       cmp       rdi,rbp
       jne       near ptr M01_L07
       mov       r14,[rbx+20]
       vmovdqu   ymm0,ymmword ptr [rsi]
       vmovdqu   ymmword ptr [rsp+28],ymm0
       vmovdqu   ymm0,ymmword ptr [rsi+20]
       vmovdqu   ymmword ptr [rsp+48],ymm0
       mov       rcx,7FF7C6CEB8A0
       cmp       [r14+18],rcx
       jne       near ptr M01_L08
M01_L01:
       mov       r15,[rbx+18]
       cmp       [r15],rbp
       jne       short M01_L06
       mov       rcx,r15
       mov       rdx,rsi
       call      qword ptr [7FF7C6D61D98]; Relay.FilterSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Accept(Relay.Benchmarks.Entry64 ByRef)
M01_L02:
       test      eax,eax
       je        short M01_L09
       cmp       byte ptr [r15+10],0
       jne       short M01_L09
M01_L03:
       mov       ecx,1
M01_L04:
       test      ecx,ecx
       je        near ptr M01_L10
       cmp       byte ptr [rbx+10],0
       jne       short M01_L10
M01_L05:
       vzeroupper
       add       rsp,68
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
M01_L06:
       mov       rcx,r15
       mov       rax,[r15]
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M01_L09
       mov       rcx,r15
       mov       rdx,rsi
       call      qword ptr [rdi+28]
       jmp       short M01_L02
M01_L07:
       mov       rcx,rbx
       mov       r14,[rdi+40]
       call      qword ptr [r14+20]
       test      eax,eax
       je        short M01_L10
       mov       rcx,rbx
       mov       rdx,rsi
       call      qword ptr [r14+28]
       mov       ecx,eax
       jmp       short M01_L04
M01_L08:
       lea       rdx,[rsp+28]
       mov       rcx,[r14+8]
       call      qword ptr [r14+18]
       test      eax,eax
       je        short M01_L03
       jmp       near ptr M01_L01
M01_L09:
       mov       rcx,[r15+8]
       test      rcx,rcx
       je        short M01_L03
       mov       rdx,rsi
       call      qword ptr [7FF7C6D8DFB0]
       jmp       near ptr M01_L03
M01_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M01_L05
       mov       rdx,rsi
       call      qword ptr [7FF7C6D8DFB0]
       nop
       vzeroupper
       add       rsp,68
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
; Total bytes of code 289
```
```assembly
; Relay.FilterSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Accept(Relay.Benchmarks.Entry64 ByRef)
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,0A0
       mov       rsi,rcx
       mov       rbx,rdx
       mov       rdi,[rsi+20]
       vmovdqu   ymm0,ymmword ptr [rbx]
       vmovdqu   ymmword ptr [rsp+60],ymm0
       vmovdqu   ymm0,ymmword ptr [rbx+20]
       vmovdqu   ymmword ptr [rsp+80],ymm0
       mov       rcx,7FF7C6CEB8A0
       cmp       [rdi+18],rcx
       jne       near ptr M02_L06
M02_L00:
       mov       rbp,[rsi+18]
       mov       rdi,[rbp]
       mov       rcx,offset MT_Relay.FilterSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]]
       cmp       rdi,rcx
       jne       short M02_L05
       mov       r14,[rbp+20]
       vmovdqu   ymm0,ymmword ptr [rbx]
       vmovdqu   ymmword ptr [rsp+20],ymm0
       vmovdqu   ymm0,ymmword ptr [rbx+20]
       vmovdqu   ymmword ptr [rsp+40],ymm0
       mov       rcx,7FF7C6CEB8A0
       cmp       [r14+18],rcx
       jne       short M02_L07
M02_L01:
       mov       rcx,[rbp+18]
       mov       rdx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FF7C6D8DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M02_L02:
       mov       eax,1
M02_L03:
       test      eax,eax
       je        short M02_L08
       cmp       byte ptr [rbp+10],0
       jne       short M02_L08
M02_L04:
       mov       eax,1
       vzeroupper
       add       rsp,0A0
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M02_L05:
       mov       rcx,rbp
       mov       rsi,[rdi+40]
       call      qword ptr [rsi+20]
       test      eax,eax
       je        short M02_L08
       mov       rcx,rbp
       mov       rdx,rbx
       call      qword ptr [rsi+28]
       jmp       short M02_L03
M02_L06:
       lea       rdx,[rsp+60]
       mov       rcx,[rdi+8]
       call      qword ptr [rdi+18]
       test      eax,eax
       je        short M02_L04
       jmp       near ptr M02_L00
M02_L07:
       lea       rdx,[rsp+20]
       mov       rcx,[r14+8]
       call      qword ptr [r14+18]
       test      eax,eax
       je        short M02_L02
       jmp       short M02_L01
M02_L08:
       mov       rcx,[rbp+8]
       test      rcx,rcx
       je        short M02_L04
       mov       rdx,rbx
       call      qword ptr [7FF7C6D8DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       jmp       short M02_L04
; Total bytes of code 268
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.FilterSinkBenchmarks.Filter_Reject()
       push      rsi
       push      rbx
       sub       rsp,68
       mov       rbx,[rcx+10]
       lea       rsi,[rcx+18]
       mov       rax,[rbx+20]
       vmovdqu   ymm0,ymmword ptr [rsi]
       vmovdqu   ymmword ptr [rsp+28],ymm0
       vmovdqu   ymm0,ymmword ptr [rsi+20]
       vmovdqu   ymmword ptr [rsp+48],ymm0
       mov       rdx,7FF7C6CDB8B8
       cmp       [rax+18],rdx
       jne       short M00_L02
M00_L00:
       cmp       byte ptr [rbx+10],0
       jne       short M00_L03
M00_L01:
       vzeroupper
       add       rsp,68
       pop       rbx
       pop       rsi
       ret
M00_L02:
       lea       rdx,[rsp+28]
       mov       rcx,[rax+8]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L00
       mov       rcx,[rbx+18]
       mov       rdx,rsi
       cmp       [rcx],ecx
       call      qword ptr [7FF7C6D7DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       jmp       short M00_L00
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L01
       mov       rdx,rsi
       call      qword ptr [7FF7C6D7DFB0]; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
       jmp       short M00_L01
; Total bytes of code 124
```
```assembly
; Relay.DispatchSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]].Enqueue(Relay.Benchmarks.Entry64 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,60
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rdx,offset MT_Relay.FilterSink`1[[Relay.Benchmarks.Entry64, Relay.Benchmarks]]
       cmp       rdi,rdx
       jne       near ptr M01_L06
       mov       rax,[rbx+20]
       vmovdqu   ymm0,ymmword ptr [rsi]
       vmovdqu   ymmword ptr [rsp+20],ymm0
       vmovdqu   ymm0,ymmword ptr [rsi+20]
       vmovdqu   ymmword ptr [rsp+40],ymm0
       mov       rdx,7FF7C6CDB8B8
       cmp       [rax+18],rdx
       jne       short M01_L04
M01_L01:
       mov       edx,1
M01_L02:
       test      edx,edx
       je        near ptr M01_L07
       cmp       byte ptr [rbx+10],0
       jne       near ptr M01_L07
M01_L03:
       vzeroupper
       add       rsp,60
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L04:
       lea       rdx,[rsp+20]
       mov       rcx,[rax+8]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M01_L01
       mov       rdi,[rbx+18]
       mov       rcx,rdi
       mov       rax,[rdi]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L05
       mov       rcx,rdi
       mov       rdx,rsi
       mov       rax,[rdi]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L05
       cmp       byte ptr [rdi+10],0
       je        short M01_L01
M01_L05:
       mov       rcx,[rdi+8]
       test      rcx,rcx
       je        short M01_L01
       mov       rdx,rsi
       call      qword ptr [7FF7C6D7DFB0]
       jmp       short M01_L01
M01_L06:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L07
       mov       rcx,rbx
       mov       rdx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       mov       edx,eax
       jmp       near ptr M01_L02
M01_L07:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M01_L03
       mov       rdx,rsi
       call      qword ptr [7FF7C6D7DFB0]
       nop
       vzeroupper
       add       rsp,60
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 261
```

