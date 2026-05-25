## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L08
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L08
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       ja        short M01_L03
       test      r8b,18
       je        short M01_L01
       mov       r8,[rdx]
       mov       [rcx],r8
       mov       rax,[rax-8]
       mov       [r10-8],rax
M01_L00:
       vzeroupper
       ret
M01_L01:
       test      r8b,4
       jne       short M01_L02
       test      r8,r8
       je        short M01_L00
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L00
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L00
M01_L02:
       mov       r8d,[rdx]
       mov       [rcx],r8d
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L00
M01_L03:
       cmp       r8,40
       jbe       short M01_L06
       cmp       r8,800
       ja        near ptr M01_L09
       cmp       r8,100
       jb        short M01_L04
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L07
M01_L06:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L07
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L07
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L07:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
       jmp       near ptr M01_L00
M01_L08:
       cmp       rcx,rdx
       jne       short M01_L09
       cmp       [rdx],dl
       jmp       near ptr M01_L00
M01_L09:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E126538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       cmp       ebp,[rsi+14]
       jg        near ptr M00_L07
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       edi,edi
       jmp       short M00_L00
M00_L02:
       mov       r14,[rsi+158]
       movsxd    rax,dword ptr [rsi+14]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       sub       rax,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       mov       r10d,[rsi+14]
       sub       r10d,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,ebp
       jl        short M00_L04
       movsxd    r8,ebp
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rsi+198]
       mov       rax,[rsi+58]
       mov       [r8+40],rax
       mov       r8,r14
       sub       r8,rax
       movsxd    rax,dword ptr [rsi+14]
       sub       rax,r8
       movsxd    r8,ebp
       cmp       rax,r8
       jl        short M00_L07
M00_L03:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L06
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,ebp
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L05
       lea       rax,[rsi+198]
       mov       r9,[rsi+58]
       mov       [rax+40],r9
       mov       rax,r14
       sub       rax,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,rax
       cmp       r9,r8
       jl        short M00_L07
M00_L05:
       movsxd    r8,r15d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+8]
       mov       rcx,[rax+58]
       mov       rdx,[rax+0D8]
       cmp       rcx,rdx
       jl        short M00_L08
       lea       r8,[rax+98]
       mov       rdx,[rax+158]
       mov       [r8+40],rdx
       cmp       rcx,rdx
       jge       short M00_L09
M00_L08:
       mov       r8,[rax+8]
       cmp       [r8],r8b
       add       r8,10
       mov       r10d,ecx
       and       r10d,[rax+10]
       movsxd    r9,r10d
       mov       r9d,[r8+r9]
       cmp       r9d,0FFFFFFFF
       jne       short M00_L10
       mov       r9d,[rax+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rcx,r10
       mov       [rax+58],rcx
       cmp       rcx,rdx
       jge       short M00_L09
       and       ecx,[rax+10]
       movsxd    rax,ecx
       mov       r9d,[r8+rax]
       jmp       short M00_L10
M00_L09:
       xor       eax,eax
       xor       ecx,ecx
       jmp       short M00_L11
M00_L10:
       add       r9d,3
       and       r9d,0FFFFFFFC
       lea       eax,[r9+4]
       mov       ecx,1
M00_L11:
       mov       rdx,[rbx+8]
       cdqe
       add       [rdx+58],rax
       movzx     eax,cl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
; Total bytes of code 436
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       cmp       edi,[rbx+14]
       jg        near ptr M00_L05
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       short M00_L00
M00_L02:
       mov       rbp,[rbx+158]
       movsxd    rax,dword ptr [rbx+14]
       mov       r8,rbp
       sub       r8,[rbx+1D8]
       sub       rax,r8
       mov       r14d,ebp
       and       r14d,[rbx+10]
       mov       r10d,[rbx+14]
       sub       r10d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,edi
       jl        short M00_L04
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L05
M00_L03:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L07
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,edi
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L06
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r8
       jge       short M00_L06
M00_L05:
       xor       eax,eax
       jmp       short M00_L08
M00_L06:
       movsxd    r8,r14d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L07:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
M00_L08:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 294
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       cmp       ebp,[rsi+14]
       jg        near ptr M00_L07
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       edi,edi
       jmp       short M00_L00
M00_L02:
       mov       r14,[rsi+158]
       movsxd    rax,dword ptr [rsi+14]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       sub       rax,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       mov       r10d,[rsi+14]
       sub       r10d,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,ebp
       jl        short M00_L04
       movsxd    r8,ebp
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rsi+198]
       mov       rax,[rsi+58]
       mov       [r8+40],rax
       mov       r8,r14
       sub       r8,rax
       movsxd    rax,dword ptr [rsi+14]
       sub       rax,r8
       movsxd    r8,ebp
       cmp       rax,r8
       jl        short M00_L07
