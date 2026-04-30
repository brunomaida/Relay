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
       sub       rsp,58
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,0BA407033CB06
       mov       [rsp+50],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        near ptr M00_L11
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FF81C5C06C8]; Relay.Sinks.SharedMemorySink.get_IsHealthy()
       test      eax,eax
       je        near ptr M00_L16
       mov       r8d,edi
       add       r8d,4
       cmp       r8d,[rbx+38]
       jg        near ptr M00_L16
M00_L01:
       mov       rax,[rbx+30]
       mov       ebp,[rax+8]
       lea       eax,[r8+rbp]
       cdq
       idiv      dword ptr [rbx+38]
       mov       rcx,[rbx+30]
       add       rcx,8
       mov       eax,ebp
       lock cmpxchg [rcx],edx
       cmp       eax,ebp
       jne       short M00_L01
       mov       r14,[rbx+30]
       add       r14,80
       xor       r8d,r8d
       mov       [rsp+20],r8d
       lea       r15,[rsp+20]
       mov       r8d,edi
       bswap     r8d
       mov       [r15],r8d
       mov       r13d,[rbx+38]
       mov       r12d,ebp
       mov       eax,4
       xor       r10d,r10d
M00_L02:
       mov       r9d,r13d
       sub       r9d,r12d
       cmp       eax,r9d
       jg        near ptr M00_L12
       mov       [rsp+48],eax
       mov       r11d,eax
M00_L03:
       mov       r8d,r10d
       mov       ecx,r11d
       add       r8,rcx
       cmp       r8,4
       ja        near ptr M00_L15
       mov       [rsp+44],r10d
       mov       edx,r10d
       add       rdx,r15
       test      r11d,r11d
       jl        near ptr M00_L15
       movsxd    rcx,r12d
       add       rcx,r14
       mov       [rsp+40],r11d
       mov       r8d,r11d
       call      qword ptr [7FF81C2257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+40]
       lea       eax,[r12+r8]
       cdq
       idiv      r13d
       mov       r12d,edx
       mov       r10d,r8d
       add       r10d,[rsp+44]
       mov       eax,r10d
       mov       edx,[rsp+48]
       sub       edx,r8d
       test      edx,edx
       mov       r10d,eax
       jg        short M00_L05
       lea       eax,[rbp+4]
       mov       ebp,[rbx+38]
       cdq
       idiv      ebp
       mov       r15d,edx
       mov       r13d,edi
       xor       r12d,r12d
       test      edi,edi
       jle       short M00_L08
M00_L04:
       mov       eax,ebp
       sub       eax,r15d
       cmp       r13d,eax
       jle       short M00_L06
       jmp       near ptr M00_L13
M00_L05:
       mov       eax,edx
       jmp       near ptr M00_L02
M00_L06:
       mov       r10d,r13d
M00_L07:
       mov       r8d,r12d
       mov       ecx,r10d
       add       r8,rcx
       mov       ecx,edi
       cmp       r8,rcx
       ja        near ptr M00_L15
       mov       edx,r12d
       add       rdx,rsi
       test      r10d,r10d
       jl        near ptr M00_L15
       movsxd    rcx,r15d
       add       rcx,r14
       mov       [rsp+3C],r10d
       mov       r8d,r10d
       call      qword ptr [7FF81C2257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       ecx,[rsp+3C]
       lea       eax,[r15+rcx]
       cdq
       idiv      ebp
       mov       r15d,edx
       add       r12d,ecx
       sub       r13d,ecx
       test      r13d,r13d
       jg        short M00_L04
M00_L08:
       cmp       byte ptr [rbx+18],0
       jne       short M00_L14
M00_L09:
       mov       rcx,0BA407033CB06
       cmp       [rsp+50],rcx
       je        short M00_L10
       call      CORINFO_HELP_FAIL_FAST
M00_L10:
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
M00_L11:
       xor       esi,esi
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L12:
       mov       r11d,r9d
       mov       [rsp+48],eax
       jmp       near ptr M00_L03
M00_L13:
       mov       ecx,eax
       mov       r10d,ecx
       jmp       near ptr M00_L07
M00_L14:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L09
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5EE610]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L09
M00_L15:
       call      qword ptr [7FF81C437798]
       int       3
