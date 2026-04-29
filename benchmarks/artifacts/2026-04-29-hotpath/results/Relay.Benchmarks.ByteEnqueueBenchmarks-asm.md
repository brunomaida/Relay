## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Healthy()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+28]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rcx
       jne       short M00_L04
       test      edi,edi
       je        near ptr M00_L08
       movzx     ecx,byte ptr [rsi]
       mov       [rbx+20],rcx
M00_L01:
       cmp       byte ptr [rbx+18],0
       jne       short M00_L05
M00_L02:
       add       rsp,38
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L03:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L04:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L06
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L06
       jmp       short M00_L01
M00_L05:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D9E130]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L06:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L07
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D9E130]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L07:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L02
M00_L08:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 214
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M01_L00:
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
       jne       short M01_L02
       mov       rcx,[rbx]
       mov       eax,[rbx+8]
       test      eax,eax
       je        near ptr M01_L07
       movzx     ecx,byte ptr [rcx]
       mov       [rsi+20],rcx
M01_L01:
       cmp       byte ptr [rsi+18],0
       je        short M01_L04
       jmp       short M01_L03
M01_L02:
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L05
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L05
       jmp       short M01_L01
M01_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L04
       mov       rdx,rbx
       call      qword ptr [7FF7C6D9E130]
M01_L04:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF7C6D9E130]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L06:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L07:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 193
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_AcceptReject()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+10]
       mov       rcx,[rcx+28]
       test      rcx,rcx
       je        short M00_L04
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rcx
       jne       short M00_L05
M00_L01:
       mov       rdx,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rbp,rdx
       jne       short M00_L06
M00_L02:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L07
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D5DF68]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M00_L03:
       nop
       add       rsp,38
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L04:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L05:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L02
       jmp       short M00_L01
M00_L06:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L02
       cmp       byte ptr [rbx+18],0
       je        short M00_L03
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D5DF68]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L03
M00_L07:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L03
; Total bytes of code 204
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M01_L00:
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
       mov       rcx,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rdi,rcx
       je        short M01_L01
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L04
M01_L01:
       mov       rdx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rdi,rdx
       jne       short M01_L03
       mov       rdx,[rbx]
       mov       ecx,[rbx+8]
       test      ecx,ecx
       je        short M01_L08
       movzx     edx,byte ptr [rdx]
       mov       [rsi+20],rdx
M01_L02:
       cmp       byte ptr [rsi+18],0
       je        short M01_L06
       jmp       short M01_L05
M01_L03:
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       jne       short M01_L02
M01_L04:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L07
       mov       rdx,rbx
       call      qword ptr [7FF7C6D5DF68]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF7C6D5DF68]
M01_L06:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L07:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L08:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 202
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_HeadUnhealthy()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+18]
       mov       rcx,[rcx+28]
       test      rcx,rcx
       je        short M00_L04
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rcx
       jne       short M00_L05
       test      edi,edi
       je        near ptr M00_L10
       movzx     ecx,byte ptr [rsi]
       mov       [rbx+20],rcx
M00_L01:
       cmp       byte ptr [rbx+18],0
       je        short M00_L03
       jmp       short M00_L08
M00_L02:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D8E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M00_L03:
       nop
       add       rsp,38
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L04:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L05:
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M00_L07
M00_L06:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L09
       jmp       short M00_L02
M00_L07:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L06
       jmp       short M00_L01
M00_L08:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D8E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L03
M00_L09:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L03
M00_L10:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 214
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M01_L00:
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
       jne       short M01_L02
       mov       rcx,[rbx]
       mov       eax,[rbx+8]
       test      eax,eax
       je        near ptr M01_L08
       movzx     ecx,byte ptr [rcx]
       mov       [rsi+20],rcx
M01_L01:
       cmp       byte ptr [rsi+18],0
       je        short M01_L06
       jmp       short M01_L05
M01_L02:
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L04
M01_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L07
       mov       rdx,rbx
       call      qword ptr [7FF7C6D8E058]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L04:
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L03
       jmp       short M01_L01
M01_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF7C6D8E058]
M01_L06:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L07:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L08:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 193
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth3_Byte_AllUnhealthy()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+20]
       mov       rcx,[rcx+28]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rcx
       je        short M00_L04
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M00_L06
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L08
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D5E070]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M00_L02:
       nop
       add       rsp,38
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       ret
M00_L03:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L04:
       test      edi,edi
       je        short M00_L09
       movzx     edx,byte ptr [rsi]
       mov       [rbx+20],rdx
M00_L05:
       cmp       byte ptr [rbx+18],0
       je        short M00_L02
       jmp       short M00_L07
M00_L06:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L01
       jmp       short M00_L05
M00_L07:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C6D5E070]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L08:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L02
M00_L09:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 208
```
```assembly
; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M01_L00:
       push      rsi
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       [rsp+30],rax
       mov       rsi,rcx
       mov       rbx,rdx
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       [rsi],rcx
       jne       short M01_L02
M01_L01:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        near ptr M01_L07
       mov       rdx,rbx
       call      qword ptr [7FF7C6D5E070]
       nop
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
M01_L02:
       mov       rcx,rsi
       mov       rax,[rsi]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L01
       mov       rdx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       [rsi],rdx
       jne       short M01_L04
       mov       rdx,[rbx]
       mov       ecx,[rbx+8]
       test      ecx,ecx
       je        short M01_L08
       movzx     edx,byte ptr [rdx]
       mov       [rsi+20],rdx
M01_L03:
       cmp       byte ptr [rsi+18],0
       je        short M01_L06
       jmp       short M01_L05
M01_L04:
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       mov       rcx,rsi
       mov       rax,[rsi]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L01
       jmp       short M01_L03
M01_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF7C6D5E070]
M01_L06:
       nop
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
M01_L07:
       add       rsi,10
       lock inc  qword ptr [rsi]
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
M01_L08:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 207
```

