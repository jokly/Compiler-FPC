section .data
        readInt  : DD '%i', 0
        readStr  : DD '%s', 0
        readReal : DD '%f', 0

        writeInt  : DD '%i', 0
        writeStr  : DD '%s', 0
        writeReal : DD '%f', 0

        writelnInt  : DD '%i', 10, 0
        writelnStr  : DD '%s', 10, 0
        writelnReal : DD '%f', 10, 0


section .bss
	a: RESD 4

section .text
        global _main

        extern _printf
        extern _scanf

_main:
	push 1
	pop DWORD [a]
	push DWORD [a]
	push writelnInt
	call _printf
	add esp, 8

