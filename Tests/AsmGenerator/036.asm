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
	sub esp, 4
	push 0x0; 0
	pop DWORD [ebp - 4]
	push 0x0; 0
	push 0xA; 10
	L0:
	pop eax
	pop ebx
	cmp eax, ebx
	jl L1
	push ebx
	push eax
	push DWORD [ebp - 4]
	push 0x1; 1
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop DWORD [ebp - 4]
	pop eax
	sub eax, 1
	push eax
	jmp L0
	L1:
	push DWORD [ebp - 4]
	push writeInt
	call _printf
	add esp, 8
        leave
        ret