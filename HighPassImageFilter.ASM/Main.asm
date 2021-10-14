.data
NetSize DWORD 3
;Masks DWORD 0, -1, 0, -1, 5, -1, 0, -1, 0

.code

DllEntry PROC hInstDLL:DWORD, reason:DWORD, reserved1:DWORD
	mov eax, 1 ; TRUE
	ret
DllEntry ENDP

; Procedura sumuje elementy w tablicy liczb ca�kowitych.
; Procedura mo�e by� wywo�ywana dla tablicy dwuwymiarowej, kt�ra b�dzie interpretowana jako ci�g liczb.
; Zwraca sum� wszystkich element�w w tablicy.
SumElementsIn2DArray PROC numbers:DWORD

xor RBX, RBX	; Zerujemy rejestr, do kt�rego zapiszemy warto�� zwracan� (sum� element�w w tablicy).
movsxd RAX, NetSize
mul RAX	; Zak�adamy, �e tablica wej�ciowa zawsze b�dzie kwadratem o rozmiarze NetSize x NetSize. P�tla sko�czy dzia�anie, gdy w EAX b�dzie warto�� 0.
lea RCX, QWORD PTR numbers	; Do RAX (64-bitowego rejestru) zapisujemy adres pierwszej zmiennej w tablicy.
xor RDX, RDX	; Zerujemy rejestr potrzebny do poruszania si� w pami�ci.

Suma_Petla1:
	add RBX, [RAX + RDX * 4]	; Adresowanie odbywa si� w nast�puj�cy spos�b: do adresu pierwszej zmiennej, przechowanego w ECX, dodajemy bajty w ilo�ci 4 * EDX
	dec RAX						; (EDX inkrementuje si� co przej�cie p�tli) - w ten spos�b uzyskujemy po�o�enie w pami�ci nast�pnej zmiennej w tablicy
	jz Suma_Koniec

	inc RDX
	jmp Suma_Petla1

Suma_Koniec:
	mov RAX, RBX
	ret

SumElementsIn2DArray ENDP

END