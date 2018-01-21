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
	push 0x0; 0
	push 4
	push ebp
	pop eax
	pop ebx
	sub eax, ebx
	pop DWORD [eax]
	push 0xA; 10
	push 0x0; 0
	L0:
	pop eax
	pop ebx
	cmp eax, ebx
	jg L1
	push ebx
	push eax
	push 4
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
	push 4
	push ebp
	pop eax
	pop ebx
	sub eax, ebx
	pop DWORD [eax]
	pop eax
	add eax, 1
	push eax
	jmp L0
	L1:
	push 4
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