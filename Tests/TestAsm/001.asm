section .data
        readInt  : DD '%i', 0
        readStr  : DD '%s', 0
        readReal : DD '%f', 0

        writeInt  : DD '%i', 0
        writeStr  : DD '%c', 0
        writeReal : DD '%f', 0

        writelnInt  : DD '%i', 10, 0
        writelnStr  : DD '%c', 10, 0
        writelnReal : DD '%f', 10, 0


section .bss

section .text
        global _main

        extern _printf
        extern _scanf

_main:
        push ebp
        mov ebp, esp
        sub esp, 4
        push 0x0; 0
        pop DWORD [ebp - 4]
        push 0x1; 1
        push 0x1; 1
        pop ebx
        pop eax
        cmp eax, ebx
        jne L0
        push 0
        jmp L1
        L0:
        push 1
        L1:
        pop eax
        push eax
        pop eax
        cmp eax, 1
        je L2
        jmp L3
        L2:
        push 0x1; 1
        pop DWORD [ebp - 4]
        jmp L4
        L3:
        push 0x1; 1
        pop eax
        neg eax
        push eax
        pop DWORD [ebp - 4]
        L4:
        push DWORD [ebp - 4]
        push writeInt
        call _printf
        add esp, 8
        leave
        ret