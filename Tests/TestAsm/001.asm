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

    mov eax, 9
    mov ebx, 10
    cmp eax, ebx
    jge L1
    push 0
    jmp L2
    L1:
        push 1
    L2:
        pop DWORD [ebp - 4]

    push DWORD [ebp - 4]
    push writelnInt
    call _printf
    add esp, 8

    leave
    ret