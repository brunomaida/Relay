## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,98
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+40],ymm4
       vmovdqu   ymmword ptr [rsp+60],ymm4
       vmovdqa   xmmword ptr [rsp+80],xmm4
       mov       rax,7DD1FFA67C50
       mov       [rsp+90],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        near ptr M00_L06
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       r14,offset MT_Relay.Sinks.SharedMemorySpscSink
       cmp       rbp,r14
       jne       near ptr M00_L07
       mov       rcx,rbx
       call      qword ptr [7FFB0E4C11E0]; Relay.Sinks.SharedMemorySpscSink.get_IsHealthy()
M00_L01:
       test      eax,eax
       je        near ptr M00_L20
       cmp       rbp,r14
       jne       near ptr M00_L18
       mov       r10,rbx
       lea       r8d,[rdi+4]
       cmp       r8d,[r10+38]
       jg        near ptr M00_L20
       mov       rax,[r10+30]
       mov       r15d,[rax+8]
       add       r8d,r15d
       mov       ecx,[r10+38]
       mov       eax,r8d
       cdq
       idiv      ecx
       mov       r13d,edx
       mov       rbp,[r10+30]
       add       rbp,80
       cmp       r8d,ecx
       jg        near ptr M00_L08
       movsxd    rcx,r15d
       add       rcx,rbp
       mov       r8,rcx
       mov       edx,edi
       bswap     edx
       mov       [r8],edx
       add       rcx,4
       mov       r8d,edi
       mov       rdx,rsi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lock or   dword ptr [rsp],0
M00_L02:
       mov       rcx,[rbx+30]
       mov       [rcx+8],r13d
M00_L03:
       cmp       byte ptr [rbx+18],0
       jne       near ptr M00_L19
M00_L04:
       mov       rcx,7DD1FFA67C50
       cmp       [rsp+90],rcx
       je        short M00_L05
       call      CORINFO_HELP_FAIL_FAST
M00_L05:
       nop
       add       rsp,98
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L06:
       xor       esi,esi
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L07:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       jmp       near ptr M00_L01
M00_L08:
       xor       edx,edx
       mov       [rsp+28],edx
       lea       rdx,[rsp+28]
       mov       ecx,edi
       bswap     ecx
       mov       [rdx],ecx
       mov       r14d,[r10+38]
       mov       [rsp+40],rdx
       mov       dword ptr [rsp+48],4
       lea       rdx,[rsp+40]
       lea       rcx,[rsp+80]
       call      qword ptr [7FFB0E4FE4D8]; System.Span`1[[System.Byte, System.Private.CoreLib]].op_Implicit(System.Span`1<Byte>)
       mov       r12d,r15d
       mov       rax,[rsp+80]
       mov       [rsp+38],rax
       mov       r10d,[rsp+88]
       mov       [rsp+64],r10d
       mov       r9d,r10d
       xor       r11d,r11d
       test      r10d,r10d
       jle       near ptr M00_L12
M00_L09:
       mov       edx,r14d
       sub       edx,r12d
       cmp       r9d,edx
       jg        short M00_L10
       mov       [rsp+78],r9d
       mov       ecx,r9d
       jmp       short M00_L11
M00_L10:
       mov       ecx,edx
       mov       [rsp+78],r9d
M00_L11:
       mov       r8d,r11d
       mov       edx,ecx
       add       r8,rdx
       mov       edx,r10d
       cmp       r8,rdx
       ja        near ptr M00_L17
       mov       [rsp+74],r11d
       mov       edx,r11d
       add       rdx,rax
       test      ecx,ecx
       jl        near ptr M00_L17
       movsxd    r8,r12d
       add       r8,rbp
       mov       [rsp+30],r8
       mov       [rsp+70],ecx
       mov       r8d,ecx
       mov       rcx,[rsp+30]
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+70]
       lea       eax,[r12+r8]
       cdq
       idiv      r14d
       mov       r12d,edx
       mov       r11d,[rsp+74]
       add       r11d,r8d
       mov       r9d,[rsp+78]
       sub       r9d,r8d
       test      r9d,r9d
       mov       rax,[rsp+38]
       mov       r10d,[rsp+64]
       jg        near ptr M00_L09
