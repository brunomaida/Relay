## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Pass()
       push      rdi
       push      rsi
       push      rbp
       push      rbx
       sub       rsp,38
       xor       eax,eax
       mov       [rsp+28],rax
       mov       rbx,[rcx+8]
       mov       rcx,[rcx+18]
       test      rcx,rcx
       je        short M00_L04
       lea       rsi,[rcx+10]
       mov       edi,[rcx+8]
M00_L00:
       mov       rbp,[rbx+20]
       mov       rcx,7FF7C73CB918
       cmp       [rbp+18],rcx
       jne       short M00_L05
M00_L01:
       mov       rcx,[rbx+28]
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       cmp       [rcx],ecx
       call      qword ptr [7FF7C746E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
M00_L02:
       cmp       byte ptr [rbx+18],0
       jne       short M00_L06
M00_L03:
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
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       mov       rcx,[rbp+8]
       call      qword ptr [rbp+18]
       test      eax,eax
       je        short M00_L02
       jmp       short M00_L01
M00_L06:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L03
       mov       [rsp+28],rsi
       mov       [rsp+30],edi
       lea       rdx,[rsp+28]
       call      qword ptr [7FF7C746E058]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L03
; Total bytes of code 160
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
       je        short M01_L04
       jmp       short M01_L03
M01_L02:
       mov       rcx,rsi
       mov       rdi,[rax+40]
       call      qword ptr [rdi+20]
       test      eax,eax
       je        short M01_L05
       vmovdqu   xmm0,xmmword ptr [rbx]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rsi
       call      qword ptr [rdi+28]
       test      eax,eax
       je        short M01_L05
       jmp       short M01_L01
M01_L03:
       mov       rcx,[rsi+8]
       test      rcx,rcx
       je        short M01_L04
       mov       rdx,rbx
       call      qword ptr [7FF7C746E058]
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
       call      qword ptr [7FF7C746E058]
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
; Total bytes of code 189
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Reject()
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,[rcx+10]
       mov       rdx,[rcx+18]
       test      rdx,rdx
       je        short M00_L03
       lea       rsi,[rdx+10]
       mov       edi,[rdx+8]
M00_L00:
       mov       rax,[rbx+20]
       mov       rdx,7FF7C739B858
       cmp       [rax+18],rdx
       jne       short M00_L04
M00_L01:
       cmp       byte ptr [rbx+18],0
       jne       short M00_L05
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
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       mov       rcx,[rax+8]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M00_L01
       mov       rcx,[rbx+28]
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       cmp       [rcx],ecx
       call      qword ptr [7FF7C743DE60]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L01
M00_L05:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M00_L02
       mov       [rsp+20],rsi
       mov       [rsp+28],edi
       lea       rdx,[rsp+20]
       call      qword ptr [7FF7C743DE60]; Relay.PacketSink.Enqueue(System.ReadOnlySpan`1<Byte>)
       jmp       short M00_L02
; Total bytes of code 158
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
       mov       rdx,offset MT_Relay.FilterSink
       cmp       rdi,rdx
       jne       near ptr M01_L05
       mov       rax,[rbx+20]
       mov       rdx,7FF7C739B858
       cmp       [rax+18],rdx
       jne       short M01_L02
M01_L01:
       cmp       byte ptr [rbx+18],0
       je        near ptr M01_L07
       jmp       near ptr M01_L06
M01_L02:
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,[rax+8]
       call      qword ptr [rax+18]
       test      eax,eax
       je        short M01_L01
       mov       rdi,[rbx+28]
       mov       rcx,rdi
       mov       rax,[rdi]
       mov       rax,[rax+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L03
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rdi
       mov       rax,[rdi]
       mov       rax,[rax+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L03
       cmp       byte ptr [rdi+18],0
       je        short M01_L01
       mov       rcx,[rdi+8]
       test      rcx,rcx
       je        short M01_L01
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       call      qword ptr [7FF7C743DE60]
       jmp       near ptr M01_L01
M01_L03:
       mov       rcx,[rdi+8]
       test      rcx,rcx
       je        short M01_L04
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       call      qword ptr [7FF7C743DE60]
       jmp       near ptr M01_L01
M01_L04:
       add       rdi,10
       lock inc  qword ptr [rdi]
       jmp       near ptr M01_L01
M01_L05:
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+20]
       test      eax,eax
       je        short M01_L08
       vmovdqu   xmm0,xmmword ptr [rsi]
       vmovdqu   xmmword ptr [rsp+20],xmm0
       lea       rdx,[rsp+20]
       mov       rcx,rbx
       mov       rax,[rdi+40]
       call      qword ptr [rax+28]
       test      eax,eax
       je        short M01_L08
       jmp       near ptr M01_L01
M01_L06:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M01_L07
       mov       rdx,rsi
       call      qword ptr [7FF7C743DE60]
M01_L07:
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L08:
       mov       rcx,[rbx+8]
       test      rcx,rcx
       je        short M01_L09
       mov       rdx,rsi
       call      qword ptr [7FF7C743DE60]
       nop
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
M01_L09:
       add       rbx,10
       lock inc  qword ptr [rbx]
       add       rsp,30
       pop       rbx
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 368
```

