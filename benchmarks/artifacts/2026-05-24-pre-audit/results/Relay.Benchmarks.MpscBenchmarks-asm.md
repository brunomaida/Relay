## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline()
       mov       rax,[rcx+18]
       lea       rdx,[rcx+28]
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
       mov       rax,[rcx+18]
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
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention()
       push      rax
       mov       rdx,[rcx+8]
       lea       r8,[rcx+28]
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r10,dword ptr [rdx+20]
       mov       r9,rax
       sub       r9,r10
       cmp       [rdx+0E8],r9
       jle       near ptr M00_L04
M00_L01:
       lea       r10,[rdx+68]
       lea       r9,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r10],r9
       mov       r10,[rsp]
       cmp       rax,r10
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r9,dword ptr [rdx+1C]
       and       r9,r10
       movsxd    r11,dword ptr [rdx+18]
       imul      r9,r11
       vmovdqu   ymm0,ymmword ptr [r8]
       vmovdqu   ymmword ptr [rax+r9+40],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+20]
       vmovdqu   ymmword ptr [rax+r9+60],ymm0
       mov       rax,[rdx+8]
       movsxd    r8,dword ptr [rdx+1C]
       and       r8,r10
       movsxd    rdx,dword ptr [rdx+18]
       imul      rdx,r8
       mov       dword ptr [rax+rdx],1
M00_L02:
       mov       rax,[rcx+8]
       mov       rcx,[rax+168]
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       cmp       dword ptr [rdx+r8],0
       je        short M00_L05
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       lea       rdx,[rdx+r8+40]
       cmp       [rdx],dl
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       vmovdqu   ymmword ptr [rdx+20],ymm0
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rdx+r8],r10d
       inc       rcx
       mov       [rax+168],rcx
       mov       eax,1
M00_L03:
       vzeroupper
       add       rsp,8
       ret
M00_L04:
       mov       r10,[rdx+168]
       mov       [rdx+0E8],r10
       cmp       r10,r9
       jg        near ptr M00_L01
       jmp       near ptr M00_L02
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 291
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full()
       push      rax
       mov       rdx,[rcx+10]
       add       rcx,28
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r8,dword ptr [rdx+20]
       mov       r10,rax
       sub       r10,r8
       cmp       [rdx+0E8],r10
       jg        short M00_L02
       mov       r8,[rdx+168]
       mov       [rdx+0E8],r8
       cmp       r8,r10
       jg        short M00_L02
       xor       eax,eax
M00_L01:
       vzeroupper
       add       rsp,8
       ret
M00_L02:
       lea       r8,[rdx+68]
       lea       r10,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r8],r10
       mov       r8,[rsp]
       cmp       rax,r8
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+1C]
       and       r10,r8
       movsxd    r9,dword ptr [rdx+18]
       imul      r10,r9
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10+40],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+60],ymm0
       mov       rax,[rdx+8]
       movsxd    rcx,dword ptr [rdx+1C]
       and       rcx,r8
       movsxd    rdx,dword ptr [rdx+18]
       imul      rcx,rdx
       mov       dword ptr [rax+rcx],1
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 164
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+168]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       cmp       dword ptr [rax+r8],0
       jne       short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       movsx     rax,byte ptr [rax+r8+40]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       lea       rax,[rax+r8+40]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rax],ymm0
       vmovdqu   ymmword ptr [rax+20],ymm0
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rax+r8],r10d
       inc       rdx
       mov       [rcx+168],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 145
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline()
       mov       rax,[rcx+18]
       lea       rdx,[rcx+28]
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
       mov       rax,[rcx+18]
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
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention()
       push      rax
       mov       rdx,[rcx+8]
       lea       r8,[rcx+28]
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r10,dword ptr [rdx+20]
       mov       r9,rax
       sub       r9,r10
       cmp       [rdx+0E8],r9
       jle       near ptr M00_L04
M00_L01:
       lea       r10,[rdx+68]
       lea       r9,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r10],r9
       mov       r10,[rsp]
       cmp       rax,r10
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r9,dword ptr [rdx+1C]
       and       r9,r10
       movsxd    r11,dword ptr [rdx+18]
       imul      r9,r11
       vmovdqu   ymm0,ymmword ptr [r8]
       vmovdqu   ymmword ptr [rax+r9+40],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+20]
       vmovdqu   ymmword ptr [rax+r9+60],ymm0
       mov       rax,[rdx+8]
       movsxd    r8,dword ptr [rdx+1C]
       and       r8,r10
       movsxd    rdx,dword ptr [rdx+18]
       imul      rdx,r8
       mov       dword ptr [rax+rdx],1
M00_L02:
       mov       rax,[rcx+8]
       mov       rcx,[rax+168]
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       cmp       dword ptr [rdx+r8],0
       je        short M00_L05
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       lea       rdx,[rdx+r8+40]
       cmp       [rdx],dl
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       vmovdqu   ymmword ptr [rdx+20],ymm0
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rdx+r8],r10d
       inc       rcx
       mov       [rax+168],rcx
       mov       eax,1
M00_L03:
       vzeroupper
       add       rsp,8
       ret
M00_L04:
       mov       r10,[rdx+168]
       mov       [rdx+0E8],r10
       cmp       r10,r9
       jg        near ptr M00_L01
       jmp       near ptr M00_L02
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 291
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full()
       push      rax
       mov       rdx,[rcx+10]
       add       rcx,28
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r8,dword ptr [rdx+20]
       mov       r10,rax
       sub       r10,r8
       cmp       [rdx+0E8],r10
       jg        short M00_L02
       mov       r8,[rdx+168]
       mov       [rdx+0E8],r8
       cmp       r8,r10
       jg        short M00_L02
       xor       eax,eax
