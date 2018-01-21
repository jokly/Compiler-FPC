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
	push 0x5; 5
	push 0x1; 1
	pop ebx
	pop eax
	add eax, ebx
	push eax
	push 0x3; 3
	pop ebx
	pop eax
	div ebx
	push eax
	push 0x2; 2
	pop ebx
	pop eax
	mul ebx
	push eax
	pop DWORD [ebp - 4]
	push DWORD [ebp - 4]
	push writeInt
	call _printf
	add esp, 8
	push DWORD [ebp - 4]
	push 0x1; 1
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop DWORD [ebp - 4]
	push DWORD [ebp - 4]
	push writeInt
	call _printf
	add esp, 8
        leave
        ret