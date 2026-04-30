## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       xor       eax,eax
       mov       [rsp+28],rax
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+30],ymm4
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        short M00_L04
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       ecx,[rbx+30]
       mov       eax,[rbx+28]
       cmp       ecx,eax
       jge       short M00_L02
       mov       [rsp+40],rsi
       mov       [rsp+48],edi
       mov       ebp,[rsp+48]
       add       ebp,3
       and       ebp,0FFFFFFFC
       add       ebp,4
       lea       edx,[rcx+rbp]
       cmp       edx,eax
       jle       short M00_L05
       xor       r14d,r14d
M00_L01:
       xor       ecx,ecx
       mov       [rsp+38],rcx
       test      r14d,r14d
       jne       short M00_L06
M00_L02:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       near ptr M00_L07
       add       rbx,10
       lock inc  qword ptr [rbx]
M00_L03:
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L04:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L05:
       mov       rax,[rbx+20]
       movsxd    rcx,ecx
       mov       edx,[rsp+48]
       mov       [rax+rcx],edx
       lea       rcx,[rsp+40]
       call      qword ptr [7FF81C60DF38]; System.ReadOnlySpan`1[[System.Byte, System.Private.CoreLib]].GetPinnableReference()
       mov       [rsp+38],rax
       mov       rdx,[rsp+38]
       mov       r8,[rbx+20]
       movsxd    rcx,dword ptr [rbx+30]
       lea       rcx,[r8+rcx+4]
       mov       r8d,[rsp+48]
       call      qword ptr [7FF81C2457B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       xor       edx,edx
       mov       [rsp+38],rdx
       add       [rbx+30],ebp
       mov       r14d,1
       jmp       short M00_L01
M00_L06:
       cmp       byte ptr [rbx+18],0
       je        short M00_L03
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C60DEC0]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L03
M00_L07:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C60DEC0]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L03
; Total bytes of code 281
```
```assembly
; System.ReadOnlySpan`1[[System.Byte, System.Private.CoreLib]].GetPinnableReference()
       xor       eax,eax
       cmp       dword ptr [rcx+8],0
       je        short M01_L00
       mov       rax,[rcx]
M01_L00:
       ret
; Total bytes of code 12
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M02_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M02_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M02_L08
       cmp       r8,40
       ja        short M02_L03
M02_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M02_L07
M02_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M02_L02:
       ret
M02_L03:
       cmp       r8,800
       ja        near ptr M02_L12
       cmp       r8,100
       jae       short M02_L06
M02_L04:
       mov       r9,r8
       shr       r9,6
M02_L05:
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
       jne       short M02_L05
       and       r8,3F
       cmp       r8,10
       ja        short M02_L00
       jmp       short M02_L01
M02_L06:
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
       jmp       short M02_L04
M02_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M02_L01
M02_L08:
       test      r8b,18
       jne       short M02_L10
       test      r8b,4
       jne       short M02_L09
       test      r8,r8
       je        near ptr M02_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M02_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M02_L02
M02_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M02_L02
M02_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M02_L02
M02_L11:
       cmp       rcx,rdx
       je        short M02_L13
M02_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FF86E2A1940]
       jmp       qword ptr [rax]
M02_L13:
       cmp       [rdx],dl
       jmp       near ptr M02_L02