M00_L01:
       vzeroupper
       add       rsp,8
       ret
M00_L02:
       lea       r8,[rdx+68]
       lea       r10,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r8],r10
       mov       r8,[rsp]
       cmp       rax,r8
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+1C]
       and       r10,r8
       movsxd    r9,dword ptr [rdx+18]
       imul      r10,r9
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10+40],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+60],ymm0
       mov       rax,[rdx+8]
       movsxd    rcx,dword ptr [rdx+1C]
       and       rcx,r8
       movsxd    rdx,dword ptr [rdx+18]
       imul      rcx,rdx
       mov       dword ptr [rax+rcx],1
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 164
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+168]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       cmp       dword ptr [rax+r8],0
       jne       short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       movsx     rax,byte ptr [rax+r8+40]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       lea       rax,[rax+r8+40]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rax],ymm0
       vmovdqu   ymmword ptr [rax+20],ymm0
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rax+r8],r10d
       inc       rdx
       mov       [rcx+168],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 145
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline()
       mov       rdx,[rcx+18]
       lea       r8,[rcx+28]
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
       mov       rax,[rcx+18]
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
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention()
       push      rax
       mov       rdx,[rcx+8]
       lea       r8,[rcx+28]
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r10,dword ptr [rdx+20]
       mov       r9,rax
       sub       r9,r10
       cmp       [rdx+0E8],r9
       jle       near ptr M00_L04
M00_L01:
       lea       r10,[rdx+68]
       lea       r9,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r10],r9
       mov       r10,[rsp]
       cmp       rax,r10
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r9,dword ptr [rdx+1C]
       and       r9,r10
       movsxd    r11,dword ptr [rdx+18]
       imul      r9,r11
       vmovdqu   ymm0,ymmword ptr [r8]
       vmovdqu   ymmword ptr [rax+r9+40],ymm0
       vmovdqu   ymm0,ymmword ptr [r8+20]
       vmovdqu   ymmword ptr [rax+r9+60],ymm0
       mov       rax,[rdx+8]
       movsxd    r8,dword ptr [rdx+1C]
       and       r8,r10
       movsxd    rdx,dword ptr [rdx+18]
       imul      rdx,r8
       mov       dword ptr [rax+rdx],1
M00_L02:
       mov       rax,[rcx+8]
       mov       rcx,[rax+168]
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       cmp       dword ptr [rdx+r8],0
       je        short M00_L05
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       lea       rdx,[rdx+r8+40]
       cmp       [rdx],dl
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       vmovdqu   ymmword ptr [rdx+20],ymm0
       mov       rdx,[rax+8]
       movsxd    r8,dword ptr [rax+1C]
       and       r8,rcx
       movsxd    r10,dword ptr [rax+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rdx+r8],r10d
       inc       rcx
       mov       [rax+168],rcx
       mov       eax,1
M00_L03:
       vzeroupper
       add       rsp,8
       ret
M00_L04:
       mov       r10,[rdx+168]
       mov       [rdx+0E8],r10
       cmp       r10,r9
       jg        near ptr M00_L01
       jmp       near ptr M00_L02
M00_L05:
       xor       eax,eax
       jmp       short M00_L03
; Total bytes of code 291
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full()
       push      rax
       mov       rdx,[rcx+10]
       add       rcx,28
       cmp       [rdx],dl
M00_L00:
       mov       rax,[rdx+68]
       movsxd    r8,dword ptr [rdx+20]
       mov       r10,rax
       sub       r10,r8
       cmp       [rdx+0E8],r10
       jg        short M00_L02
       mov       r8,[rdx+168]
       mov       [rdx+0E8],r8
       cmp       r8,r10
       jg        short M00_L02
       xor       eax,eax
M00_L01:
       vzeroupper
       add       rsp,8
       ret
M00_L02:
       lea       r8,[rdx+68]
       lea       r10,[rax+1]
       mov       [rsp],rax
       lock cmpxchg [r8],r10
       mov       r8,[rsp]
       cmp       rax,r8
       jne       short M00_L00
       mov       rax,[rdx+8]
       movsxd    r10,dword ptr [rdx+1C]
       and       r10,r8
       movsxd    r9,dword ptr [rdx+18]
       imul      r10,r9
       vmovdqu   ymm0,ymmword ptr [rcx]
       vmovdqu   ymmword ptr [rax+r10+40],ymm0
       vmovdqu   ymm0,ymmword ptr [rcx+20]
       vmovdqu   ymmword ptr [rax+r10+60],ymm0
       mov       rax,[rdx+8]
       movsxd    rcx,dword ptr [rdx+1C]
       and       rcx,r8
       movsxd    rdx,dword ptr [rdx+18]
       imul      rcx,rdx
       mov       dword ptr [rax+rcx],1
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 164
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+168]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       cmp       dword ptr [rax+r8],0
       jne       short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       movsx     rax,byte ptr [rax+r8+40]
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       lea       rax,[rax+r8+40]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rax],ymm0
       vmovdqu   ymmword ptr [rax+20],ymm0
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+1C]
       and       r8,rdx
       movsxd    r10,dword ptr [rcx+18]
       imul      r8,r10
       xor       r10d,r10d
       mov       [rax+r8],r10d
       inc       rdx
       mov       [rcx+168],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 145
```

