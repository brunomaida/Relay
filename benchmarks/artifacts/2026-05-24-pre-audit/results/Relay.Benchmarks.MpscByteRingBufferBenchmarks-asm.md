## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0B6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E48DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E48FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E0A6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E48DEA8]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E48FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E4ADEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E4ADED8]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E4ADEA8]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E4ADED8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+18]
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
       mov       rdx,[rbx+18]
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       vmovdqa   xmmword ptr [rsp+40],xmm4
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       edi,7FFFFFFE
       ja        near ptr M00_L10
       lea       r8d,[rdi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rsi+14]
       jg        near ptr M00_L10
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rsi+58]
       mov       [rsp+30],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       ecx,[rsi+14]
       sub       ecx,ebp
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    r14,dword ptr [rsi+14]
       sub       rcx,r14
       cmp       [rsi+0D8],rcx
       jg        short M00_L05
       mov       r14,[rsi+158]
       mov       [rsi+0D8],r14
       cmp       r14,rcx
       jle       near ptr M00_L10
M00_L05:
       lea       rcx,[rsi+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+30]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       or        edi,80000000
       mov       [r8+10],edi
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rcx,ebp
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L10
M00_L06:
       mov       r8,[rsi+8]
       lea       ecx,[rbp+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L16
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       near ptr M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       edi,r10d
       ja        short M00_L09
       mov       r8d,edi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    rcx,ebp
       mov       edx,edi
       or        edx,80000000
       mov       [rax+rcx+10],edx
       jmp       short M00_L10
M00_L09:
       call      qword ptr [7FFB0E49FCF0]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       mov       r8,[rcx+158]
       mov       esi,r8d
       and       esi,[rcx+10]
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,esi
       mov       eax,[rdx+rax+10]
       test      eax,80000000
       jne       short M00_L11
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+48],eax
       jmp       near ptr M00_L15
M00_L11:
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       jne       short M00_L12
       mov       edx,[rcx+14]
       sub       edx,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,esi
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+48]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E49DE60]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L15
M00_L12:
       lea       edi,[rax+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M00_L13
       mov       ecx,esi
       mov       r8d,eax
       add       rcx,r8
       mov       r8d,[rdx+8]
       cmp       rcx,r8
       ja        short M00_L16
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L14
M00_L13:
       or        esi,eax
       jne       short M00_L16
       xor       edx,edx
       xor       eax,eax
M00_L14:
       mov       [rsp+20],rdx
       mov       [rsp+28],eax
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E49DEA8]
       add       edi,4
       mov       [rsp+48],edi
       mov       eax,1
M00_L15:
       mov       rcx,[rbx+8]
       mov       edx,[rsp+48]
       mov       r8,[rcx+158]
       mov       r10d,r8d
       and       r10d,[rcx+10]
       mov       r9,[rcx+8]
       cmp       [r9],r9b
       movsxd    r10,r10d
       xor       r11d,r11d
       mov       [r9+r10+10],r11d
       movsxd    rdx,edx
       add       rdx,r8
       mov       [rcx+158],rdx
       movzx     eax,al
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L16:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 685
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M02_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M02_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E49DE60]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M02_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M02_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M02_L04
M02_L03:
       or        esi,r9d
       jne       short M02_L05
       xor       edx,edx
       xor       r9d,r9d
M02_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E49DEA8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L12
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L12
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       sub       ecx,edi
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    rbp,dword ptr [rbx+14]
       sub       rcx,rbp
       cmp       [rbx+0D8],rcx
       jg        short M00_L05
       mov       rbp,[rbx+158]
       mov       [rbx+0D8],rbp
       cmp       rbp,rcx
       jle       near ptr M00_L12
M00_L05:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L11
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L11
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       or        esi,80000000
       mov       [r8+10],esi
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       movsxd    rcx,edi
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L09
M00_L06:
       mov       r8,[rbx+8]
       lea       ecx,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        short M00_L11
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       short M00_L11
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L10
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       edx,esi
       or        edx,80000000
       mov       [rax+rcx+10],edx
M00_L09:
       mov       eax,1
       jmp       short M00_L13
M00_L10:
       call      qword ptr [7FFB0E49FC90]
       int       3
M00_L11:
       call      qword ptr [7FFB0E2E77B0]
       int       3
M00_L12:
       xor       eax,eax