M00_L12:
       mov       r14d,[rbx+38]
       lea       eax,[r15+4]
       cdq
       idiv      dword ptr [rbx+38]
       mov       r15d,edx
       mov       r12d,edi
       xor       eax,eax
       test      edi,edi
       jle       short M00_L16
M00_L13:
       mov       r10d,r14d
       sub       r10d,r15d
       cmp       r12d,r10d
       jg        short M00_L14
       mov       r9d,r12d
       jmp       short M00_L15
M00_L14:
       mov       r9d,r10d
M00_L15:
       mov       r8d,eax
       mov       ecx,r9d
       add       r8,rcx
       mov       ecx,edi
       cmp       r8,rcx
       ja        short M00_L17
       mov       [rsp+6C],eax
       mov       edx,eax
       add       rdx,rsi
       test      r9d,r9d
       jl        short M00_L17
       movsxd    rcx,r15d
       add       rcx,rbp
       mov       [rsp+68],r9d
       mov       r8d,r9d
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r9d,[rsp+68]
       lea       eax,[r15+r9]
       cdq
       idiv      r14d
       mov       r15d,edx
       mov       eax,[rsp+6C]
       add       eax,r9d
       sub       r12d,r9d
       test      r12d,r12d
       jg        short M00_L13
M00_L16:
       lock or   dword ptr [rsp],0
       jmp       near ptr M00_L02
M00_L17:
       call      qword ptr [7FFB0E3377B0]
       int       3
M00_L18:
       mov       [rsp+50],rsi
       mov       [rsp+58],edi
       lea       rdx,[rsp+50]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L20
       jmp       near ptr M00_L03
M00_L19:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L04
       mov       [rsp+50],rsi
       mov       [rsp+58],edi
       lea       rdx,[rsp+50]
       call      qword ptr [7FFB0E4FE418]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L04
M00_L20:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L21
       mov       [rsp+50],rsi
       mov       [rsp+58],edi
       lea       rdx,[rsp+50]
       call      qword ptr [7FFB0E4FE418]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L04
M00_L21:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L04
; Total bytes of code 823
```
```assembly
; Relay.Sinks.SharedMemorySpscSink.get_IsHealthy()
       mov       rax,[rcx+30]
       mov       eax,[rax+8]
       mov       rdx,[rcx+30]
       sub       eax,[rdx+40]
       mov       ecx,[rcx+38]
       add       eax,ecx
       cdq
       idiv      ecx
       cmp       edx,ecx
       setl      al
       movzx     eax,al
       ret
; Total bytes of code 31
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M02_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M02_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M02_L06
       cmp       r8,40
       ja        short M02_L03
M02_L00:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L01
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M02_L01
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M02_L01:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M02_L02:
       vzeroupper
       ret
M02_L03:
       cmp       r8,800
       ja        near ptr M02_L10
       cmp       r8,100
       jb        short M02_L04
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
M02_L04:
       mov       r9,r8
       shr       r9,6
M02_L05:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M02_L05
       and       r8,3F
       cmp       r8,10
       ja        near ptr M02_L00
       jmp       near ptr M02_L01
M02_L06:
       test      r8b,18
       jne       short M02_L08
       test      r8b,4
       jne       short M02_L07
       test      r8,r8
       je        near ptr M02_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M02_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M02_L02
M02_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M02_L02
M02_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M02_L02
M02_L09:
       cmp       rcx,rdx
       jne       short M02_L10
       cmp       [rdx],dl
       jmp       near ptr M02_L02
M02_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E106538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 340
```
```assembly
; System.Span`1[[System.Byte, System.Private.CoreLib]].op_Implicit(System.Span`1<Byte>)
       mov       rax,[rdx]
       mov       edx,[rdx+8]
       mov       [rcx],rax
       mov       [rcx+8],edx
       mov       rax,rcx
       ret
