section .data
        readInt  : DD '%i', 0
        readStr  : DD '%s', 0
        readReal : DD '%f', 0

        writeInt  : DD '%i', 0
        writeStr  : DD '%c', 0
        writeReal : DD '%f', 0

        writelnInt  : DB '%i', 10, 0
        writelnStr  : DB '%c', 10, 0
        writelnReal : DB '%f', 10, 0


section .bss

section .text
        global _main

        extern _printf
        extern _scanf

_main:
        push ebp
        mov ebp, esp
        push 0x0; 0
        push 0xA; 10
        pop eax
        pop ebx
        sub eax, ebx
        push 4
        pop ebx
        mul eax
        sub esp, eax
        sub esp, 4
        push 0x1; 1
        push 0x0; 0
        pop eax
        push 4
        pop ebx
        mul ebx
        push eax
        push ebp
        pop eax
        pop ebx
        sub eax, ebx
        pop DWORD [eax]
        L2:
        push 0x0; 0
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push 0xA; 10
        pop ebx
        pop eax
        cmp eax, ebx
        jle L0
        push 0
        jmp L1
        L0:
        push 1
        L1:
        pop eax
        push eax
        pop eax
        cmp eax, 1
        jne L3
        push 0x0; 0
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push 0x1; 1
        pop ebx
        pop eax
        sub eax, ebx
        push eax
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push 0x1; 1
        pop ebx
        pop eax
        add eax, ebx
        push eax
        push 0x0; 0
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        pop eax
        push 4
        pop ebx
        mul ebx
        push eax
        push ebp
        pop eax
        pop ebx
        sub eax, ebx
        pop DWORD [eax]
        push 0x0; 0
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push writelnInt
        call _printf
        add esp, 8
        push 0x0; 0
        push 4
        pop eax
        pop ebx
        mul ebx
        push eax
        pop ebx
        push ebp
        pop eax
        sub eax, ebx
        push DWORD [eax]
        push 0x1; 1
        pop ebx
        pop eax
        add eax, ebx
        push eax
        push 0x0; 0
        pop eax
        push 4
        pop ebx
        mul ebx
        push eax
        push ebp
        pop eax
        pop ebx
        sub eax, ebx
        pop DWORD [eax]
        jmp L2
        L3:
        leave
        ret