M00_L13:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 392
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E4ADE60]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E4ADE90]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E4ADE60]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E4ADE90]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+18]
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
       mov       rdx,[rbx+18]
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       vmovdqa   xmmword ptr [rsp+40],xmm4
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       edi,7FFFFFFE
       ja        near ptr M00_L10
       lea       r8d,[rdi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rsi+14]
       jg        near ptr M00_L10
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rsi+58]
       mov       [rsp+30],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       ecx,[rsi+14]
       sub       ecx,ebp
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    r14,dword ptr [rsi+14]
       sub       rcx,r14
       cmp       [rsi+0D8],rcx
       jg        short M00_L05
       mov       r14,[rsi+158]
       mov       [rsi+0D8],r14
       cmp       r14,rcx
       jle       near ptr M00_L10
M00_L05:
       lea       rcx,[rsi+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+30]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       or        edi,80000000
       mov       [r8+10],edi
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rcx,ebp
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L10
M00_L06:
       mov       r8,[rsi+8]
       lea       ecx,[rbp+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L16
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       near ptr M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       edi,r10d
       ja        short M00_L09
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    rcx,ebp
       mov       edx,edi
       or        edx,80000000
       mov       [rax+rcx+10],edx
       jmp       short M00_L10
M00_L09:
       call      qword ptr [7FFB0E47FCF0]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       mov       r8,[rcx+158]
       mov       esi,r8d
       and       esi,[rcx+10]
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,esi
       mov       eax,[rdx+rax+10]
       test      eax,80000000
       jne       short M00_L11
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+48],eax
       jmp       near ptr M00_L15
M00_L11:
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       jne       short M00_L12
       mov       edx,[rcx+14]
       sub       edx,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,esi
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+48]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E47DE60]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L15
M00_L12:
       lea       edi,[rax+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M00_L13
       mov       ecx,esi
       mov       r8d,eax
       add       rcx,r8
       mov       r8d,[rdx+8]
       cmp       rcx,r8
       ja        short M00_L16
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L14
M00_L13:
       or        esi,eax
       jne       short M00_L16
       xor       edx,edx
       xor       eax,eax
M00_L14:
       mov       [rsp+20],rdx
       mov       [rsp+28],eax
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E47DEA8]
       add       edi,4
       mov       [rsp+48],edi
       mov       eax,1
M00_L15:
       mov       rcx,[rbx+8]
       mov       edx,[rsp+48]
       mov       r8,[rcx+158]
       mov       r10d,r8d
       and       r10d,[rcx+10]
       mov       r9,[rcx+8]
       cmp       [r9],r9b
       movsxd    r10,r10d
       xor       r11d,r11d
       mov       [r9+r10+10],r11d
       movsxd    rdx,edx
       add       rdx,r8
       mov       [rcx+158],rdx
       movzx     eax,al
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L16:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 685
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M02_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M02_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E47DE60]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M02_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M02_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M02_L04
M02_L03:
       or        esi,r9d
       jne       short M02_L05
       xor       edx,edx
       xor       r9d,r9d
M02_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E47DEA8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L05:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L12
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L12
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       sub       ecx,edi
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    rbp,dword ptr [rbx+14]
       sub       rcx,rbp
       cmp       [rbx+0D8],rcx
       jg        short M00_L05
       mov       rbp,[rbx+158]
       mov       [rbx+0D8],rbp
       cmp       rbp,rcx
       jle       near ptr M00_L12
M00_L05:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L11
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L11
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       or        esi,80000000
       mov       [r8+10],esi
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       movsxd    rcx,edi
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L09
M00_L06:
       mov       r8,[rbx+8]
       lea       ecx,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        short M00_L11
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       short M00_L11
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L10
       mov       r8d,esi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       edx,esi
       or        edx,80000000
       mov       [rax+rcx+10],edx
M00_L09:
       mov       eax,1
       jmp       short M00_L13
M00_L10:
       call      qword ptr [7FFB0E48FC90]
       int       3
M00_L11:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M00_L12:
       xor       eax,eax
M00_L13:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 392
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E4ADE48]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E4ADE78]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E4ADE48]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E4ADE78]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+18]
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
       mov       rdx,[rbx+18]
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       vmovdqa   xmmword ptr [rsp+40],xmm4
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       edi,7FFFFFFE
       ja        near ptr M00_L10
       lea       r8d,[rdi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rsi+14]
       jg        near ptr M00_L10
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rsi+58]
       mov       [rsp+30],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       ecx,[rsi+14]
       sub       ecx,ebp
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    r14,dword ptr [rsi+14]
       sub       rcx,r14
       cmp       [rsi+0D8],rcx
       jg        short M00_L05
       mov       r14,[rsi+158]
       mov       [rsi+0D8],r14
       cmp       r14,rcx
       jle       near ptr M00_L10