M00_L03:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L06
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,ebp
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L05
       lea       rax,[rsi+198]
       mov       r9,[rsi+58]
       mov       [rax+40],r9
       mov       rax,r14
       sub       rax,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,rax
       cmp       r9,r8
       jl        short M00_L07
M00_L05:
       movsxd    r8,r15d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+8]
       mov       rcx,[rax+58]
       mov       rdx,[rax+0D8]
       cmp       rcx,rdx
       jl        short M00_L08
       lea       r8,[rax+98]
       mov       rdx,[rax+158]
       mov       [r8+40],rdx
       cmp       rcx,rdx
       jge       short M00_L09
M00_L08:
       mov       r8,[rax+8]
       cmp       [r8],r8b
       add       r8,10
       mov       r10d,ecx
       and       r10d,[rax+10]
       movsxd    r9,r10d
       mov       r9d,[r8+r9]
       cmp       r9d,0FFFFFFFF
       jne       short M00_L10
       mov       r9d,[rax+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rcx,r10
       mov       [rax+58],rcx
       cmp       rcx,rdx
       jge       short M00_L09
       and       ecx,[rax+10]
       movsxd    rax,ecx
       mov       r9d,[r8+rax]
       jmp       short M00_L10
M00_L09:
       xor       eax,eax
       xor       ecx,ecx
       jmp       short M00_L11
M00_L10:
       add       r9d,3
       and       r9d,0FFFFFFFC
       lea       eax,[r9+4]
       mov       ecx,1
M00_L11:
       mov       rdx,[rbx+8]
       cdqe
       add       [rdx+58],rax
       movzx     eax,cl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
; Total bytes of code 436
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       cmp       edi,[rbx+14]
       jg        near ptr M00_L05
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       short M00_L00
M00_L02:
       mov       rbp,[rbx+158]
       movsxd    rax,dword ptr [rbx+14]
       mov       r8,rbp
       sub       r8,[rbx+1D8]
       sub       rax,r8
       mov       r14d,ebp
       and       r14d,[rbx+10]
       mov       r10d,[rbx+14]
       sub       r10d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,edi
       jl        short M00_L04
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L05
M00_L03:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L07
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,edi
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L06
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r8
       jge       short M00_L06
M00_L05:
       xor       eax,eax
       jmp       short M00_L08
M00_L06:
       movsxd    r8,r14d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L07:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
M00_L08:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 294
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       cmp       ebp,[rsi+14]
       jg        near ptr M00_L07
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       edi,edi
       jmp       short M00_L00
M00_L02:
       mov       r14,[rsi+158]
       movsxd    rax,dword ptr [rsi+14]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       sub       rax,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       mov       r10d,[rsi+14]
       sub       r10d,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,ebp
       jl        short M00_L04
       movsxd    r8,ebp
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rsi+198]
       mov       rax,[rsi+58]
       mov       [r8+40],rax
       mov       r8,r14
       sub       r8,rax
       movsxd    rax,dword ptr [rsi+14]
       sub       rax,r8
       movsxd    r8,ebp
       cmp       rax,r8
       jl        short M00_L07