; Total bytes of code 358
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M03_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,40
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   xmmword ptr [rsp+28],xmm4
       xor       eax,eax
       mov       [rsp+38],rax
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Sinks.RamSink
       cmp       rdi,rcx
       jne       near ptr M03_L03
       mov       ecx,[rbx+30]
       cmp       ecx,[rbx+28]
       jge       short M03_L01
       mov       rcx,[rsi]
       mov       r8d,[rsi+8]
       xor       edx,edx
       mov       [rsp+38],rdx
       lea       edi,[r8+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       edx,edi
       add       edx,[rbx+30]
       cmp       edx,[rbx+28]
       jle       short M03_L02
M03_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       near ptr M03_L06
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L02:
       mov       rdx,[rbx+20]
       movsxd    rax,dword ptr [rbx+30]
       mov       [rdx+rax],r8d
       xor       edx,edx
       test      r8d,r8d
       cmovne    rdx,rcx
       mov       [rsp+38],rdx
       mov       rcx,[rbx+20]
       movsxd    rax,dword ptr [rbx+30]
       lea       rcx,[rcx+rax+4]
       mov       r8d,r8d
       call      qword ptr [7FF81C2457B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       xor       ecx,ecx
       mov       [rsp+38],rcx
       add       [rbx+30],edi
       jmp       short M03_L04
M03_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M03_L01
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        near ptr M03_L01
M03_L04:
       cmp       byte ptr [rbx+18],0
       je        short M03_L05
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M03_L05
       mov       rdx,rsi
       call      qword ptr [7FF81C60DEC0]
M03_L05:
       nop
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L06:
       mov       rdx,rsi
       call      qword ptr [7FF81C60DEC0]
       nop
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 281
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,50
       xor       eax,eax
       mov       [rsp+28],rax
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rsp+30],ymm4
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+10]
       test      rcx,rcx
       je        short M00_L04
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       ecx,[rbx+30]
       mov       eax,[rbx+28]
       cmp       ecx,eax
       jge       short M00_L02
       mov       [rsp+40],rsi
       mov       [rsp+48],edi
       mov       ebp,[rsp+48]
       add       ebp,3
       and       ebp,0FFFFFFFC
       add       ebp,4
       lea       edx,[rcx+rbp]
       cmp       edx,eax
       jle       short M00_L05
       xor       r14d,r14d
M00_L01:
       xor       ecx,ecx
       mov       [rsp+38],rcx
       test      r14d,r14d
       jne       short M00_L06
M00_L02:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       near ptr M00_L07
       add       rbx,10
       lock inc  qword ptr [rbx]
M00_L03:
       add       rsp,50
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       ret
M00_L04:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L05:
       mov       rax,[rbx+20]
       movsxd    rcx,ecx
       mov       edx,[rsp+48]
       mov       [rax+rcx],edx
       lea       rcx,[rsp+40]
       call      qword ptr [7FF81C5FDF20]; System.ReadOnlySpan`1[[System.Byte, System.Private.CoreLib]].GetPinnableReference()
       mov       [rsp+38],rax
       mov       rdx,[rsp+38]
       mov       r8,[rbx+20]
       movsxd    rcx,dword ptr [rbx+30]
       lea       rcx,[r8+rcx+4]
       mov       r8d,[rsp+48]
       call      qword ptr [7FF81C2357B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       xor       edx,edx
       mov       [rsp+38],rdx
       add       [rbx+30],ebp
       mov       r14d,1
       jmp       short M00_L01
M00_L06:
       cmp       byte ptr [rbx+18],0
       je        short M00_L03
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5FDEA8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L03
M00_L07:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5FDEA8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M00_L03
; Total bytes of code 281
```
```assembly
; System.ReadOnlySpan`1[[System.Byte, System.Private.CoreLib]].GetPinnableReference()
       xor       eax,eax
       cmp       dword ptr [rcx+8],0
       je        short M01_L00
       mov       rax,[rcx]
M01_L00:
       ret
; Total bytes of code 12
```
```assembly
; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       mov       rax,rcx
       sub       rax,rdx
       cmp       rax,r8
       jb        near ptr M02_L11
       mov       rax,rdx
       sub       rax,rcx
       cmp       rax,r8
       jb        near ptr M02_L11
       lea       rax,[rdx+r8]
       lea       r10,[rcx+r8]
       cmp       r8,10
       jbe       near ptr M02_L08
       cmp       r8,40
       ja        short M02_L03