M00_L05:
       lea       rcx,[rsi+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+30]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       or        edi,80000000
       mov       [r8+10],edi
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rcx,ebp
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L10
M00_L06:
       mov       r8,[rsi+8]
       lea       ecx,[rbp+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L16
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       near ptr M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       edi,r10d
       ja        short M00_L09
       mov       r8d,edi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    rcx,ebp
       mov       edx,edi
       or        edx,80000000
       mov       [rax+rcx+10],edx
       jmp       short M00_L10
M00_L09:
       call      qword ptr [7FFB0E4AFCF0]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       mov       r8,[rcx+158]
       mov       esi,r8d
       and       esi,[rcx+10]
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,esi
       mov       eax,[rdx+rax+10]
       test      eax,80000000
       jne       short M00_L11
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+48],eax
       jmp       near ptr M00_L15
M00_L11:
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       jne       short M00_L12
       mov       edx,[rcx+14]
       sub       edx,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,esi
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+48]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E4ADE78]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L15
M00_L12:
       lea       edi,[rax+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M00_L13
       mov       ecx,esi
       mov       r8d,eax
       add       rcx,r8
       mov       r8d,[rdx+8]
       cmp       rcx,r8
       ja        short M00_L16
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L14
M00_L13:
       or        esi,eax
       jne       short M00_L16
       xor       edx,edx
       xor       eax,eax
M00_L14:
       mov       [rsp+20],rdx
       mov       [rsp+28],eax
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E4ADEC0]
       add       edi,4
       mov       [rsp+48],edi
       mov       eax,1
M00_L15:
       mov       rcx,[rbx+8]
       mov       edx,[rsp+48]
       mov       r8,[rcx+158]
       mov       r10d,r8d
       and       r10d,[rcx+10]
       mov       r9,[rcx+8]
       cmp       [r9],r9b
       movsxd    r10,r10d
       xor       r11d,r11d
       mov       [r9+r10+10],r11d
       movsxd    rdx,edx
       add       rdx,r8
       mov       [rcx+158],rdx
       movzx     eax,al
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L16:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 685
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M02_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M02_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E4ADE78]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M02_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M02_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M02_L04
M02_L03:
       or        esi,r9d
       jne       short M02_L05
       xor       edx,edx
       xor       r9d,r9d
M02_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E4ADEC0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L12
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L12
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       sub       ecx,edi
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    rbp,dword ptr [rbx+14]
       sub       rcx,rbp
       cmp       [rbx+0D8],rcx
       jg        short M00_L05
       mov       rbp,[rbx+158]
       mov       [rbx+0D8],rbp
       cmp       rbp,rcx
       jle       near ptr M00_L12
M00_L05:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L11
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L11
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       or        esi,80000000
       mov       [r8+10],esi
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       movsxd    rcx,edi
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L09
M00_L06:
       mov       r8,[rbx+8]
       lea       ecx,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        short M00_L11
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       short M00_L11
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L10
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       edx,esi
       or        edx,80000000
       mov       [rax+rcx+10],edx
M00_L09:
       mov       eax,1
       jmp       short M00_L13
M00_L10:
       call      qword ptr [7FFB0E47FC90]
       int       3
M00_L11:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L12:
       xor       eax,eax
M00_L13:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 392
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E47DE78]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E47DEA8]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E47DE78]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E47DEA8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0C6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E47DE90]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E47FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E096538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E47DE90]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E49FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2E77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E48DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E48DED8]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E48DEA8]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E48DED8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0A6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E49DEC0]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E49FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2E77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E0B6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 340
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E49DEC0]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2E77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E47FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E49DDA0]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E49DDD0]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E49DDA0]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E49DDD0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jbe       near ptr M01_L04
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
       ja        short M01_L04
M01_L02:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M01_L03:
       vzeroupper
       ret
M01_L04:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M01_L02
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M01_L02
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
       jmp       short M01_L02
M01_L05:
       test      r8b,18
       jne       short M01_L07
       test      r8b,4
       jne       short M01_L06
       test      r8,r8
       je        short M01_L03
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M01_L03
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M01_L03
M01_L06:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M01_L03
M01_L07:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M01_L03
M01_L08:
       cmp       rcx,rdx
       jne       short M01_L09
       cmp       [rdx],dl
       jmp       short M01_L03
M01_L09:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E0B6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 317
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    r8,r11d
       add       r8,rax
       movsxd    rcx,r14d
       mov       r11,r8
       sub       r11,rcx
       cmp       [rsi+0D8],r11
       jg        short M00_L03
       mov       rcx,[rsi+158]
       mov       [rsi+0D8],rcx
       cmp       rcx,r11
       jle       short M00_L05
