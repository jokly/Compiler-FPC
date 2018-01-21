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
	push 0x0; 0
	push 4
	push ebp
	pop eax
	pop ebx
	sub eax, ebx
	pop DWORD [eax]
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
	je L7
	jmp L8
	L7:
	push 0x1; 1
	push 4
	push ebp
	pop eax
	pop ebx
	sub eax, ebx
	pop DWORD [eax]
	jmp L9
	L8:
	push 0x1; 1
	push 0x1; 1
	pop ebx
	pop eax
	cmp eax, ebx
	jge L2
	push 0
	jmp L3
	L2:
	push 1
	L3:
	pop eax
	push eax
	pop eax
	cmp eax, 1
	je L4
	jmp L5
	L4:
	push 0x1; 1
	pop eax
	neg eax
	push eax
	push 4
	push ebp
	pop eax
	pop ebx
	sub eax, ebx
	pop DWORD [eax]
	jmp L6
	L5:
	L6:
	L9:
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