M00_L03:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L06
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,ebp
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L05
       lea       rax,[rsi+198]
       mov       r9,[rsi+58]
       mov       [rax+40],r9
       mov       rax,r14
       sub       rax,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,rax
       cmp       r9,r8
       jl        short M00_L07
M00_L05:
       movsxd    r8,r15d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+8]
       mov       rcx,[rax+58]
       mov       rdx,[rax+0D8]
       cmp       rcx,rdx
       jl        short M00_L08
       lea       r8,[rax+98]
       mov       rdx,[rax+158]
       mov       [r8+40],rdx
       cmp       rcx,rdx
       jge       short M00_L09
M00_L08:
       mov       r8,[rax+8]
       cmp       [r8],r8b
       add       r8,10
       mov       r10d,ecx
       and       r10d,[rax+10]
       movsxd    r9,r10d
       mov       r9d,[r8+r9]
       cmp       r9d,0FFFFFFFF
       jne       short M00_L10
       mov       r9d,[rax+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rcx,r10
       mov       [rax+58],rcx
       cmp       rcx,rdx
       jge       short M00_L09
       and       ecx,[rax+10]
       movsxd    rax,ecx
       mov       r9d,[r8+rax]
       jmp       short M00_L10
M00_L09:
       xor       eax,eax
       xor       ecx,ecx
       jmp       short M00_L11
M00_L10:
       add       r9d,3
       and       r9d,0FFFFFFFC
       lea       eax,[r9+4]
       mov       ecx,1
M00_L11:
       mov       rdx,[rbx+8]
       cdqe
       add       [rdx+58],rax
       movzx     eax,cl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
; Total bytes of code 436
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       cmp       edi,[rbx+14]
       jg        near ptr M00_L05
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       short M00_L00
M00_L02:
       mov       rbp,[rbx+158]
       movsxd    rax,dword ptr [rbx+14]
       mov       r8,rbp
       sub       r8,[rbx+1D8]
       sub       rax,r8
       mov       r14d,ebp
       and       r14d,[rbx+10]
       mov       r10d,[rbx+14]
       sub       r10d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,edi
       jl        short M00_L04
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L05
M00_L03:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L07
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,edi
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L06
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r8
       jge       short M00_L06
M00_L05:
       xor       eax,eax
       jmp       short M00_L08
M00_L06:
       movsxd    r8,r14d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L07:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
M00_L08:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 294
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L08
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L08
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       ja        short M01_L03
       test      r8b,18
       je        short M01_L01
       mov       r8,[rdx]
       mov       [rcx],r8
       mov       rax,[rax-8]
       mov       [r10-8],rax
M01_L00:
       vzeroupper
       ret
M01_L01:
       test      r8b,4
       jne       short M01_L02
       test      r8,r8
       je        short M01_L00
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L00
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L00
M01_L02:
       mov       r8d,[rdx]
       mov       [rcx],r8d
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L00
M01_L03:
       cmp       r8,40
       jbe       short M01_L06
       cmp       r8,800
       ja        near ptr M01_L09
       cmp       r8,100
       jb        short M01_L04
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L07
M01_L06:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L07
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L07
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L07:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
       jmp       near ptr M01_L00
M01_L08:
       cmp       rcx,rdx
       jne       short M01_L09
       cmp       [rdx],dl
       jmp       near ptr M01_L00
M01_L09:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E106538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L06
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L01
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L01:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L02:
       vzeroupper
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L10
       cmp       r8,100
       jb        short M01_L04
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        near ptr M01_L00
       jmp       near ptr M01_L01
M01_L06:
       test      r8b,18
       jne       short M01_L08
       test      r8b,4
       jne       short M01_L07
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L09:
       cmp       rcx,rdx
       jne       short M01_L10
       cmp       [rdx],dl
       jmp       near ptr M01_L02
M01_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E116538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 340
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L08
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L08
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L05
       cmp       r8,40
       jbe       short M01_L02
       cmp       r8,800
       ja        near ptr M01_L09
       cmp       r8,100
       jb        short M01_L00
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L00:
       mov       r9,r8
       shr       r9,6
M01_L01:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L01
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L03
M01_L02:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L03
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L03
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L03:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L04:
       vzeroupper
       ret
M01_L05:
       test      r8b,18
       jne       short M01_L07
       test      r8b,4
       jne       short M01_L06
       test      r8,r8
       je        short M01_L04
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L04
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L04
M01_L06:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L04
M01_L07:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M01_L04
M01_L08:
       cmp       rcx,rdx
       jne       short M01_L09
       cmp       [rdx],dl
       jmp       short M01_L04
M01_L09:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E0F6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 311
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       cmp       ebp,[rsi+14]
       jg        near ptr M00_L07
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       edi,edi
       jmp       short M00_L00
M00_L02:
       mov       r14,[rsi+158]
       movsxd    rax,dword ptr [rsi+14]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       sub       rax,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       mov       r10d,[rsi+14]
       sub       r10d,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,ebp
       jl        short M00_L04
       movsxd    r8,ebp
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rsi+198]
       mov       rax,[rsi+58]
       mov       [r8+40],rax
       mov       r8,r14
       sub       r8,rax
       movsxd    rax,dword ptr [rsi+14]
       sub       rax,r8
       movsxd    r8,ebp
       cmp       rax,r8
       jl        short M00_L07