M00_L03:
       lea       rcx,[rsi+58]
       lock cmpxchg [rcx],r8
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L10
       mov       r8,[rsi+8]
       lea       r12d,[rbp+4]
       test      r8,r8
       je        near ptr M00_L12
       mov       ecx,r12d
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[r8+8]
       cmp       rcx,rax
       ja        near ptr M00_L15
       mov       ecx,r12d
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L13
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       r12,[rcx+158]
       mov       edi,r12d
       and       edi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,edi
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L16
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L11
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       edi,4
       test      rcx,rcx
       je        near ptr M00_L14
       mov       edx,edi
       mov       r10d,eax
       add       rdx,r10
       mov       r10d,[rcx+8]
       cmp       rdx,r10
       ja        near ptr M00_L15
       mov       edx,edi
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r10d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r9,[rax+8]
       cmp       [r9],r9b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r9+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r10b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L15
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L15
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L11:
       mov       r8d,[rcx+14]
       sub       r8d,edi
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,edi
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,r12
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E47DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r10d,eax
       jmp       near ptr M00_L07
M00_L12:
       or        r12d,r10d
       jne       short M00_L15
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L13:
       call      qword ptr [7FFB0E47FF18]
       int       3
M00_L14:
       or        edi,eax
       jne       short M00_L15
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L15:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L16:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r10d,r10d
       jmp       near ptr M00_L07
; Total bytes of code 746
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
       jmp       qword ptr [7FFB0E096538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 311
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E47DEA8]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E4AFC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2F77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E49DDA0]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E49DDD0]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E49DDA0]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E49DDD0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,rcx
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L06:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L07:
       mov       rax,[rbx+18]
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
       mov       rdx,[rbx+18]
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       vmovdqa   xmmword ptr [rsp+40],xmm4
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       edi,7FFFFFFE
       ja        near ptr M00_L10
       lea       r8d,[rdi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rsi+14]
       jg        near ptr M00_L10
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rsi+58]
       mov       [rsp+30],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       ecx,[rsi+14]
       sub       ecx,ebp
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    r14,dword ptr [rsi+14]
       sub       rcx,r14
       cmp       [rsi+0D8],rcx
       jg        short M00_L05
       mov       r14,[rsi+158]
       mov       [rsi+0D8],r14
       cmp       r14,rcx
       jle       near ptr M00_L10
M00_L05:
       lea       rcx,[rsi+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+30]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       or        edi,80000000
       mov       [r8+10],edi
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rcx,ebp
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L10
M00_L06:
       mov       r8,[rsi+8]
       lea       ecx,[rbp+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L16
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       near ptr M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       edi,r10d
       ja        short M00_L09
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    rcx,ebp
       mov       edx,edi
       or        edx,80000000
       mov       [rax+rcx+10],edx
       jmp       short M00_L10
M00_L09:
       call      qword ptr [7FFB0E47FCF0]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       mov       r8,[rcx+158]
       mov       esi,r8d
       and       esi,[rcx+10]
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,esi
       mov       eax,[rdx+rax+10]
       test      eax,80000000
       jne       short M00_L11
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+48],eax
       jmp       near ptr M00_L15
M00_L11:
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       jne       short M00_L12
       mov       edx,[rcx+14]
       sub       edx,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,esi
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+48]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E47DE60]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L15
M00_L12:
       lea       edi,[rax+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M00_L13
       mov       ecx,esi
       mov       r8d,eax
       add       rcx,r8
       mov       r8d,[rdx+8]
       cmp       rcx,r8
       ja        short M00_L16
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L14
M00_L13:
       or        esi,eax
       jne       short M00_L16
       xor       edx,edx
       xor       eax,eax
M00_L14:
       mov       [rsp+20],rdx
       mov       [rsp+28],eax
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E47DEA8]
       add       edi,4
       mov       [rsp+48],edi
       mov       eax,1
M00_L15:
       mov       rcx,[rbx+8]
       mov       edx,[rsp+48]
       mov       r8,[rcx+158]
       mov       r10d,r8d
       and       r10d,[rcx+10]
       mov       r9,[rcx+8]
       cmp       [r9],r9b
       movsxd    r10,r10d
       xor       r11d,r11d
       mov       [r9+r10+10],r11d
       movsxd    rdx,edx
       add       rdx,r8
       mov       [rcx+158],rdx
       movzx     eax,al
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L16:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 685
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M02_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M02_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E47DE60]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M02_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M02_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M02_L04
M02_L03:
       or        esi,r9d
       jne       short M02_L05
       xor       edx,edx
       xor       r9d,r9d
