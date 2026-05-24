## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip()
       mov       rax,[rcx+8]
       lea       rdx,[rcx+30]
       mov       r8,[rax+160]
       movsxd    r10,dword ptr [rax+18]
       mov       r9,r8
       sub       r9,r10
       cmp       [rax+1E0],r9
       jle       short M00_L04
M00_L00:
       mov       r10,[rax+8]
       movsxd    r9,dword ptr [rax+10]
       and       r9,r8
       shl       r9,6
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [r10+r9],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [r10+r9+20],ymm0
       inc       r8
       mov       [rax+160],r8
M00_L01:
       mov       rax,[rcx+8]
       mov       rcx,[rax+60]
       cmp       rcx,[rax+0E0]
       jl        short M00_L02
       mov       rdx,[rax+160]
       mov       [rax+0E0],rdx
       cmp       rcx,rdx
       jge       short M00_L05
M00_L02:
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+10]
       and       r8,rcx
       shl       r8,6
       movsx     rdx,byte ptr [rdx+r8]
       inc       rcx
       mov       [rax+60],rcx
       mov       eax,1
M00_L03:
       vzeroupper
       ret
M00_L04:
       mov       r10,[rax+60]
       mov       [rax+1E0],r10
       cmp       r10,r9
       jle       short M00_L01
       jmp       near ptr M00_L00
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 178
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full()
       mov       rdx,[rcx+10]
       add       rcx,30
       mov       r8,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r10,r8
       sub       r10,rax
       cmp       [rdx+1E0],r10
       jg        short M00_L01
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       rax,r10
       jg        short M00_L01
       xor       eax,eax
M00_L00:
       vzeroupper
       ret
M00_L01:
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+10]
       and       r10,r8
       shl       r10,6
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+20],ymm0
       inc       r8
       mov       [rdx+160],r8
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 110
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rdx,[rcx+8]
       mov       rax,[rcx+18]
       test      rax,rax
       je        near ptr M00_L10
       lea       r8,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rdx],dl
       test      r10d,r10d
       je        near ptr M00_L04
       mov       rax,[rdx+160]
       movsxd    r9,dword ptr [rdx+18]
       mov       r11,rax
       sub       r11,r9
       mov       r9d,r10d
       lea       r9,[r11+r9-1]
       cmp       [rdx+1E0],r9
       jg        short M00_L01
       mov       r9,[rdx+60]
       mov       [rdx+1E0],r9
       cmp       r9,r11
       jle       short M00_L04
       nop       dword ptr [rax]
M00_L01:
       mov       r9,[rdx+1E0]
       sub       r9,r11
       mov       r11d,r10d
       cmp       r9,r11
       cmovg     r9,r11
       xor       r11d,r11d
       test      r9d,r9d
       jle       short M00_L03
       nop       dword ptr [rax]
M00_L02:
       mov       rbx,[rdx+8]
       movsxd    rsi,r11d
       add       rsi,rax
       movsxd    rdi,dword ptr [rdx+10]
       and       rsi,rdi
       shl       rsi,6
       cmp       r11d,r10d
       jae       near ptr M00_L13
       mov       rdi,r11
       shl       rdi,6
       vmovdqu   ymm0,ymmword ptr [r8+rdi]
       vmovdqu   ymmword ptr [rbx+rsi],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+rdi+20]
       vmovdqu   ymmword ptr [rbx+rsi+20],ymm0
       inc       r11d
       cmp       r11d,r9d
       jl        short M00_L02
M00_L03:
       lock or   dword ptr [rsp],0
       movsxd    r8,r9d
       add       rax,r8
       mov       [rdx+160],rax
M00_L04:
       mov       r8,[rcx+8]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L11
       lea       r10,[rax+10]
       mov       r9d,[rax+8]
M00_L05:
       mov       rax,[r8+60]
       cmp       rax,[r8+0E0]
       jl        short M00_L06
       mov       rcx,[r8+160]
       mov       [r8+0E0],rcx
       cmp       rax,rcx
       jge       near ptr M00_L12