M00_L03:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L06
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,ebp
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L05
       lea       rax,[rsi+198]
       mov       r9,[rsi+58]
       mov       [rax+40],r9
       mov       rax,r14
       sub       rax,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,rax
       cmp       r9,r8
       jl        short M00_L07
M00_L05:
       movsxd    r8,r15d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+8]
       mov       rcx,[rax+58]
       mov       rdx,[rax+0D8]
       cmp       rcx,rdx
       jl        short M00_L08
       lea       r8,[rax+98]
       mov       rdx,[rax+158]
       mov       [r8+40],rdx
       cmp       rcx,rdx
       jge       short M00_L09
M00_L08:
       mov       r8,[rax+8]
       cmp       [r8],r8b
       add       r8,10
       mov       r10d,ecx
       and       r10d,[rax+10]
       movsxd    r9,r10d
       mov       r9d,[r8+r9]
       cmp       r9d,0FFFFFFFF
       jne       short M00_L10
       mov       r9d,[rax+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rcx,r10
       mov       [rax+58],rcx
       cmp       rcx,rdx
       jge       short M00_L09
       and       ecx,[rax+10]
       movsxd    rax,ecx
       mov       r9d,[r8+rax]
       jmp       short M00_L10
M00_L09:
       xor       eax,eax
       xor       ecx,ecx
       jmp       short M00_L11
M00_L10:
       add       r9d,3
       and       r9d,0FFFFFFFC
       lea       eax,[r9+4]
       mov       ecx,1
M00_L11:
       mov       rdx,[rbx+8]
       cdqe
       add       [rdx+58],rax
       movzx     eax,cl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
; Total bytes of code 436
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        short M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       cmp       edi,[rbx+14]
       jg        near ptr M00_L05
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       short M00_L00
M00_L02:
       mov       rbp,[rbx+158]
       movsxd    rax,dword ptr [rbx+14]
       mov       r8,rbp
       sub       r8,[rbx+1D8]
       sub       rax,r8
       mov       r14d,ebp
       and       r14d,[rbx+10]
       mov       r10d,[rbx+14]
       sub       r10d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r10d,edi
       jl        short M00_L04
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L03
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L05
M00_L03:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L07
M00_L04:
       movsxd    r8,r10d
       movsxd    r9,edi
       add       r8,r9
       cmp       rax,r8
       jge       short M00_L06
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r8
       jge       short M00_L06
M00_L05:
       xor       eax,eax
       jmp       short M00_L08
M00_L06:
       movsxd    r8,r14d
       mov       dword ptr [rcx+r8],0FFFFFFFF
       movsxd    r8,r10d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L07:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
M00_L08:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 294
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L08
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L08
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       ja        short M01_L03
       test      r8b,18
       je        short M01_L01
       mov       r8,[rdx]
       mov       [rcx],r8
       mov       rax,[rax-8]
       mov       [r10-8],rax
M01_L00:
       vzeroupper
       ret
M01_L01:
       test      r8b,4
       jne       short M01_L02
       test      r8,r8
       je        short M01_L00
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L00
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L00
M01_L02:
       mov       r8d,[rdx]
       mov       [rcx],r8d
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L00
M01_L03:
       cmp       r8,40
       jbe       short M01_L06
       cmp       r8,800
       ja        near ptr M01_L09
       cmp       r8,100
       jb        short M01_L04
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L07
M01_L06:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L07
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L07
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L07:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
       jmp       near ptr M01_L00
M01_L08:
       cmp       rcx,rdx
       jne       short M01_L09
       cmp       [rdx],dl
       jmp       near ptr M01_L00
M01_L09:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E0F6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L06
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L01
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M01_L01:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L02:
       vzeroupper
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L10
       cmp       r8,100
       jb        short M01_L04
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        near ptr M01_L00
       jmp       near ptr M01_L01
M01_L06:
       test      r8b,18
       jne       short M01_L08
       test      r8b,4
       jne       short M01_L07
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L09:
       cmp       rcx,rdx
       jne       short M01_L10
       cmp       [rdx],dl
       jmp       near ptr M01_L02
M01_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E126538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 340
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E1157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L06
       cmp       r8,40
       jbe       short M01_L02
       cmp       r8,800
       ja        near ptr M01_L10
       cmp       r8,100
       jb        short M01_L00
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L00:
       mov       r9,r8
       shr       r9,6
M01_L01:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L01
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L03
M01_L02:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L03
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       ja        short M01_L05
M01_L03:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L04:
       vzeroupper
       ret
M01_L05:
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
       jmp       short M01_L03
M01_L06:
       test      r8b,18
       jne       short M01_L08
       test      r8b,4
       jne       short M01_L07
       test      r8,r8
       je        short M01_L04
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L04
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L04
M01_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L04
M01_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M01_L04
M01_L09:
       cmp       rcx,rdx
       jne       short M01_L10
       cmp       [rdx],dl
       jmp       short M01_L04
M01_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E126538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 313
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+58]
       mov       r8,[rcx+0D8]
       cmp       rdx,r8
       jl        short M00_L02
       lea       rax,[rcx+98]
       mov       r8,[rcx+158]
       mov       [rax+40],r8
       cmp       rdx,r8
       jl        short M00_L02
