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
	sub esp, 4
	sub esp, 4
	push 0x3; 3
	pop DWORD [ebp - 4]
	push DWORD [ebp - 4]
	push 0x3; 3
	push 0x2; 2
	pop ebx
	pop eax
	mul ebx
	push eax
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop DWORD [ebp - 8]
	push DWORD [ebp - 8]
	pop DWORD [ebp - 4]
	push DWORD [ebp - 4]
	push writeInt
	call _printf
	add esp, 8
        leave
        ret