M00_L16:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L17
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5EE610]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L09
M00_L17:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L09
; Total bytes of code 590
```
```assembly
; Relay.Sinks.SharedMemorySink.get_IsHealthy()
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
       ja        short M02_L01
       test      r8b,18
       jne       short M02_L05
       test      r8b,4
       je        short M02_L04
       mov       r8d,[rdx]
       mov       [rcx],r8d
       mov       edx,[rax-4]
       mov       [r10-4],edx
M02_L00:
       vzeroupper
       ret
M02_L01:
       cmp       r8,40
       ja        short M02_L06
M02_L02:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L03
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       jbe       short M02_L03
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
M02_L03:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
       jmp       short M02_L00
M02_L04:
       test      r8,r8
       je        short M02_L00
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        short M02_L00
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       short M02_L00
M02_L05:
       mov       r8,[rdx]
       mov       [rcx],r8
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M02_L00
M02_L06:
       cmp       r8,800
       ja        short M02_L10
       cmp       r8,100
       jb        short M02_L07
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
M02_L07:
       mov       r9,r8
       shr       r9,6
M02_L08:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M02_L08
       and       r8,3F
       cmp       r8,10
       ja        near ptr M02_L02
       jmp       near ptr M02_L03
M02_L09:
       cmp       rcx,rdx
       jne       short M02_L10
       cmp       [rdx],dl
       jmp       near ptr M02_L00
M02_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FF81C226538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 317
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
       mov       rax,offset MT_Relay.Sinks.SharedMemorySink
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
       call      qword ptr [7FF81C5C06D0]; Relay.Sinks.SharedMemorySink.Accept(System.ReadOnlySpan`1<Byte>)
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
       call      qword ptr [7FF81C5EE610]
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
       call      qword ptr [7FF81C5EE610]
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
       call      qword ptr [7FF86E291138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FF86E2BCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FF86E291140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Sinks.SharedMemorySink.Accept(System.ReadOnlySpan`1<Byte>)
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,0BA407033CB06
       mov       [rsp+40],rax
       mov       rbx,rdx
       mov       rsi,rcx
       mov       edi,[rbx+8]
       lea       r8d,[rdi+4]
       cmp       r8d,[rsi+38]
       jg        near ptr M05_L11
M05_L00:
       mov       rax,[rsi+30]
       mov       ebp,[rax+8]
       lea       eax,[r8+rbp]
       cdq
       idiv      dword ptr [rsi+38]
       mov       rcx,[rsi+30]
       add       rcx,8
       mov       eax,ebp
       lock cmpxchg [rcx],edx
       cmp       eax,ebp
       jne       short M05_L00
       mov       r14,[rsi+30]
       add       r14,80
       lea       r15,[rsp+28]
       mov       r8d,edi
       bswap     r8d
       mov       [r15],r8d
       mov       r13d,[rsi+38]
       mov       r12d,ebp
       mov       eax,4
       xor       r10d,r10d
M05_L01:
       mov       r9d,r13d
       sub       r9d,r12d
       cmp       eax,r9d
       jg        near ptr M05_L09
       mov       [rsp+3C],eax
       mov       r11d,eax
M05_L02:
       mov       r8d,r10d
       mov       ecx,r11d
       add       r8,rcx
       cmp       r8,4
       ja        near ptr M05_L13
       mov       [rsp+38],r10d
       mov       edx,r10d
       add       rdx,r15
       test      r11d,r11d
       jl        near ptr M05_L13
       movsxd    rcx,r12d
       add       rcx,r14
       mov       [rsp+34],r11d
       mov       r8d,r11d
       call      qword ptr [7FF81C2257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+34]
       lea       eax,[r12+r8]
       cdq
       idiv      r13d
       mov       r12d,edx
       mov       r10d,r8d
       add       r10d,[rsp+38]
       mov       eax,r10d
       mov       edx,[rsp+3C]
       sub       edx,r8d
       test      edx,edx
       mov       r10d,eax
       jg        short M05_L04
       lea       eax,[rbp+4]
       mov       esi,[rsi+38]
       cdq
       idiv      esi
       mov       ebp,edx
       mov       rbx,[rbx]
       mov       r15d,edi
       xor       r13d,r13d
       test      edi,edi
       jle       short M05_L07
M05_L03:
       mov       r12d,esi
       sub       r12d,ebp
       cmp       r15d,r12d
       jle       short M05_L05
       jmp       near ptr M05_L10
M05_L04:
       mov       eax,edx
       jmp       near ptr M05_L01