M00_L00:
       xor       eax,eax
M00_L01:
       ret
M00_L02:
       mov       rax,[rcx+8]
       cmp       [rax],al
       add       rax,10
       mov       r10d,edx
       and       r10d,[rcx+10]
       movsxd    r9,r10d
       cmp       dword ptr [rax+r9],0FFFFFFFF
       jne       short M00_L03
       mov       r9d,[rcx+14]
       sub       r9d,r10d
       movsxd    r10,r9d
       add       rdx,r10
       mov       [rcx+58],rdx
       cmp       rdx,r8
       jge       short M00_L00
       and       edx,[rcx+10]
       movsxd    rcx,edx
       mov       eax,[rax+rcx]
M00_L03:
       mov       eax,1
       jmp       short M00_L01
; Total bytes of code 111
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       r8,[rbx+18]
       test      r8,r8
       je        near ptr M00_L07
       lea       rdx,[r8+10]
       mov       edi,[r8+8]
M00_L00:
       lea       ebp,[rdi+3]
       and       ebp,0FFFFFFFC
       add       ebp,4
       mov       eax,[rsi+14]
       cmp       ebp,eax
       jg        short M00_L03
       mov       r14,[rsi+158]
       mov       r8,r14
       sub       r8,[rsi+1D8]
       movsxd    r10,eax
       sub       r10,r8
       mov       r15d,r14d
       and       r15d,[rsi+10]
       sub       eax,r15d
       mov       rcx,[rsi+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       eax,ebp
       jl        near ptr M00_L08
       movsxd    r8,ebp
       cmp       r10,r8
       jl        near ptr M00_L11
M00_L01:
       movsxd    r8,r15d
       mov       [rcx+r8],edi
       add       r15d,4
       movsxd    r8,r15d
       add       rcx,r8
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+8]
       mov       r13,[r15+58]
       mov       r12,[r15+0D8]
       cmp       r13,r12
       jl        short M00_L04
       lea       rax,[r15+98]
       mov       r12,[r15+158]
       mov       [rax+40],r12
       cmp       r13,r12
       jge       near ptr M00_L12
