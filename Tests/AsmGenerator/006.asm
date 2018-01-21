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
	push 0x40133333; 2,3
	fld DWORD [esp]
	pop eax
	fstp DWORD [ebp - 4]
	push DWORD [ebp - 4]
	fld DWORD [esp]
	pop eax
	push 0x3F800000; 1
	fld DWORD [esp]
	pop eax
	faddp
	push 0x3F333333; 0,7
	fld DWORD [esp]
	pop eax
	fsubp
	push 0x0; 0
	fld DWORD [esp]
	pop eax
	fmulp
	push 0x3F800000; 1
	fld DWORD [esp]
	pop eax
	fdivp
	fstp DWORD [ebp - 4]
	push DWORD [ebp - 4]
	fld DWORD [esp]
	pop eax
	sub esp, 8
	fstp QWORD [esp]
	push writeReal
	call _printf
	add esp, 12
        leave
        ret