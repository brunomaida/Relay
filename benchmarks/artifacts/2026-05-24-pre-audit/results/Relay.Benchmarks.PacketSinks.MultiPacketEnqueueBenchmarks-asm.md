## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi_Packet_Enqueue()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+18]
       test      rcx,rcx
       je        short M00_L02
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E4513F0]; Relay.MultiSink.get_IsHealthy()
       test      eax,eax
       je        short M00_L04
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       call      qword ptr [7FFB0E4513F8]; Relay.MultiSink.Accept(System.ReadOnlySpan`1<Byte>)
       test      eax,eax
       je        short M00_L04
       cmp       byte ptr [rbx+18],0
       jne       short M00_L03
M00_L01:
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L02:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L01
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FFB0E47DDE8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L01
M00_L04:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L05
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FFB0E47DDE8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L01
M00_L05:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L01
; Total bytes of code 168
```
```assembly
; Relay.MultiSink.get_IsHealthy()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,20
       mov       rbx,[rcx+20]
       xor       esi,esi
       cmp       dword ptr [rbx+8],0
       jle       short M01_L03
       mov       rdi,offset MT_Relay.Benchmarks.ByteCounterPipe
M01_L00:
       mov       rcx,[rbx+rsi*8+10]
       cmp       [rcx],rdi
       jne       short M01_L02
M01_L01:
       mov       eax,1
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       mov       rax,[rcx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L01
       inc       esi
       cmp       [rbx+8],esi
       jg        short M01_L00
M01_L03:
       xor       eax,eax
       add       rsp,20
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 83
```
```assembly
; Relay.MultiSink.Accept(System.ReadOnlySpan`1<Byte>)
       push      r15
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       [rsp+30],rax
       mov       rbx,rdx
       mov       rsi,[rcx+20]
       mov       edi,[rsi+8]
       test      edi,edi
       jle       short M02_L03
       mov       rbp,offset MT_Relay.Benchmarks.ByteCounterPipe
       add       rsi,10
M02_L00:
       mov       r14,[rsi]
       mov       rdx,[r14]
       cmp       rdx,rbp
       jne       short M02_L04
       mov       rax,[rbx]
       mov       ecx,[rbx+8]
       test      ecx,ecx
       je        near ptr M02_L08
       movzx     eax,byte ptr [rax]
       mov       [r14+20],rax
M02_L01:
       cmp       byte ptr [r14+18],0
       jne       short M02_L05
M02_L02:
       add       rsi,8
       dec       edi
       jne       short M02_L00
M02_L03:
       mov       eax,1
       add       rsp,38
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       ret
M02_L04:
       mov       rcx,r14
       mov       r15,[rdx+40]
       call      qword ptr [r15+20]
       test      eax,eax
       je        short M02_L06
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       mov       rcx,r14
       call      qword ptr [r15+28]
       test      eax,eax
       je        short M02_L06
       jmp       short M02_L01
M02_L05:
       mov       rcx,[r14+8]
       test      rcx,rcx
       je        short M02_L02
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       call      qword ptr [7FFB0E47DDE8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M02_L02
M02_L06:
       mov       rcx,[r14+8]
       test      rcx,rcx
       je        short M02_L07
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       call      qword ptr [7FFB0E47DDE8]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       near ptr M02_L02
M02_L07:
       add       r14,10
       lock inc  qword ptr [r14]
       jmp       near ptr M02_L02
M02_L08:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 246
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
       mov       rsi,rcx
       mov       rbx,rdx
       mov       rax,[rsi]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rax,rcx
       jne       short M03_L02
       mov       rcx,[rbx]
       mov       eax,[rbx+8]
       test      eax,eax
       je        near ptr M03_L07
       movzx     ecx,byte ptr [rcx]
       mov       [rsi+20],rcx
M03_L01:
       cmp       byte ptr [rsi+18],0
       je        short M03_L04
       jmp       short M03_L03
M03_L02:
       mov       rcx,rsi
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M03_L05
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       call      qword ptr [rdi+28]
       test      eax,eax
       je        short M03_L05
       jmp       short M03_L01
M03_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M03_L04
       mov       rdx,rbx
       call      qword ptr [7FFB0E47DDE8]
M03_L04:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M03_L06
       mov       rdx,rbx
       call      qword ptr [7FFB0E47DDE8]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L06:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M03_L07:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 189
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi2_Packet_Enqueue()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,[rcx+10]
       mov       rcx,[rcx+18]
       test      rcx,rcx
       je        short M00_L02
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,rbx
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E4717F8]; Relay.Multi2PacketSink`2[[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
       test      eax,eax
       je        short M00_L04
       mov       rcx,[rbx+20]
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E49DE00]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       mov       rcx,[rbx+28]
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       cmp       [rcx],ecx
       call      qword ptr [7FFB0E49DE00]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       cmp       byte ptr [rbx+18],0
       jne       short M00_L03
M00_L01:
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L02:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L03:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L01
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FFB0E49DE00]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L01
M00_L04:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L05
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FFB0E49DE00]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L01
M00_L05:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L01
; Total bytes of code 193
```
```assembly
; Relay.Multi2PacketSink`2[[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].get_IsHealthy()
       push      rbx
       sub       rsp,20
       mov       rbx,rcx
       mov       rcx,[rbx+20]
       mov       rax,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       [rcx],rax
       jne       short M01_L01
M01_L00:
       mov       eax,1
       add       rsp,20
       pop       rbx
       ret
M01_L01:
       mov       rax,[rcx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L00
       mov       rcx,[rbx+28]
       mov       rax,[rcx]
       mov       rax,[rax+40]
       add       rsp,20
       pop       rbx
       jmp       qword ptr [rax+20]
; Total bytes of code 72
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M02_L00:
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       [rsp+28],rax
       mov       rsi,rcx
       mov       rbx,rdx
       mov       rdi,[rsi]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rdi,rcx
       jne       short M02_L02
       mov       rcx,[rbx]
       mov       eax,[rbx+8]
       test      eax,eax
       je        near ptr M02_L07
       movzx     ecx,byte ptr [rcx]
       mov       [rsi+20],rcx
M02_L01:
       cmp       byte ptr [rsi+18],0
       je        short M02_L04
       jmp       short M02_L03
M02_L02:
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M02_L05
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M02_L05
       jmp       short M02_L01
M02_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M02_L04
       mov       rdx,rbx
       call      qword ptr [7FFB0E49DE00]
M02_L04:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M02_L06
       mov       rdx,rbx
       call      qword ptr [7FFB0E49DE00]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L06:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M02_L07:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 193
```

