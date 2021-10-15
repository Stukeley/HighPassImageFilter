#include "Algorytm.h"

//typedef int(__stdcall* f_SumElements)(int** numbers);

//int TestArraySum()
//{
//	HINSTANCE hGetProcIDDLL = LoadLibrary(L"C:\\Programowanie\\HighPassImageFilter\\x64\\Debug\\HighPassImageFilter.ASM.dll");
//
//	if (hGetProcIDDLL == NULL)
//	{
//		cout << "Blad przy lokalizowaniu pliku DLL" << endl;
//		return 0;
//	}
//
//	f_SumElements SumElementsProc = (f_SumElements)GetProcAddress(hGetProcIDDLL, "SumElementsIn2DArray");
//
//	if (!SumElementsProc)
//	{
//		cout << "Blad przy lokalizowaniu procedury SumElementsIn2DArray" << endl;
//		return 0;
//	}
//
//	int** testArray = new int* [3];
//
//	for (int i = 0; i < 3; i++)
//	{
//		testArray[i] = new int[3];
//
//		for (int j = 0; j < 3; j++)
//		{
//			testArray[i][j] = 5;
//		}
//	}
//
//	int sum = SumElementsProc(testArray);
//
//	FreeLibrary(hGetProcIDDLL);
//
//	return sum;
//}

unsigned int* LoadBmpFromFileTest()
{
	int width = 152, height = 151;

	unsigned int* bitmap = new unsigned int[54 + width * height];
	FILE* f;

	//fopen_s(&f, "C:\\Programowanie\\HighPassImageFilter\\HighPassImageFilter.CS\\Resources\\Input.bmp", "rb");

	fread(bitmap, sizeof(unsigned int), 54 + width * height, f);

	fclose(f);

	return bitmap;
}

void SaveBmpToFileTest(unsigned int* bitmap)
{
	int width = 152, height = 151;

	FILE* f;

	//fopen_s(&f, "C:\\Programowanie\\HighPassImageFilter\\HighPassImageFilter.CS\\Resources\\OutputCpp.bmp", "wb");

	fwrite(bitmap, sizeof(unsigned int), 54 + width * height, f);

	fclose(f);
}

int main()
{
	/*int sum = TestArraySum();

	cout << "Suma:" << sum << endl;*/

	unsigned int* bitmap = LoadBmpFromFileTest();

	unsigned int* outputBitmap = ApplyFilterToImage(bitmap, 152, 151);

	SaveBmpToFileTest(outputBitmap);

	return 0;
}