M00_L04:
       mov       r10,[r15+8]
       cmp       [r10],r10b
       add       r10,10
       mov       r9d,r13d
       and       r9d,[r15+10]
       movsxd    rax,r9d
       mov       r11d,[r10+rax]
       cmp       r11d,0FFFFFFFF
       je        near ptr M00_L10
M00_L05:
       add       r11d,3
       and       r11d,0FFFFFFFC
       add       r11d,4
       mov       edx,1
M00_L06:
       mov       rax,[rbx+8]
       movsxd    r8,r11d
       add       [rax+58],r8
       movzx     eax,dl
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L07:
       xor       edx,edx
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L08:
       movsxd    r8,eax
       movsxd    rax,ebp
       add       rax,r8
       cmp       r10,rax
       jge       short M00_L09
       lea       r10,[rsi+198]
       mov       r9,[rsi+58]
       mov       [r10+40],r9
       mov       r10,r14
       sub       r10,r9
       movsxd    r9,dword ptr [rsi+14]
       sub       r9,r10
       cmp       r9,rax
       jl        near ptr M00_L03
M00_L09:
       movsxd    rax,r15d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       add       r14,r8
       mov       [rcx],edi
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E1257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       near ptr M00_L02
M00_L10:
       mov       eax,[r15+14]
       sub       eax,r9d
       cdqe
       add       r13,rax
       mov       [r15+58],r13
       cmp       r13,r12
       jge       short M00_L12
       mov       eax,r13d
       and       eax,[r15+10]
       cdqe
       mov       r11d,[r10+rax]
       jmp       near ptr M00_L05
M00_L11:
       lea       rax,[rsi+198]
       mov       r8,[rsi+58]
       mov       [rax+40],r8
       mov       rax,r14
       sub       rax,r8
       movsxd    r8,dword ptr [rsi+14]
       sub       r8,rax
       movsxd    rax,ebp
       cmp       r8,rax
       jl        near ptr M00_L03
       jmp       near ptr M00_L01
M00_L12:
       xor       r11d,r11d
       xor       edx,edx
       jmp       near ptr M00_L06
; Total bytes of code 479
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L06
       cmp       r8,40
       jbe       short M01_L02
       cmp       r8,800
       ja        near ptr M01_L10
       cmp       r8,100
       jb        short M01_L00
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
M01_L00:
       mov       r9,r8
       shr       r9,6
M01_L01:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L01
       and       r8,3F
       cmp       r8,10
       jbe       short M01_L03
M01_L02:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L03
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       ja        short M01_L05
M01_L03:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L04:
       vzeroupper
       ret
M01_L05:
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
       jmp       short M01_L03
M01_L06:
       test      r8b,18
       jne       short M01_L08
       test      r8b,4
       jne       short M01_L07
       test      r8,r8
       je        short M01_L04
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L04
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L04
M01_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L04
M01_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M01_L04
M01_L09:
       cmp       rcx,rdx
       jne       short M01_L10
       cmp       [rdx],dl
       jmp       short M01_L04