M05_L05:
       mov       r12d,r15d
M05_L06:
       mov       r8d,r13d
       mov       ecx,r12d
       add       r8,rcx
       mov       ecx,edi
       cmp       r8,rcx
       ja        near ptr M05_L13
       mov       edx,r13d
       add       rdx,rbx
       test      r12d,r12d
       jl        near ptr M05_L13
       movsxd    rcx,ebp
       add       rcx,r14
       mov       r8d,r12d
       call      qword ptr [7FF81C2257B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lea       eax,[r12+rbp]
       cdq
       idiv      esi
       mov       ebp,edx
       add       r13d,r12d
       sub       r15d,r12d
       test      r15d,r15d
       jg        short M05_L03
M05_L07:
       mov       eax,1
       mov       rcx,0BA407033CB06
       cmp       [rsp+40],rcx
       je        short M05_L08
       call      CORINFO_HELP_FAIL_FAST
M05_L08:
       nop
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
M05_L09:
       mov       r11d,r9d
       mov       [rsp+3C],eax
       jmp       near ptr M05_L02
M05_L10:
       jmp       near ptr M05_L06
M05_L11:
       xor       eax,eax
       mov       rcx,0BA407033CB06
       cmp       [rsp+40],rcx
       je        short M05_L12
       call      CORINFO_HELP_FAIL_FAST
M05_L12:
       nop
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
M05_L13:
       call      qword ptr [7FF81C437798]
       int       3
; Total bytes of code 487
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
       sub       rsp,58
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,0FF9B631EFF3D
       mov       [rsp+50],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        near ptr M00_L11
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FF81C5B06C8]; Relay.Sinks.SharedMemorySink.get_IsHealthy()
       test      eax,eax
       je        near ptr M00_L16
       mov       r8d,edi
       add       r8d,4
       cmp       r8d,[rbx+38]
       jg        near ptr M00_L16
M00_L01:
       mov       rax,[rbx+30]
       mov       ebp,[rax+8]
       lea       eax,[r8+rbp]
       cdq
       idiv      dword ptr [rbx+38]
       mov       rcx,[rbx+30]
       add       rcx,8
       mov       eax,ebp
       lock cmpxchg [rcx],edx
       cmp       eax,ebp
       jne       short M00_L01
       mov       r14,[rbx+30]
       add       r14,80
       xor       r8d,r8d
       mov       [rsp+20],r8d
       lea       r15,[rsp+20]
       mov       r8d,edi
       bswap     r8d
       mov       [r15],r8d
       mov       r13d,[rbx+38]
       mov       r12d,ebp
       mov       eax,4
       xor       r10d,r10d
M00_L02:
       mov       r9d,r13d
       sub       r9d,r12d
       cmp       eax,r9d
       jg        near ptr M00_L12
       mov       [rsp+48],eax
       mov       r11d,eax
M00_L03:
       mov       r8d,r10d
       mov       ecx,r11d
       add       r8,rcx
       cmp       r8,4
       ja        near ptr M00_L15
       mov       [rsp+44],r10d
       mov       edx,r10d
       add       rdx,r15
       test      r11d,r11d
       jl        near ptr M00_L15
       movsxd    rcx,r12d
       add       rcx,r14
       mov       [rsp+40],r11d
       mov       r8d,r11d
       call      qword ptr [7FF81C2157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+40]
       lea       eax,[r12+r8]
       cdq
       idiv      r13d
       mov       r12d,edx
       mov       r10d,r8d
       add       r10d,[rsp+44]
       mov       eax,r10d
       mov       edx,[rsp+48]
       sub       edx,r8d
       test      edx,edx
       mov       r10d,eax
       jg        short M00_L05
       lea       eax,[rbp+4]
       mov       ebp,[rbx+38]
       cdq
       idiv      ebp
       mov       r15d,edx
       mov       r13d,edi
       xor       r12d,r12d
       test      edi,edi
       jle       short M00_L08
M00_L04:
       mov       eax,ebp
       sub       eax,r15d
       cmp       r13d,eax
       jle       short M00_L06
       jmp       near ptr M00_L13
M00_L05:
       mov       eax,edx
       jmp       near ptr M00_L02
M00_L06:
       mov       r10d,r13d