M00_L06:
       mov       rdx,[r8+0E0]
       sub       rdx,rax
       mov       ecx,r9d
       cmp       rdx,rcx
       cmovg     rdx,rcx
       xor       ecx,ecx
       test      edx,edx
       jle       short M00_L08
       xchg      ax,ax
M00_L07:
       cmp       ecx,r9d
       jae       short M00_L13
       mov       r11,rcx
       shl       r11,6
       mov       rbx,[r8+8]
       movsxd    rsi,ecx
       add       rsi,rax
       movsxd    rdi,dword ptr [r8+10]
       and       rsi,rdi
       shl       rsi,6
       vmovdqu   ymm0,ymmword ptr [rbx+rsi]
       vmovdqu   ymmword ptr [r10+r11],ymm0
       vmovdqu   ymm0,ymmword ptr [rbx+rsi+20]
       vmovdqu   ymmword ptr [r10+r11+20],ymm0
       inc       ecx
       cmp       ecx,edx
       jl        short M00_L07
M00_L08:
       movsxd    rcx,edx
       add       rax,rcx
       mov       [r8+60],rax
M00_L09:
       mov       eax,edx
       vzeroupper
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L10:
       xor       r8d,r8d
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L11:
       xor       r10d,r10d
       xor       r9d,r9d
       jmp       near ptr M00_L05
M00_L12:
       xor       edx,edx
       jmp       short M00_L09
M00_L13:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 422
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip()
       mov       rax,[rcx+8]
       lea       rdx,[rcx+30]
       mov       r8,[rax+160]
       movsxd    r10,dword ptr [rax+18]
       mov       r9,r8
       sub       r9,r10
       cmp       [rax+1E0],r9
       jle       short M00_L04
M00_L00:
       mov       r10,[rax+8]
       movsxd    r9,dword ptr [rax+10]
       and       r9,r8
       shl       r9,6
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [r10+r9],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [r10+r9+20],ymm0
       inc       r8
       mov       [rax+160],r8
M00_L01:
       mov       rax,[rcx+8]
       mov       rcx,[rax+60]
       cmp       rcx,[rax+0E0]
       jl        short M00_L02
       mov       rdx,[rax+160]
       mov       [rax+0E0],rdx
       cmp       rcx,rdx
       jge       short M00_L05
M00_L02:
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+10]
       and       r8,rcx
       shl       r8,6
       movsx     rdx,byte ptr [rdx+r8]
       inc       rcx
       mov       [rax+60],rcx
       mov       eax,1
M00_L03:
       vzeroupper
       ret
M00_L04:
       mov       r10,[rax+60]
       mov       [rax+1E0],r10
       cmp       r10,r9
       jle       short M00_L01
       jmp       near ptr M00_L00
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 178
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full()
       mov       rdx,[rcx+10]
       add       rcx,30
       mov       r8,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r10,r8
       sub       r10,rax
       cmp       [rdx+1E0],r10
       jg        short M00_L01
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       rax,r10
       jg        short M00_L01
       xor       eax,eax
M00_L00:
       vzeroupper
       ret
M00_L01:
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+10]
       and       r10,r8
       shl       r10,6
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+20],ymm0
       inc       r8
       mov       [rdx+160],r8
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 110
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rdx,[rcx+8]
       mov       rax,[rcx+18]
       test      rax,rax
       je        near ptr M00_L11
       lea       r8,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rdx],dl
       test      r10d,r10d
       je        near ptr M00_L04
       mov       r9,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r11,r9
       sub       r11,rax
       mov       eax,r10d
       lea       rax,[r11+rax-1]
       cmp       [rdx+1E0],rax
       jle       near ptr M00_L10
M00_L01:
       mov       rax,[rdx+1E0]
       sub       rax,r11
       mov       r11d,r10d
       cmp       rax,r11
       cmovg     rax,r11
       xor       r11d,r11d
       test      eax,eax
       jle       short M00_L03