; Total bytes of code 16
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M04_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rax,offset MT_Relay.Sinks.SharedMemorySpscSink
       cmp       rdi,rax
       jne       short M04_L02
       mov       rax,[rbx+30]
       mov       eax,[rax+8]
       mov       rdx,[rbx+30]
       sub       eax,[rdx+40]
       mov       ecx,[rbx+38]
       add       eax,ecx
       cdq
       idiv      ecx
       cmp       edx,ecx
       jge       short M04_L05
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       call      qword ptr [7FFB0E4C11E8]; Relay.Sinks.SharedMemorySpscSink.Accept(System.ReadOnlySpan`1<Byte>)
M04_L01:
       test      eax,eax
       je        short M04_L05
       cmp       byte ptr [rbx+18],0
       je        short M04_L04
       jmp       short M04_L03
M04_L02:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M04_L05
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       jmp       short M04_L01
M04_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M04_L04
       mov       rdx,rsi
       call      qword ptr [7FFB0E4FE418]
M04_L04:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M04_L05:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M04_L06
       mov       rdx,rsi
       call      qword ptr [7FFB0E4FE418]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M04_L06:
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 216
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
```assembly
; Relay.Sinks.SharedMemorySpscSink.Accept(System.ReadOnlySpan`1<Byte>)
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,58
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,7DD1FFA67C50
       mov       [rsp+50],rax
       mov       rbx,rdx
       mov       rsi,rcx
       mov       edi,[rbx+8]
       lea       r8d,[rdi+4]
       cmp       r8d,[rsi+38]
       jg        near ptr M06_L11
       mov       rax,[rsi+30]
       mov       ebp,[rax+8]
       add       r8d,ebp
       mov       ecx,[rsi+38]
       mov       eax,r8d
       cdq
       idiv      ecx
       mov       r14d,edx
       mov       r15,[rsi+30]
       add       r15,80
       cmp       r8d,ecx
       jg        short M06_L01
       movsxd    rcx,ebp
       add       rcx,r15
       mov       rdx,rcx
       mov       r8d,edi
       bswap     r8d
       mov       [rdx],r8d
       test      edi,edi
       jl        near ptr M06_L10
       add       rcx,4
       mov       rdx,[rbx]
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lock or   dword ptr [rsp],0
       mov       rax,[rsi+30]
       mov       [rax+8],r14d
       mov       eax,1
       mov       rcx,7DD1FFA67C50
       cmp       [rsp+50],rcx
       je        short M06_L00
       call      CORINFO_HELP_FAIL_FAST
M06_L00:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M06_L01:
       lea       r13,[rsp+28]
       mov       r8d,edi
       bswap     r8d
       mov       [r13],r8d
       mov       r12d,[rsi+38]
       mov       eax,ebp
       mov       r10d,4
       xor       r9d,r9d
M06_L02:
       mov       r11d,r12d
       sub       r11d,eax
       cmp       r10d,r11d
       jg        short M06_L03
       mov       [rsp+4C],r10d
       mov       edx,r10d
       jmp       short M06_L04
M06_L03:
       mov       edx,r11d
       mov       [rsp+4C],r10d
M06_L04:
       mov       [rsp+48],r9d
       mov       r8d,r9d
       mov       ecx,edx
       lea       r11,[r8+rcx]
       cmp       r11,4
       ja        near ptr M06_L10
       add       r8,r13
       mov       r11,r8
       mov       [rsp+40],edx
       test      edx,edx
       jl        near ptr M06_L10
       mov       [rsp+44],eax
       movsxd    r8,eax
       add       r8,r15
       mov       [rsp+30],r8
       mov       r8,rcx
       mov       rcx,[rsp+30]
       mov       rdx,r11
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+40]
       mov       eax,r8d
       add       eax,[rsp+44]
       cdq
       idiv      r12d
       mov       eax,edx
       mov       r9d,r8d
       add       r9d,[rsp+48]
       mov       edx,r9d
       mov       r10d,[rsp+4C]
       sub       r10d,r8d
       test      r10d,r10d
       mov       r9d,edx
       jg        near ptr M06_L02
       mov       r8d,[rsi+38]
       mov       r13d,r8d
       lea       eax,[rbp+4]
       cdq
       idiv      r8d
       mov       ebp,edx
       mov       rbx,[rbx]
       mov       r12d,edi
       xor       eax,eax
       test      edi,edi
       jle       short M06_L08
M06_L05:
       mov       r10d,r13d
       sub       r10d,ebp
       cmp       r12d,r10d
       jg        short M06_L06
       mov       r9d,r12d
       jmp       short M06_L07
M06_L06:
       mov       r9d,r10d
M06_L07:
       mov       [rsp+3C],eax
       mov       edx,eax
       mov       r8d,r9d
       lea       rcx,[rdx+r8]
       mov       r10d,edi
       cmp       rcx,r10
       ja        short M06_L10
       add       rdx,rbx
       mov       [rsp+38],r9d
       test      r9d,r9d
       jl        short M06_L10
       movsxd    rcx,ebp
       add       rcx,r15
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r9d,[rsp+38]
       lea       eax,[r9+rbp]
       cdq
       idiv      r13d
       mov       ebp,edx
       mov       eax,r9d
       add       eax,[rsp+3C]
       sub       r12d,r9d
       test      r12d,r12d
       jg        short M06_L05
M06_L08:
       lock or   dword ptr [rsp],0
       mov       rax,[rsi+30]
       mov       [rax+8],r14d
       mov       eax,1
       mov       rcx,7DD1FFA67C50
       cmp       [rsp+50],rcx
       je        short M06_L09
       call      CORINFO_HELP_FAIL_FAST
M06_L09:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M06_L10:
       call      qword ptr [7FFB0E3377B0]
       int       3
M06_L11:
       xor       eax,eax
       mov       rcx,7DD1FFA67C50
       cmp       [rsp+50],rcx
       je        short M06_L12
       call      CORINFO_HELP_FAIL_FAST
M06_L12:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
; Total bytes of code 611
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single()
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,68
       xor       eax,eax
       mov       [rsp+30],rax
       mov       rax,0D761DE696D85
       mov       [rsp+60],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        near ptr M00_L06
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,offset MT_Relay.Sinks.SharedMemorySpscSink
       cmp       [rbx],rcx
       jne       near ptr M00_L12
       mov       rcx,rbx
       call      qword ptr [7FFB0E4C11E0]; Relay.Sinks.SharedMemorySpscSink.get_IsHealthy()
M00_L01:
       test      eax,eax
       je        near ptr M00_L19
       mov       rax,offset MT_Relay.Sinks.SharedMemorySpscSink
       cmp       [rbx],rax
       jne       near ptr M00_L17
       mov       r10,rbx
       lea       r8d,[rdi+4]
       cmp       r8d,[r10+38]
       jg        near ptr M00_L19
       mov       rax,[r10+30]
       mov       ebp,[rax+8]
       add       r8d,ebp
       mov       ecx,[r10+38]
       mov       eax,r8d
       cdq
       idiv      ecx
       mov       r14d,edx
       mov       r15,[r10+30]
       add       r15,80
       cmp       r8d,ecx
       jg        near ptr M00_L13
       movsxd    rcx,ebp
       add       rcx,r15
       mov       r8,rcx
       mov       edx,edi
       bswap     edx
       mov       [r8],edx
       add       rcx,4
       mov       r8d,edi
       mov       rdx,rsi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lock or   dword ptr [rsp],0
M00_L02:
       mov       r8,[rbx+30]
       mov       [r8+8],r14d
M00_L03:
       cmp       byte ptr [rbx+18],0
       jne       near ptr M00_L18
M00_L04:
       mov       rcx,0D761DE696D85
       cmp       [rsp+60],rcx
       je        short M00_L05
       call      CORINFO_HELP_FAIL_FAST
M00_L05:
       nop
       add       rsp,68
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M00_L06:
       xor       esi,esi
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L07:
       mov       r11d,r12d
       sub       r11d,eax
       cmp       r10d,r11d
       jg        near ptr M00_L14
       mov       [rsp+58],r10d
       mov       edx,r10d
M00_L08:
       mov       [rsp+54],r9d
       mov       r8d,r9d
       mov       ecx,edx
       lea       r11,[r8+rcx]
       cmp       r11,4
       ja        near ptr M00_L16
       add       r8,r13
       mov       r11,r8
       mov       [rsp+4C],edx
       test      edx,edx
       jl        near ptr M00_L16
       mov       [rsp+50],eax
       movsxd    r8,eax
       add       r8,r15
       mov       [rsp+28],r8
       mov       r8,rcx
       mov       rcx,[rsp+28]
       mov       rdx,r11
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+4C]
       mov       eax,r8d
       add       eax,[rsp+50]
       cdq
       idiv      r12d
       mov       eax,edx
       mov       r9d,r8d
       add       r9d,[rsp+54]
       mov       edx,r9d
       mov       r10d,[rsp+58]
       sub       r10d,r8d
       test      r10d,r10d
       mov       r9d,edx
       jg        near ptr M00_L07
       mov       r8d,[rbx+38]
       mov       r13d,r8d
       lea       eax,[rbp+4]
       cdq
       idiv      r8d
       mov       ebp,edx
       mov       r8d,edi
       mov       r12d,r8d
       xor       eax,eax
       test      r8d,r8d
       jle       short M00_L11
M00_L09:
       mov       r10d,r13d
       sub       r10d,ebp
       cmp       r12d,r10d
       jg        near ptr M00_L15
       mov       r9d,r12d
M00_L10:
       mov       [rsp+48],eax
       mov       edx,eax
       mov       r8d,r9d
       lea       rcx,[rdx+r8]
       mov       r10d,edi
       cmp       rcx,r10
       ja        near ptr M00_L16
       add       rdx,rsi
       mov       [rsp+44],r9d
       test      r9d,r9d
       jl        near ptr M00_L16
       movsxd    rcx,ebp
       add       rcx,r15
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       ecx,[rsp+44]
       lea       eax,[rcx+rbp]
       cdq
       idiv      r13d
       mov       ebp,edx
       mov       eax,ecx
       add       eax,[rsp+48]
       sub       r12d,ecx
       test      r12d,r12d
       jg        short M00_L09
M00_L11:
       lock or   dword ptr [rsp],0
       jmp       near ptr M00_L02
M00_L12:
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       jmp       near ptr M00_L01
M00_L13:
       xor       r8d,r8d
       mov       [rsp+20],r8d
       lea       r13,[rsp+20]
       mov       r8d,edi
       bswap     r8d
       mov       [r13],r8d
       mov       r12d,[r10+38]
       mov       eax,ebp
       mov       r10d,4
       xor       r9d,r9d
       jmp       near ptr M00_L07
M00_L14:
       mov       edx,r11d
       mov       [rsp+58],r10d
       jmp       near ptr M00_L08
M00_L15:
       mov       ecx,r10d
       mov       r9d,ecx
       jmp       near ptr M00_L10
M00_L16:
       call      qword ptr [7FFB0E3377B0]
       int       3
M00_L17:
       mov       [rsp+30],rsi
       mov       [rsp+38],edi
       lea       rdx,[rsp+30]
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L19
       jmp       near ptr M00_L03
M00_L18:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L04
       mov       [rsp+30],rsi
       mov       [rsp+38],edi
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E4FE430]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L04
M00_L19:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L20
       mov       [rsp+30],rsi
       mov       [rsp+38],edi
       lea       rdx,[rsp+30]
       call      qword ptr [7FFB0E4FE430]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L04
M00_L20:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L04
; Total bytes of code 778
```
```assembly
; Relay.Sinks.SharedMemorySpscSink.get_IsHealthy()
       mov       rax,[rcx+30]
       mov       eax,[rax+8]
       mov       rdx,[rcx+30]
       sub       eax,[rdx+40]
       mov       ecx,[rcx+38]
       add       eax,ecx
       cdq
       idiv      ecx
       cmp       edx,ecx
       setl      al
       movzx     eax,al
       ret
; Total bytes of code 31
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M02_L09
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M02_L09
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M02_L06
       cmp       r8,40
       jbe       short M02_L02
       cmp       r8,800
       ja        near ptr M02_L10
       cmp       r8,100
       jb        short M02_L00
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
M02_L00:
       mov       r9,r8
       shr       r9,6
M02_L01:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M02_L01
       and       r8,3F
       cmp       r8,10
       jbe       short M02_L03
M02_L02:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L03
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       ja        short M02_L05
M02_L03:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
M02_L04:
       vzeroupper
       ret
M02_L05:
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
       jmp       short M02_L03
M02_L06:
       test      r8b,18
       jne       short M02_L08
       test      r8b,4
       jne       short M02_L07
       test      r8,r8
       je        short M02_L04
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M02_L04
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M02_L04
M02_L07:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       short M02_L04
M02_L08:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M02_L04
M02_L09:
       cmp       rcx,rdx
       jne       short M02_L10
       cmp       [rdx],dl
       jmp       short M02_L04
M02_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FFB0E106538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 313
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M03_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rax,offset MT_Relay.Sinks.SharedMemorySpscSink
       cmp       rdi,rax
       jne       short M03_L02
       mov       rax,[rbx+30]
       mov       eax,[rax+8]
       mov       rdx,[rbx+30]
       sub       eax,[rdx+40]
       mov       ecx,[rbx+38]
       add       eax,ecx
       cdq
       idiv      ecx
       cmp       edx,ecx
       jge       short M03_L05
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       call      qword ptr [7FFB0E4C11E8]; Relay.Sinks.SharedMemorySpscSink.Accept(System.ReadOnlySpan`1<Byte>)
M03_L01:
       test      eax,eax
       je        short M03_L05
       cmp       byte ptr [rbx+18],0
       je        short M03_L04
       jmp       short M03_L03
M03_L02:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M03_L05
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       jmp       short M03_L01
M03_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M03_L04
       mov       rdx,rsi
       call      qword ptr [7FFB0E4FE430]
M03_L04:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L05:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M03_L06
       mov       rdx,rsi
       call      qword ptr [7FFB0E4FE430]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L06:
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 216
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
```assembly
; Relay.Sinks.SharedMemorySpscSink.Accept(System.ReadOnlySpan`1<Byte>)
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,58
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,0D761DE696D85
       mov       [rsp+50],rax
       mov       rbx,rdx
       mov       rsi,rcx
       mov       edi,[rbx+8]
       lea       r8d,[rdi+4]
       cmp       r8d,[rsi+38]
       jg        near ptr M05_L14
       mov       rax,[rsi+30]
       mov       ebp,[rax+8]
       add       r8d,ebp
       mov       ecx,[rsi+38]
       mov       eax,r8d
       cdq
       idiv      ecx
       mov       r14d,edx
       mov       r15,[rsi+30]
       add       r15,80
       cmp       r8d,ecx
       jg        near ptr M05_L05
       movsxd    rcx,ebp
       add       rcx,r15
       mov       rdx,rcx
       mov       r8d,edi
       bswap     r8d
       mov       [rdx],r8d
       test      edi,edi
       jl        near ptr M05_L13
       add       rcx,4
       mov       rdx,[rbx]
       mov       r8d,edi
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lock or   dword ptr [rsp],0
       mov       rax,[rsi+30]
       mov       [rax+8],r14d
       mov       eax,1
       mov       rcx,0D761DE696D85
       cmp       [rsp+50],rcx
       je        short M05_L00
       call      CORINFO_HELP_FAIL_FAST