M00_L07:
       mov       r8d,r12d
       mov       ecx,r10d
       add       r8,rcx
       mov       ecx,edi
       cmp       r8,rcx
       ja        near ptr M00_L15
       mov       edx,r12d
       add       rdx,rsi
       test      r10d,r10d
       jl        near ptr M00_L15
       movsxd    rcx,r15d
       add       rcx,r14
       mov       [rsp+3C],r10d
       mov       r8d,r10d
       call      qword ptr [7FF81C2157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       ecx,[rsp+3C]
       lea       eax,[r15+rcx]
       cdq
       idiv      ebp
       mov       r15d,edx
       add       r12d,ecx
       sub       r13d,ecx
       test      r13d,r13d
       jg        short M00_L04
M00_L08:
       cmp       byte ptr [rbx+18],0
       jne       short M00_L14
M00_L09:
       mov       rcx,0FF9B631EFF3D
       cmp       [rsp+50],rcx
       je        short M00_L10
       call      CORINFO_HELP_FAIL_FAST
M00_L10:
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
M00_L11:
       xor       esi,esi
       xor       edi,edi
       jmp       near ptr M00_L00
M00_L12:
       mov       r11d,r9d
       mov       [rsp+48],eax
       jmp       near ptr M00_L03
M00_L13:
       mov       ecx,eax
       mov       r10d,ecx
       jmp       near ptr M00_L07
M00_L14:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L09
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5DE5F8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L09
M00_L15:
       call      qword ptr [7FF81C427798]
       int       3
M00_L16:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L17
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5DE5F8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L09
M00_L17:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L09
; Total bytes of code 590
```
```assembly
; Relay.Sinks.SharedMemorySink.get_IsHealthy()
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
       ja        short M02_L02
       test      r8b,18
       jne       short M02_L01
       test      r8b,4
       je        near ptr M02_L08
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
M02_L00:
       vzeroupper
       ret
M02_L01:
       mov       r8,[rdx]
       mov       [rcx],r8
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       short M02_L00
M02_L02:
       cmp       r8,40
       jbe       short M02_L05
       cmp       r8,800
       ja        near ptr M02_L10
       cmp       r8,100
       jb        short M02_L03
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
M02_L03:
       mov       r9,r8
       shr       r9,6
M02_L04:
       vmovdqu   ymm0,ymmword ptr [rdx]
       vmovdqu   ymmword ptr [rcx],ymm0
       vmovdqu   ymm0,ymmword ptr [rdx+20]
       vmovdqu   ymmword ptr [rcx+20],ymm0
       add       rcx,40
       add       rdx,40
       dec       r9
       jne       short M02_L04
       and       r8,3F
       cmp       r8,10
       jbe       short M02_L06
M02_L05:
       vmovups   xmm0,[rdx]
       vmovups   [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L06
       vmovups   xmm0,[rdx+10]
       vmovups   [rcx+10],xmm0
       cmp       r8,30
       ja        short M02_L07
M02_L06:
       vmovups   xmm0,[rax-10]
       vmovups   [r10-10],xmm0
       jmp       near ptr M02_L00
M02_L07:
       vmovups   xmm0,[rdx+20]
       vmovups   [rcx+20],xmm0
       jmp       short M02_L06
M02_L08:
       test      r8,r8
       je        near ptr M02_L00
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M02_L00
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M02_L00
M02_L09:
       cmp       rcx,rdx
       jne       short M02_L10
       cmp       [rdx],dl
       jmp       near ptr M02_L00
M02_L10:
       cmp       [rcx],cl
       cmp       [rdx],dl
       vzeroupper
       jmp       qword ptr [7FF81C216538]; System.Buffer._Memmove(Byte ByRef, Byte ByRef, UIntPtr)
; Total bytes of code 330
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
       mov       rax,offset MT_Relay.Sinks.SharedMemorySink
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
       call      qword ptr [7FF81C5B06D0]; Relay.Sinks.SharedMemorySink.Accept(System.ReadOnlySpan`1<Byte>)
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
       call      qword ptr [7FF81C5DE5F8]
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
       call      qword ptr [7FF81C5DE5F8]
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
       call      qword ptr [7FF86E291138]; CORINFO_HELP_JIT_PINVOKE_BEGIN
       mov       rax,[7FF86E2BCB90]
       mov       rcx,[rbp-0A8]
       mov       rdx,[rbp-0B0]
       mov       r8,[rbp-0B8]
       call      qword ptr [rax]
       lea       rcx,[rbp-0A0]
       call      qword ptr [7FF86E291140]; CORINFO_HELP_JIT_PINVOKE_END
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
; Relay.Sinks.SharedMemorySink.Accept(System.ReadOnlySpan`1<Byte>)
       push      r15
       push      r14
       push      r13
       push      r12
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,48
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rax,0FF9B631EFF3D
       mov       [rsp+40],rax
       mov       rbx,rdx
       mov       rsi,rcx
       mov       edi,[rbx+8]
       lea       r8d,[rdi+4]
       cmp       r8d,[rsi+38]
       jg        near ptr M05_L11
M05_L00:
       mov       rax,[rsi+30]
       mov       ebp,[rax+8]
       lea       eax,[r8+rbp]
       cdq
       idiv      dword ptr [rsi+38]
       mov       rcx,[rsi+30]
       add       rcx,8
       mov       eax,ebp
       lock cmpxchg [rcx],edx
       cmp       eax,ebp
       jne       short M05_L00
       mov       r14,[rsi+30]
       add       r14,80
       lea       r15,[rsp+28]
       mov       r8d,edi
       bswap     r8d
       mov       [r15],r8d
       mov       r13d,[rsi+38]
       mov       r12d,ebp
       mov       eax,4
       xor       r10d,r10d
M05_L01:
       mov       r9d,r13d
       sub       r9d,r12d
       cmp       eax,r9d
       jg        near ptr M05_L09
       mov       [rsp+3C],eax
       mov       r11d,eax
M05_L02:
       mov       r8d,r10d
       mov       ecx,r11d
       add       r8,rcx
       cmp       r8,4
       ja        near ptr M05_L13
       mov       [rsp+38],r10d
       mov       edx,r10d
       add       rdx,r15
       test      r11d,r11d
       jl        near ptr M05_L13
       movsxd    rcx,r12d
       add       rcx,r14
       mov       [rsp+34],r11d
       mov       r8d,r11d
       call      qword ptr [7FF81C2157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       r8d,[rsp+34]
       lea       eax,[r12+r8]
       cdq
       idiv      r13d
       mov       r12d,edx
       mov       r10d,r8d
       add       r10d,[rsp+38]
       mov       eax,r10d
       mov       edx,[rsp+3C]
       sub       edx,r8d
       test      edx,edx
       mov       r10d,eax
       jg        short M05_L04
       lea       eax,[rbp+4]
       mov       esi,[rsi+38]
       cdq
       idiv      esi
       mov       ebp,edx
       mov       rbx,[rbx]
       mov       r15d,edi
       xor       r13d,r13d
       test      edi,edi
       jle       short M05_L07
M05_L03:
       mov       r12d,esi
       sub       r12d,ebp
       cmp       r15d,r12d
       jle       short M05_L05
       jmp       near ptr M05_L10
M05_L04:
       mov       eax,edx
       jmp       near ptr M05_L01
M05_L05:
       mov       r12d,r15d
M05_L06:
       mov       r8d,r13d
       mov       ecx,r12d
       add       r8,rcx
       mov       ecx,edi
       cmp       r8,rcx
       ja        near ptr M05_L13
       mov       edx,r13d
       add       rdx,rbx
       test      r12d,r12d
       jl        near ptr M05_L13
       movsxd    rcx,ebp
       add       rcx,r14
       mov       r8d,r12d
       call      qword ptr [7FF81C2157B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       lea       eax,[r12+rbp]
       cdq
       idiv      esi
       mov       ebp,edx
       add       r13d,r12d
       sub       r15d,r12d
       test      r15d,r15d
       jg        short M05_L03
M05_L07:
       mov       eax,1
       mov       rcx,0FF9B631EFF3D
       cmp       [rsp+40],rcx
       je        short M05_L08
       call      CORINFO_HELP_FAIL_FAST
M05_L08:
       nop
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
M05_L09:
       mov       r11d,r9d
       mov       [rsp+3C],eax
       jmp       near ptr M05_L02
M05_L10:
       jmp       near ptr M05_L06
M05_L11:
       xor       eax,eax
       mov       rcx,0FF9B631EFF3D
       cmp       [rsp+40],rcx
       je        short M05_L12
       call      CORINFO_HELP_FAIL_FAST
M05_L12:
       nop
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
M05_L13:
       call      qword ptr [7FF81C427798]
       int       3
; Total bytes of code 487
```