M00_L02:
       mov       rbx,[rdx+8]
       movsxd    rsi,r11d
       add       rsi,r9
       movsxd    rdi,dword ptr [rdx+10]
       and       rsi,rdi
       shl       rsi,6
       cmp       r11d,r10d
       jae       near ptr M00_L14
       mov       rdi,r11
       shl       rdi,6
       vmovdqu   ymm0,ymmword ptr [r8+rdi]
       vmovdqu   ymmword ptr [rbx+rsi],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+rdi+20]
       vmovdqu   ymmword ptr [rbx+rsi+20],ymm0
       inc       r11d
       cmp       r11d,eax
       jl        short M00_L02
M00_L03:
       lock or   dword ptr [rsp],0
       cdqe
       add       rax,r9
       mov       [rdx+160],rax
M00_L04:
       mov       r11,[rcx+8]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L12
       lea       rbx,[rax+10]
       mov       esi,[rax+8]
M00_L05:
       mov       rax,[r11+60]
       cmp       rax,[r11+0E0]
       jl        short M00_L06
       mov       rdx,[r11+160]
       mov       [r11+0E0],rdx
       cmp       rax,rdx
       jge       near ptr M00_L13
M00_L06:
       mov       r8,[r11+0E0]
       sub       r8,rax
       mov       r10d,esi
       cmp       r8,r10
       cmovg     r8,r10
       xor       r9d,r9d
       test      r8d,r8d
       jle       short M00_L08
M00_L07:
       cmp       r9d,esi
       jae       near ptr M00_L14
       mov       rcx,r9
       shl       rcx,6
       mov       rdx,[r11+8]
       movsxd    r10,r9d
       add       r10,rax
       movsxd    rdi,dword ptr [r11+10]
       and       r10,rdi
       shl       r10,6
       vmovdqu   ymm0,ymmword ptr [rdx+r10]
       vmovdqu   ymmword ptr [rbx+rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+r10+20]
       vmovdqu   ymmword ptr [rbx+rcx+20],ymm0
       inc       r9d
       cmp       r9d,r8d
       jl        short M00_L07
M00_L08:
       movsxd    rcx,r8d
       add       rax,rcx
       mov       [r11+60],rax
M00_L09:
       mov       eax,r8d
       vzeroupper
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L10:
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       rax,r11
       jle       near ptr M00_L04
       jmp       near ptr M00_L01
M00_L11:
       xor       r8d,r8d
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L12:
       xor       ebx,ebx
       xor       esi,esi
       jmp       near ptr M00_L05
M00_L13:
       xor       r8d,r8d
       jmp       short M00_L09
M00_L14:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 431
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip()
       mov       rdx,[rcx+8]
       lea       r8,[rcx+30]
       mov       r10,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r9,r10
       sub       r9,rax
       cmp       [rdx+1E0],r9
       jle       short M00_L04
M00_L00:
       mov       rax,[rdx+8]
       movsxd    r9,dword ptr [rdx+10]
       and       r9,r10
       shl       r9,6
       vmovdqu   ymm0,ymmword ptr [r8]
       vmovdqu   ymmword ptr [rax+r9],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+20]
       vmovdqu   ymmword ptr [rax+r9+20],ymm0
       inc       r10
       mov       [rdx+160],r10
M00_L01:
       mov       rax,[rcx+8]
       mov       rdx,[rax+60]
       cmp       rdx,[rax+0E0]
       jl        short M00_L02
       mov       rcx,[rax+160]
       mov       [rax+0E0],rcx
       cmp       rdx,rcx
       jge       short M00_L05
M00_L02:
       mov       r8,[rax+8]
       movsxd    rcx,dword ptr [rax+10]
       and       rcx,rdx
       shl       rcx,6
       movsx     rcx,byte ptr [r8+rcx]
       inc       rdx
       mov       [rax+60],rdx
       mov       eax,1
M00_L03:
       vzeroupper
       ret