M05_L00:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M05_L01:
       mov       r11d,r12d
       sub       r11d,eax
       cmp       r10d,r11d
       jle       near ptr M05_L06
       jmp       near ptr M05_L07
M05_L02:
       mov       [rsp+48],r9d
       mov       r8d,r9d
       mov       ecx,edx
       lea       r11,[r8+rcx]
       cmp       r11,4
       ja        near ptr M05_L13
       add       r8,r13
       mov       r11,r8
       mov       [rsp+40],edx
       test      edx,edx
       jl        near ptr M05_L13
       mov       [rsp+44],eax
       movsxd    r8,eax
       add       r8,r15
       mov       [rsp+30],r8
       mov       r8,rcx
       mov       rcx,[rsp+30]
       mov       rdx,r11
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+40]
       mov       eax,r8d
       add       eax,[rsp+44]
       cdq
       idiv      r12d
       mov       eax,edx
       mov       r9d,r8d
       add       r9d,[rsp+48]
       mov       edx,r9d
       mov       r10d,[rsp+4C]
       sub       r10d,r8d
       test      r10d,r10d
       mov       r9d,edx
       jg        near ptr M05_L01
       jmp       near ptr M05_L08
M05_L03:
       mov       r10d,r13d
       sub       r10d,ebp
       cmp       r12d,r10d
       jle       near ptr M05_L09
       jmp       near ptr M05_L10