M01_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E126538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 313
```
```assembly
; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,0A8
       lea       rbp,[rsp+0E0]
       mov       [rbp-40],rcx
       mov       [rbp-48],rdx
       mov       [rbp-0A8],rcx
       mov       [rbp-0B0],rdx
       mov       [rbp-0B8],r8
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAF500CB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAF4FE1140]; CORINFO_HELP_JIT_PINVOKE_END
       xor       eax,eax
       mov       [rbp-48],rax
       mov       [rbp-40],rax
       add       rsp,0A8
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
; Total bytes of code 142
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+10]
       mov       r8,[rcx+18]
       test      r8,r8
       je        near ptr M00_L01
       lea       rdx,[r8+10]
       mov       esi,[r8+8]
M00_L00:
       lea       edi,[rsi+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       r8d,[rbx+14]
       cmp       edi,r8d
       jg        near ptr M00_L06
       mov       rbp,[rbx+158]
       mov       rcx,rbp
       sub       rcx,[rbx+1D8]
       movsxd    rax,r8d
       sub       rax,rcx
       mov       r14d,ebp
       and       r14d,[rbx+10]
       sub       r8d,r14d
       mov       rcx,[rbx+8]
       cmp       [rcx],cl
       add       rcx,10
       cmp       r8d,edi
       jge       short M00_L03
       movsxd    r10,r8d
       movsxd    r9,edi
       add       r10,r9
       cmp       rax,r10
       jge       short M00_L02
       lea       rax,[rbx+198]
       mov       r9,[rbx+58]
       mov       [rax+40],r9
       mov       rax,rbp
       sub       rax,r9
       movsxd    r9,dword ptr [rbx+14]
       sub       r9,rax
       cmp       r9,r10
       jl        near ptr M00_L06
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       esi,esi
       jmp       near ptr M00_L00
M00_L02:
       movsxd    rax,r14d
       mov       dword ptr [rcx+rax],0FFFFFFFF
       movsxd    r8,r8d
       add       rbp,r8
       mov       [rcx],esi
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       jmp       short M00_L05
M00_L03:
       movsxd    r8,edi
       cmp       rax,r8
       jge       short M00_L04
       lea       r8,[rbx+198]
       mov       rax,[rbx+58]
       mov       [r8+40],rax
       mov       r8,rbp
       sub       r8,rax
       movsxd    rax,dword ptr [rbx+14]
       sub       rax,r8
       movsxd    r8,edi
       cmp       rax,r8
       jl        short M00_L06
M00_L04:
       movsxd    r8,r14d
       mov       [rcx+r8],esi
       add       r14d,4
       movsxd    r8,r14d
       add       rcx,r8
       mov       r8d,esi
       call      qword ptr [7FFB0E0F57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L05:
       movsxd    rax,edi
       add       rax,rbp
       mov       [rbx+158],rax
       mov       eax,1
       jmp       short M00_L07
M00_L06:
       xor       eax,eax
M00_L07:
       add       rsp,20
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
; Total bytes of code 303
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M01_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M01_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M01_L08
       cmp       r8,40
       ja        short M01_L03
M01_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M01_L07
M01_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M01_L02:
       ret
M01_L03:
       cmp       r8,800
       ja        near ptr M01_L12
       cmp       r8,100
       jae       short M01_L06
M01_L04:
       mov       r9,r8
       shr       r9,6
M01_L05:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M01_L05
       and       r8,3F
       cmp       r8,10
       ja        short M01_L00
       jmp       short M01_L01
M01_L06:
       mov       r9,rcx
       and       r9,3F
       neg       r9
       add       r9,40
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       movups    xmm0,[rdx+30]
       movups    [rcx+30],xmm0
       add       rdx,r9
       add       rcx,r9
       sub       r8,r9
       jmp       short M01_L04
M01_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M01_L01
M01_L08:
       test      r8b,18
       jne       short M01_L10
       test      r8b,4
       jne       short M01_L09
       test      r8,r8
       je        near ptr M01_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M01_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M01_L02
M01_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M01_L02
M01_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M01_L02
M01_L11:
       cmp       rcx,rdx
       je        short M01_L13
M01_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FFAF4FF1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