M00_L04:
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       [rdx+1E0],r9
       jle       short M00_L01
       jmp       near ptr M00_L00
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 184
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full()
       mov       rdx,[rcx+10]
       add       rcx,30
       mov       r8,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r10,r8
       sub       r10,rax
       cmp       [rdx+1E0],r10
       jg        short M00_L01
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       rax,r10
       jg        short M00_L01
       xor       eax,eax
M00_L00:
       vzeroupper
       ret
M00_L01:
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+10]
       and       r10,r8
       shl       r10,6
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+20],ymm0
       inc       r8
       mov       [rdx+160],r8
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 110
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rdx,[rcx+8]
       mov       rax,[rcx+18]
       test      rax,rax
       je        near ptr M00_L11
       lea       r8,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rdx],dl
       test      r10d,r10d
       je        near ptr M00_L04
       mov       r9,[rdx+160]
       movsxd    rax,dword ptr [rdx+18]
       mov       r11,r9
       sub       r11,rax
       mov       eax,r10d
       lea       rax,[r11+rax-1]
       cmp       [rdx+1E0],rax
       jle       near ptr M00_L10
M00_L01:
       mov       rax,[rdx+1E0]
       sub       rax,r11
       mov       r11d,r10d
       cmp       rax,r11
       cmovg     rax,r11
       xor       r11d,r11d
       test      eax,eax
       jle       short M00_L03
M00_L02:
       mov       rbx,[rdx+8]
       movsxd    rsi,r11d
       add       rsi,r9
       movsxd    rdi,dword ptr [rdx+10]
       and       rsi,rdi
       shl       rsi,6
       cmp       r11d,r10d
       jae       near ptr M00_L14
       mov       rdi,r11
       shl       rdi,6
       vmovdqu   ymm0,ymmword ptr [r8+rdi]
       vmovdqu   ymmword ptr [rbx+rsi],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+rdi+20]
       vmovdqu   ymmword ptr [rbx+rsi+20],ymm0
       inc       r11d
       cmp       r11d,eax
       jl        short M00_L02
M00_L03:
       lock or   dword ptr [rsp],0
       cdqe
       add       rax,r9
       mov       [rdx+160],rax
M00_L04:
       mov       r11,[rcx+8]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L12
       lea       rbx,[rax+10]
       mov       esi,[rax+8]
M00_L05:
       mov       rax,[r11+60]
       cmp       rax,[r11+0E0]
       jl        short M00_L06
       mov       rdx,[r11+160]
       mov       [r11+0E0],rdx
       cmp       rax,rdx
       jge       near ptr M00_L13
M00_L06:
       mov       r8,[r11+0E0]
       sub       r8,rax
       mov       r10d,esi
       cmp       r8,r10
       cmovg     r8,r10
       xor       r9d,r9d
       test      r8d,r8d
       jle       short M00_L08
M00_L07:
       cmp       r9d,esi
       jae       near ptr M00_L14
       mov       rcx,r9
       shl       rcx,6
       mov       rdx,[r11+8]
       movsxd    r10,r9d
       add       r10,rax
       movsxd    rdi,dword ptr [r11+10]
       and       r10,rdi
       shl       r10,6
       vmovdqu   ymm0,ymmword ptr [rdx+r10]
       vmovdqu   ymmword ptr [rbx+rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+r10+20]
       vmovdqu   ymmword ptr [rbx+rcx+20],ymm0
       inc       r9d
       cmp       r9d,r8d
       jl        short M00_L07
M00_L08:
       movsxd    rcx,r8d
       add       rax,rcx
       mov       [r11+60],rax
M00_L09:
       mov       eax,r8d
       vzeroupper
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L10:
       mov       rax,[rdx+60]
       mov       [rdx+1E0],rax
       cmp       rax,r11
       jle       near ptr M00_L04
       jmp       near ptr M00_L01
M00_L11:
       xor       r8d,r8d
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L12:
       xor       ebx,ebx
       xor       esi,esi
       jmp       near ptr M00_L05
M00_L13:
       xor       r8d,r8d
       jmp       short M00_L09
M00_L14:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 431
```