M05_L04:
       mov       [rsp+3C],eax
       mov       edx,eax
       mov       r8d,r9d
       lea       rcx,[rdx+r8]
       mov       r10d,edi
       cmp       rcx,r10
       ja        near ptr M05_L13
       add       rdx,rbx
       mov       [rsp+38],r9d
       test      r9d,r9d
       jl        near ptr M05_L13
       movsxd    rcx,ebp
       add       rcx,r15
       call      qword ptr [7FFB0E1057B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r9d,[rsp+38]
       lea       eax,[r9+rbp]
       cdq
       idiv      r13d
       mov       ebp,edx
       mov       eax,r9d
       add       eax,[rsp+3C]
       sub       r12d,r9d
       test      r12d,r12d
       jg        short M05_L03
       jmp       short M05_L11
M05_L05:
       lea       r13,[rsp+28]
       mov       r8d,edi
       bswap     r8d
       mov       [r13],r8d
       mov       r12d,[rsi+38]
       mov       eax,ebp
       mov       r10d,4
       xor       r9d,r9d
       jmp       near ptr M05_L01
M05_L06:
       mov       [rsp+4C],r10d
       mov       edx,r10d
       jmp       near ptr M05_L02
M05_L07:
       mov       edx,r11d
       mov       [rsp+4C],r10d
       jmp       near ptr M05_L02
M05_L08:
       mov       r8d,[rsi+38]
       mov       r13d,r8d
       lea       eax,[rbp+4]
       cdq
       idiv      r8d
       mov       ebp,edx
       mov       rbx,[rbx]
       mov       r12d,edi
       xor       eax,eax
       test      edi,edi
       jle       short M05_L11
       jmp       near ptr M05_L03
M05_L09:
       mov       r9d,r12d
       jmp       near ptr M05_L04
M05_L10:
       mov       r9d,r10d
       jmp       near ptr M05_L04
M05_L11:
       lock or   dword ptr [rsp],0
       mov       rax,[rsi+30]
       mov       [rax+8],r14d
       mov       eax,1
       mov       rcx,0D761DE696D85
       cmp       [rsp+50],rcx
       je        short M05_L12
       call      CORINFO_HELP_FAIL_FAST
M05_L12:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
M05_L13:
       call      qword ptr [7FFB0E3377B0]
       int       3
M05_L14:
       xor       eax,eax
       mov       rcx,0D761DE696D85
       cmp       [rsp+50],rcx
       je        short M05_L15
       call      CORINFO_HELP_FAIL_FAST
M05_L15:
       nop
       add       rsp,58
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r12
       pop       r13
       pop       r14
       pop       r15
       ret
; Total bytes of code 674
```

