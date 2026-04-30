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
       mov       rcx,[rcx+48]
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
       call      qword ptr [7FF81C5DE058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L06:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L07
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5DE058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
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
       call      qword ptr [7FF81C5DE058]
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
       call      qword ptr [7FF81C5DE058]
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
       mov       rcx,[rcx+48]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rax,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rax,rcx
       jne       short M00_L04
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L05
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5DE040]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
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
       mov       rcx,rbx
       mov       rbp,[rax+40]
       call      qword ptr [rbp+20]
       test      eax,eax
       je        short M00_L01
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       call      qword ptr [rbp+28]
       test      eax,eax
       je        short M00_L01
       cmp       byte ptr [rbx+18],0
       je        short M00_L02
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5DE040]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L05:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       short M00_L02
; Total bytes of code 183
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
       mov       rax,[rsi]
       mov       rcx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rax,rcx
       jne       short M01_L02
       mov       rcx,[rbx]
       mov       eax,[rbx+8]
       test      eax,eax
       je        near ptr M01_L07
       movzx     ecx,byte ptr [rcx]
       mov       [rsi+20],rcx
M01_L01:
       cmp       byte ptr [rsi+18],0
       je        short M01_L05
       jmp       short M01_L04
M01_L02:
       mov       rcx,rsi
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M01_L03
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       call      qword ptr [rdi+28]
       test      eax,eax
       jne       short M01_L01
M01_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF81C5DE040]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L04:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L05
       mov       rdx,rbx
       call      qword ptr [7FF81C5DE040]
M01_L05:
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
; Total bytes of code 187
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
       mov       rcx,[rcx+48]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       rbp,rcx
       jne       short M00_L04
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L08
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C60E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
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
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L01
       mov       rdx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rdx
       jne       short M00_L06
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
       call      qword ptr [7FF81C60E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L08:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L02
M00_L09:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 230
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
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       rdi,rcx
       je        short M01_L01
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       jne       short M01_L02
M01_L01:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L07
       mov       rdx,rbx
       call      qword ptr [7FF81C60E058]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       mov       rdx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rdi,rdx
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
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L01
       jmp       short M01_L03
M01_L05:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L06
       mov       rdx,rbx
       call      qword ptr [7FF81C60E058]
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
; Total bytes of code 204
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
       mov       rcx,[rcx+48]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       rbp,rcx
       jne       short M00_L04
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        near ptr M00_L08
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C5EE058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
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
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L01
       mov       rdx,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rdx
       jne       short M00_L06
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
       call      qword ptr [7FF81C5EE058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L08:
       add       rbx,10
       lock inc  qword ptr [rbx]
       jmp       near ptr M00_L02
M00_L09:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 230
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
       call      qword ptr [7FF81C5EE058]
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
       call      qword ptr [7FF81C5EE058]
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

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Healthy()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+28]
       mov       rax,[rcx+48]
       test      rax,rax
       je        short M00_L03
       lea       rsi,[rax+10]
       mov       edi,[rax+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rax,offset MT_Relay.Benchmarks.ByteCounterPipe
       cmp       rbp,rax
       jne       short M00_L04
       test      edi,edi
       je        short M00_L06
       movzx     eax,byte ptr [rsi]
       mov       [rbx+20],rax
       mov       edx,1
M00_L01:
       movzx     eax,dl
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
       je        short M00_L05
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       mov       edx,eax
       jmp       short M00_L01
M00_L05:
       xor       eax,eax
       jmp       short M00_L02
M00_L06:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 139
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Reject()
       push      r14
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,[rcx+30]
       mov       rax,[rcx+48]
       test      rax,rax
       je        short M00_L03
       lea       rsi,[rax+10]
       mov       edi,[rax+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rax,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rbp,rax
       jne       short M00_L04
       xor       r14d,r14d
M00_L01:
       movzx     eax,r14b
M00_L02:
       add       rsp,30
       pop       rbx
       pop       rbp
       pop       rsi
       pop       rdi
       pop       r14
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
       je        short M00_L05
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       mov       r14d,eax
       jmp       short M00_L01
M00_L05:
       xor       eax,eax
       jmp       short M00_L02
; Total bytes of code 126
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Unhealthy()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,[rcx+38]
       mov       rcx,[rcx+48]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       [rbx],rcx
       jne       short M00_L04
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       short M00_L05
       add       rbx,10
       lock inc  qword ptr [rbx]
M00_L02:
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M00_L03:
       xor       esi,esi
       xor       edi,edi
       jmp       short M00_L00
M00_L04:
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M00_L01
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L01
       cmp       byte ptr [rbx+18],0
       je        short M00_L02
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FF81C60E070]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L05:
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FF81C60E070]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
; Total bytes of code 187
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
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rcx,offset MT_Relay.Benchmarks.ByteDeadPipe
       cmp       [rbx],rcx
       jne       short M01_L02
M01_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       short M01_L04
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
M01_L02:
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L01
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+28],xmm0
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbx]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L01
       cmp       byte ptr [rbx+18],0
       je        short M01_L03
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M01_L03
       mov       rdx,rsi
       call      qword ptr [7FF81C60E070]
M01_L03:
       nop
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
M01_L04:
       mov       rdx,rsi
       call      qword ptr [7FF81C60E070]
       nop
       add       rsp,38
       pop       rbx
       pop       rsi
       ret
; Total bytes of code 161
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Reject()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+40]
       mov       rcx,[rcx+48]
       test      rcx,rcx
       je        short M00_L03
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rbp,rcx
       jne       short M00_L04
M00_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       short M00_L05
       add       rbx,10
       lock inc  qword ptr [rbx]
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
       je        short M00_L01
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,rbx
       mov       rax,[rbp+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M00_L01
       cmp       byte ptr [rbx+18],0
       je        short M00_L02
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C60E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
M00_L05:
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF81C60E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
; Total bytes of code 186
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
       mov       rbx,rcx
       mov       rsi,rdx
       mov       rdi,[rbx]
       mov       rcx,offset MT_Relay.Benchmarks.ByteRejectPipe
       cmp       rdi,rcx
       jne       short M01_L02
M01_L01:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       jne       short M01_L04
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L02:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L01
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L01
       cmp       byte ptr [rbx+18],0
       je        short M01_L03
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M01_L03
       mov       rdx,rsi
       call      qword ptr [7FF81C60E058]
M01_L03:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L04:
       mov       rdx,rsi
       call      qword ptr [7FF81C60E058]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 162
```