M02_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E47DEA8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L05:
       call      qword ptr [7FFB0E2C77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        short M00_L01
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L12
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L12
       jmp       short M00_L02
M00_L01:
       xor       edx,edx
       xor       r10d,r10d
       jmp       short M00_L00
M00_L02:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       sub       ecx,edi
       cmp       ecx,r8d
       jl        short M00_L03
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L04
M00_L03:
       lea       r9d,[rcx+r8]
       mov       r11d,1
M00_L04:
       movsxd    rcx,r9d
       add       rcx,rax
       movsxd    rbp,dword ptr [rbx+14]
       sub       rcx,rbp
       cmp       [rbx+0D8],rcx
       jg        short M00_L05
       mov       rbp,[rbx+158]
       mov       [rbx+0D8],rbp
       cmp       rbp,rcx
       jle       near ptr M00_L12
M00_L05:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       short M00_L02
       test      r11d,r11d
       je        short M00_L06
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L11
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L11
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       or        esi,80000000
       mov       [r8+10],esi
       mov       r8,[rbx+8]
       cmp       [r8],r8b
       movsxd    rcx,edi
       mov       dword ptr [r8+rcx+10],0FFFFFFFF
       jmp       short M00_L09
M00_L06:
       mov       r8,[rbx+8]
       lea       ecx,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       eax,ecx
       mov       r9d,r10d
       add       rax,r9
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        short M00_L11
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ecx,r10d
       jne       short M00_L11
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L10
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       edx,esi
       or        edx,80000000
       mov       [rax+rcx+10],edx
M00_L09:
       mov       eax,1
       jmp       short M00_L13
M00_L10:
       call      qword ptr [7FFB0E4AFC90]
       int       3
M00_L11:
       call      qword ptr [7FFB0E2F77B0]
       int       3
M00_L12:
       xor       eax,eax
M00_L13:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 392
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E48DE48]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E48DE78]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E48DE48]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E48DE78]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0857B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0857B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E086538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E47DE90]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E47FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E096538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 315
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E47DE90]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L05
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L05
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       short M00_L01
       test      r11d,r11d
       jne       short M00_L11
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L09
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
M00_L06:
       cmp       esi,r10d
       ja        short M00_L10
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
M00_L07:
       mov       eax,1
M00_L08:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L09:
       or        ebp,r10d
       jne       short M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       short M00_L06
M00_L10:
       call      qword ptr [7FFB0E4AFC90]
       int       3
M00_L11:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
       jmp       short M00_L07
M00_L12:
       call      qword ptr [7FFB0E2F77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       short M00_L08
; Total bytes of code 393
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E48DE90]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E48DEC0]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E48DE90]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E48DEC0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2D77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0C6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0C57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E4ADEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E4AFF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2F77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E0C6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 340
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E4ADEA8]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2F77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E47FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E4ADDA0]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E4ADDD0]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E4ADDA0]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E4ADDD0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2F77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0B6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0A57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E48DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E48FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E0A6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 313
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E48DEA8]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2D77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E47FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E49DDA0]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E49DDD0]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E49DDA0]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E49DDD0]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 256
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip()
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
       mov       rsi,[rbx+18]
       mov       r8,[rbx+20]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
M00_L02:
       movsxd    rax,ebp
       add       rax,r14
       mov       [rsi+158],rax
M00_L03:
       mov       r15,[rbx+18]
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
       mov       rax,[rbx+18]
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
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       jmp       qword ptr [7FFB0E0B6538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rbx,rcx
       mov       rsi,[rbx+8]
       mov       rax,[rbx+20]
       test      rax,rax
       je        near ptr M00_L08
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rsi],sil
       mov       edi,r10d
       cmp       r10d,7FFFFFFE
       ja        near ptr M00_L05
       lea       r9d,[r10+3]
       and       r9d,0FFFFFFFC
       add       r9d,4
       cmp       r9d,[rsi+14]
       jg        near ptr M00_L05
M00_L01:
       mov       rax,[rsi+58]
       mov       [rsp+28],rax
       mov       ebp,eax
       and       ebp,[rsi+10]
       mov       r14d,[rsi+14]
       mov       r15d,r14d
       sub       r15d,ebp
       cmp       r15d,r9d
       jl        near ptr M00_L09
       mov       r11d,r9d
       xor       r13d,r13d
M00_L02:
       movsxd    rcx,r11d
       add       rcx,rax
       movsxd    r8,r14d
       mov       r12,rcx
       sub       r12,r8
       cmp       [rsi+0D8],r12
       jle       near ptr M00_L10
