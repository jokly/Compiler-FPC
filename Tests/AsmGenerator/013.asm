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
	push 0x10; 16
	push 0x2; 2
	pop ebx
	pop eax
	push ebx
	mov cl, [esp]
	pop ebx
	shr eax, cl
	push eax
	pop DWORD [ebp - 4]
	push DWORD [ebp - 4]
	push writelnInt
	call _printf
	add esp, 8
        leave
        ret