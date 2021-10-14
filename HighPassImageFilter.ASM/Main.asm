.data
NetSize DWORD 3
;Masks DWORD 0, -1, 0, -1, 5, -1, 0, -1, 0

.code

DllEntry PROC hInstDLL:DWORD, reason:DWORD, reserved1:DWORD
	mov eax, 1 ; TRUE
	ret
DllEntry ENDP

; Procedura sumuje elementy w tablicy liczb ca³kowitych.
; Procedura mo¿e byæ wywo³ywana dla tablicy dwuwymiarowej, która bêdzie interpretowana jako ci¹g liczb.
; Zwraca sumê wszystkich elementów w tablicy.
SumElementsIn2DArray PROC numbers:DWORD

xor RBX, RBX	; Zerujemy rejestr, do którego zapiszemy wartoœæ zwracan¹ (sumê elementów w tablicy).
movsxd RAX, NetSize
mul RAX	; Zak³adamy, ¿e tablica wejœciowa zawsze bêdzie kwadratem o rozmiarze NetSize x NetSize. Pêtla skoñczy dzia³anie, gdy w EAX bêdzie wartoœæ 0.
lea RCX, QWORD PTR numbers	; Do RAX (64-bitowego rejestru) zapisujemy adres pierwszej zmiennej w tablicy.
xor RDX, RDX	; Zerujemy rejestr potrzebny do poruszania siê w pamiêci.

Suma_Petla1:
	add RBX, [RAX + RDX * 4]	; Adresowanie odbywa siê w nastêpuj¹cy sposób: do adresu pierwszej zmiennej, przechowanego w ECX, dodajemy bajty w iloœci 4 * EDX
	dec RAX						; (EDX inkrementuje siê co przejœcie pêtli) - w ten sposób uzyskujemy po³o¿enie w pamiêci nastêpnej zmiennej w tablicy
	jz Suma_Koniec

	inc RDX
	jmp Suma_Petla1

Suma_Koniec:
	mov RAX, RBX
	ret

SumElementsIn2DArray ENDP

END