M00_L03:
       lea       r8,[rsi+58]
       lock cmpxchg [r8],rcx
       cmp       rax,[rsp+28]
       jne       short M00_L01
       test      r13d,r13d
       jne       near ptr M00_L11
       mov       r8,[rsi+8]
       lea       eax,[rbp+4]
       test      r8,r8
       je        near ptr M00_L13
       mov       ecx,eax
       mov       r9d,r10d
       add       rcx,r9
       mov       r9d,[r8+8]
       cmp       rcx,r9
       ja        near ptr M00_L16
       mov       ecx,eax
       lea       rcx,[r8+rcx+10]
M00_L04:
       cmp       edi,r10d
       ja        near ptr M00_L14
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rsi+8]
       cmp       [rax],al
       movsxd    r8,ebp
       or        edi,80000000
       mov       [rax+r8+10],edi
M00_L05:
       mov       rcx,[rbx+8]
       mov       rdi,[rcx+158]
       mov       r10d,edi
       and       r10d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,r10d
       mov       eax,[rax+r8+10]
       test      eax,80000000
       je        near ptr M00_L17
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        near ptr M00_L12
       lea       r8d,[rax+3]
       and       r8d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r10d,4
       test      rcx,rcx
       je        near ptr M00_L15
       mov       edx,r10d
       mov       r9d,eax
       add       rdx,r9
       mov       r9d,[rcx+8]
       cmp       rdx,r9
       ja        near ptr M00_L16
       mov       edx,r10d
       lea       rcx,[rcx+rdx+10]
M00_L06:
       mov       [rsp+30],rcx
       mov       [rsp+38],eax
       add       r8d,4
       mov       [rsp+40],r8d
       mov       r9d,1
M00_L07:
       mov       rax,[rbx+8]
       mov       r8d,[rsp+40]
       mov       rcx,[rax+158]
       mov       edx,ecx
       and       edx,[rax+10]
       mov       r10,[rax+8]
       cmp       [r10],r10b
       movsxd    rdx,edx
       xor       r11d,r11d
       mov       [r10+rdx+10],r11d
       movsxd    r8,r8d
       add       r8,rcx
       mov       [rax+158],r8
       movzx     eax,r9b
       add       rsp,48
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L08:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L09:
       lea       r11d,[r15+r9]
       mov       r13d,1
       jmp       near ptr M00_L02
M00_L10:
       mov       r8,[rsi+158]
       mov       [rsi+0D8],r8
       cmp       r8,r12
       jle       near ptr M00_L05
       jmp       near ptr M00_L03
M00_L11:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M00_L16
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        near ptr M00_L16
       add       rcx,10
       add       rcx,4
       mov       r8d,edi
       call      qword ptr [7FFB0E0957B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       mov       edx,edi
       or        edx,80000000
       mov       [r8+10],edx
       mov       r8,[rsi+8]
       cmp       [r8],r8b
       movsxd    rdx,ebp
       mov       dword ptr [r8+rdx+10],0FFFFFFFF
       jmp       near ptr M00_L05
M00_L12:
       mov       r8d,[rcx+14]
       sub       r8d,r10d
       mov       rdx,[rcx+8]
       cmp       [rdx],dl
       movsxd    rax,r10d
       xor       r10d,r10d
       mov       [rdx+rax+10],r10d
       movsxd    r8,r8d
       add       r8,rdi
       mov       [rcx+158],r8
       lea       r8,[rsp+40]
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E47DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       mov       r9d,eax
       jmp       near ptr M00_L07
M00_L13:
       or        eax,r10d
       jne       short M00_L16
       xor       ecx,ecx
       xor       r10d,r10d
       jmp       near ptr M00_L04
M00_L14:
       call      qword ptr [7FFB0E47FF18]
       int       3
M00_L15:
       or        r10d,eax
       jne       short M00_L16
       xor       ecx,ecx
       xor       eax,eax
       jmp       near ptr M00_L06
M00_L16:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M00_L17:
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+30],xmm0
       xor       eax,eax
       mov       [rsp+40],eax
       xor       r9d,r9d
       jmp       near ptr M00_L07