M02_L00:
       movups    xmm0,[rdx]
       movups    [rcx],xmm0
       cmp       r8,20
       jbe       short M02_L01
       movups    xmm0,[rdx+10]
       movups    [rcx+10],xmm0
       cmp       r8,30
       ja        near ptr M02_L07
M02_L01:
       movups    xmm0,[rax-10]
       movups    [r10-10],xmm0
M02_L02:
       ret
M02_L03:
       cmp       r8,800
       ja        near ptr M02_L12
       cmp       r8,100
       jae       short M02_L06
M02_L04:
       mov       r9,r8
       shr       r9,6
M02_L05:
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
       jne       short M02_L05
       and       r8,3F
       cmp       r8,10
       ja        short M02_L00
       jmp       short M02_L01
M02_L06:
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
       jmp       short M02_L04
M02_L07:
       movups    xmm0,[rdx+20]
       movups    [rcx+20],xmm0
       jmp       near ptr M02_L01
M02_L08:
       test      r8b,18
       jne       short M02_L10
       test      r8b,4
       jne       short M02_L09
       test      r8,r8
       je        near ptr M02_L02
       movzx     edx,byte ptr [rdx]
       mov       [rcx],dl
       test      r8b,2
       je        near ptr M02_L02
       movsx     rcx,word ptr [rax-2]
       mov       [r10-2],cx
       jmp       near ptr M02_L02
M02_L09:
       mov       edx,[rdx]
       mov       [rcx],edx
       mov       ecx,[rax-4]
       mov       [r10-4],ecx
       jmp       near ptr M02_L02
M02_L10:
       mov       rdx,[rdx]
       mov       [rcx],rdx
       mov       rcx,[rax-8]
       mov       [r10-8],rcx
       jmp       near ptr M02_L02
M02_L11:
       cmp       rcx,rdx
       je        short M02_L13
M02_L12:
       cmp       [rcx],cl
       cmp       [rdx],dl
       lea       rax,[7FF86E2A1940]
       jmp       qword ptr [rax]
M02_L13:
       cmp       [rdx],dl
       jmp       near ptr M02_L02
; Total bytes of code 358
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M03_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,40
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   xmmword ptr [rsp+28],xmm4
       xor       eax,eax
       mov       [rsp+38],rax
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Sinks.RamSink
       cmp       rdi,rcx
       jne       near ptr M03_L03
       mov       ecx,[rbx+30]
       cmp       ecx,[rbx+28]
       jge       short M03_L01
       mov       rcx,[rsi]
       mov       r8d,[rsi+8]
       xor       edx,edx
       mov       [rsp+38],rdx
       lea       edi,[r8+3]
       and       edi,0FFFFFFFC
       add       edi,4
       mov       edx,edi
       add       edx,[rbx+30]
       cmp       edx,[rbx+28]
       jle       short M03_L02
M03_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       near ptr M03_L06
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L02:
       mov       rdx,[rbx+20]
       movsxd    rax,dword ptr [rbx+30]
       mov       [rdx+rax],r8d
       xor       edx,edx
       test      r8d,r8d
       cmovne    rdx,rcx
       mov       [rsp+38],rdx
       mov       rcx,[rbx+20]
       movsxd    rax,dword ptr [rbx+30]
       lea       rcx,[rcx+rax+4]
       mov       r8d,r8d
       call      qword ptr [7FF81C2357B8]; System.SpanHelpers.Memmove(Byte ByRef, Byte ByRef, UIntPtr)
       xor       ecx,ecx
       mov       [rsp+38],rcx
       add       [rbx+30],edi
       jmp       short M03_L04
M03_L03:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M03_L01
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        near ptr M03_L01
M03_L04:
       cmp       byte ptr [rbx+18],0
       je        short M03_L05
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M03_L05
       mov       rdx,rsi
       call      qword ptr [7FF81C5FDEA8]
M03_L05:
       nop
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L06:
       mov       rdx,rsi
       call      qword ptr [7FF81C5FDEA8]
       nop
       add       rsp,40
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 281
```

