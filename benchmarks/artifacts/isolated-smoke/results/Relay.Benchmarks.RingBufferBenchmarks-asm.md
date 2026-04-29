## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

## .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
```assembly
; Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty()
       mov       rcx,[rcx+8]
       mov       rdx,[rcx+60]
       cmp       rdx,[rcx+0E0]
       jl        short M00_L01
       mov       rax,[rcx+160]
       mov       [rcx+0E0],rax
       cmp       rdx,rax
       jl        short M00_L01
       xor       eax,eax
M00_L00:
       ret
M00_L01:
       mov       rax,[rcx+8]
       movsxd    r8,dword ptr [rcx+10]
       and       r8,rdx
       shl       r8,6
       movsx     rax,byte ptr [rax+r8]
       inc       rdx
       mov       [rcx+60],rdx
       mov       eax,1
       jmp       short M00_L00
; Total bytes of code 73
```