; Total bytes of code 761
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
       jmp       qword ptr [7FFB0E096538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 313
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M02_L00:
       push      rbx
       sub       rsp,20
       mov       r10,[rcx+158]
       mov       r9d,r10d
       and       r9d,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r11,r9d
       mov       eax,[rax+r11+10]
       test      eax,80000000
       je        near ptr M02_L05
       and       eax,7FFFFFFF
       cmp       eax,7FFFFFFF
       je        short M02_L02
       lea       r10d,[rax+3]
       and       r10d,0FFFFFFFC
       mov       rcx,[rcx+8]
       add       r9d,4
       test      rcx,rcx
       je        short M02_L03
       mov       r11d,r9d
       mov       ebx,eax
       add       r11,rbx
       mov       ebx,[rcx+8]
       cmp       r11,rbx
       ja        short M02_L04
       lea       rbx,[rcx+r9+10]
M02_L01:
       mov       [rdx],rbx
       mov       [rdx+8],eax
       add       r10d,4
       mov       [r8],r10d
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M02_L02:
       mov       eax,[rcx+14]
       sub       eax,r9d
       mov       r11,[rcx+8]
       cmp       [r11],r11b
       movsxd    r9,r9d
       xor       ebx,ebx
       mov       [r11+r9+10],ebx
       cdqe
       add       rax,r10
       mov       [rcx+158],rax
       call      qword ptr [7FFB0E47DEA8]
       nop
       add       rsp,20
       pop       rbx
       ret
M02_L03:
       or        r9d,eax
       jne       short M02_L04
       xor       ebx,ebx
       xor       eax,eax
       jmp       short M02_L01
M02_L04:
       call      qword ptr [7FFB0E2C77B0]
       int       3
M02_L05:
       xor       eax,eax
       mov       [rdx],rax
       mov       [rdx+8],rax
       mov       [r8],eax
       add       rsp,20
       pop       rbx
       ret
; Total bytes of code 206
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
       call      qword ptr [7FFAE1EA1138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FFAE1ECCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FFAE1EA1140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,28
       mov       rbx,[rcx+10]
       mov       rax,[rcx+20]
       test      rax,rax
       je        near ptr M00_L03
       lea       rdx,[rax+10]
       mov       r10d,[rax+8]
M00_L00:
       cmp       [rbx],bl
       mov       esi,r10d
       cmp       esi,7FFFFFFE
       ja        near ptr M00_L13
       lea       r8d,[rsi+3]
       and       r8d,0FFFFFFFC
       add       r8d,4
       cmp       r8d,[rbx+14]
       jg        near ptr M00_L13
M00_L01:
       mov       rax,[rbx+58]
       mov       [rsp+20],rax
       mov       edi,eax
       and       edi,[rbx+10]
       mov       ecx,[rbx+14]
       mov       r9d,ecx
       sub       r9d,edi
       cmp       r9d,r8d
       jge       short M00_L04
       add       r9d,r8d
       mov       r11d,1
M00_L02:
       movsxd    rbp,r9d
       add       rbp,rax
       movsxd    rcx,ecx
       sub       rbp,rcx
       cmp       [rbx+0D8],rbp
       jg        short M00_L06
       mov       rcx,[rbx+158]
       mov       [rbx+0D8],rcx
       cmp       rcx,rbp
       jle       near ptr M00_L13
       jmp       short M00_L06
M00_L03:
       xor       edx,edx
       xor       r10d,r10d
       jmp       near ptr M00_L00
M00_L04:
       mov       r9d,r8d
       xor       r11d,r11d
       jmp       short M00_L02
M00_L05:
       add       rsp,28
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L06:
       lea       rcx,[rbx+58]
       movsxd    r9,r9d
       add       r9,rax
       lock cmpxchg [rcx],r9
       cmp       rax,[rsp+20]
       jne       near ptr M00_L01
       test      r11d,r11d
       jne       short M00_L10
       mov       r8,[rbx+8]
       lea       ebp,[rdi+4]
       test      r8,r8
       je        short M00_L07
       mov       ecx,ebp
       mov       eax,r10d
       add       rax,rcx
       mov       r9d,[r8+8]
       cmp       rax,r9
       ja        near ptr M00_L12
       lea       rcx,[r8+rcx+10]
       jmp       short M00_L08
M00_L07:
       or        ebp,r10d
       jne       near ptr M00_L12
       xor       ecx,ecx
       xor       r10d,r10d
M00_L08:
       cmp       esi,r10d
       ja        short M00_L09
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       or        esi,80000000
       mov       [rax+rcx+10],esi
       jmp       short M00_L11
M00_L09:
       call      qword ptr [7FFB0E49FC90]
       int       3
M00_L10:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L12
       mov       r8d,[rcx+8]
       mov       eax,r10d
       add       rax,4
       cmp       r8,rax
       jb        short M00_L12
       add       rcx,10
       add       rcx,4
       mov       r8d,esi
       call      qword ptr [7FFB0E0B57B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,[rbx+8]
       cmp       [rax],al
       mov       ecx,esi
       or        ecx,80000000
       mov       [rax+10],ecx
       mov       rax,[rbx+8]
       cmp       [rax],al
       movsxd    rcx,edi
       mov       dword ptr [rax+rcx+10],0FFFFFFFF
M00_L11:
       mov       eax,1
       jmp       near ptr M00_L05
M00_L12:
       call      qword ptr [7FFB0E2E77B0]
       int       3
M00_L13:
       xor       eax,eax
       jmp       near ptr M00_L05
; Total bytes of code 409
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
       lea       rax,[7FFAE1EB1940]
       jmp       qword ptr [rax]
M01_L13:
       cmp       [rdx],dl
       jmp       near ptr M01_L02
; Total bytes of code 358
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty()
       push      rsi
       push      rbx
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+20],ymm4
       xor       eax,eax
       mov       [rsp+40],rax
       mov       rcx,[rcx+8]
       mov       r8,[rcx+158]
       mov       ebx,r8d
       and       ebx,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    rdx,ebx
       mov       r10d,[rax+rdx+10]
       test      r10d,80000000
       jne       short M00_L01
       vxorps    xmm0,xmm0,xmm0
       vmovdqu   xmmword ptr [rsp+38],xmm0
       xor       eax,eax
       mov       [rsp+30],eax
M00_L00:
       add       rsp,48
       pop       rbx
       pop       rsi
       ret
M00_L01:
       and       r10d,7FFFFFFF
       cmp       r10d,7FFFFFFF
       jne       short M00_L02
       mov       edx,[rcx+14]
       sub       edx,ebx
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r10,ebx
       xor       r9d,r9d
       mov       [rax+r10+10],r9d
       movsxd    rdx,edx
       add       r8,rdx
       mov       [rcx+158],r8
       lea       r8,[rsp+30]
       lea       rdx,[rsp+38]
       call      qword ptr [7FFB0E49DEA8]; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
       jmp       short M00_L00
M00_L02:
       lea       esi,[r10+3]
       and       esi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       ebx,4
       test      rdx,rdx
       je        short M00_L03
       mov       ecx,ebx
       mov       eax,r10d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M00_L05
       mov       ecx,ebx
       lea       rdx,[rdx+rcx+10]
       jmp       short M00_L04
M00_L03:
       or        ebx,r10d
       jne       short M00_L05
       xor       edx,edx
       xor       r10d,r10d
M00_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r10d
       lea       rdx,[rsp+20]
       lea       rcx,[rsp+38]
       call      qword ptr [7FFB0E49DED8]
       add       esi,4
       mov       [rsp+30],esi
       mov       eax,1
       jmp       near ptr M00_L00
M00_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 259
```
```assembly
; Relay.Buffers.MpscByteRingBuffer.TryPeek(System.ReadOnlySpan`1<Byte> ByRef, Int32 ByRef)
M01_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       r10,rdx
       mov       rbx,r8
       mov       rdx,[rcx+158]
       mov       esi,edx
       and       esi,[rcx+10]
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r8,esi
       mov       r9d,[rax+r8+10]
       test      r9d,80000000
       jne       short M01_L01
       xor       eax,eax
       mov       [r10],rax
       mov       [r10+8],rax
       mov       [rbx],eax
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L01:
       and       r9d,7FFFFFFF
       cmp       r9d,7FFFFFFF
       jne       short M01_L02
       mov       r8d,[rcx+14]
       sub       r8d,esi
       mov       rax,[rcx+8]
       cmp       [rax],al
       movsxd    r9,esi
       xor       r11d,r11d
       mov       [rax+r9+10],r11d
       movsxd    r8,r8d
       add       rdx,r8
       mov       [rcx+158],rdx
       mov       rdx,r10
       mov       r8,rbx
       call      qword ptr [7FFB0E49DEA8]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       lea       edi,[r9+3]
       and       edi,0FFFFFFFC
       mov       rdx,[rcx+8]
       add       esi,4
       test      rdx,rdx
       je        short M01_L03
       mov       ecx,esi
       mov       eax,r9d
       add       rcx,rax
       mov       eax,[rdx+8]
       cmp       rcx,rax
       ja        short M01_L05
       mov       ecx,esi
       lea       rdx,[rdx+rcx+10]
       jmp       short M01_L04
M01_L03:
       or        esi,r9d
       jne       short M01_L05
       xor       edx,edx
       xor       r9d,r9d
M01_L04:
       mov       [rsp+20],rdx
       mov       [rsp+28],r9d
       lea       rdx,[rsp+20]
       mov       rcx,r10
       call      qword ptr [7FFB0E49DED8]
       add       edi,4
       mov       [rbx],edi
       mov       eax,1
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       call      qword ptr [7FFB0E2E77B0]
       int       3
; Total bytes of code 256
```

