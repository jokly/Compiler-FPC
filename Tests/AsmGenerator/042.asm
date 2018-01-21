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
	push 0x1; 1
	pop eax
	pop ebx
	sub eax, ebx
	push 4
	pop ebx
	mul eax
	sub esp, eax
	sub esp, 4
	push 0x5; 5
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
	push 0xA; 10
	push 0x1; 1
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
	push writeInt
	call _printf
	add esp, 8
	push 0x1; 1
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
	push writeInt
	call _printf
	add esp, 8
        leave
        ret