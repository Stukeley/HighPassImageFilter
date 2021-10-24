// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "Algorytm.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" __declspec(dllexport) unsigned int* ApplyFilterToImage(unsigned int* bitmap, int width, int height)
{
	BmpWidth = width;
	BmpHeight = height;

	InitializeMasks();

	InitializeInputAndOutput(bitmap);

	for (int j = 1; j < height - 2; j++)
	{
		for (int i = 1; i < width - 2; i++)
		{
			int startingX = i;
			int startingY = j;

			int** pixelValuesR = GetRgbValues(startingX, startingY, 'R');
			int** pixelValuesG = GetRgbValues(startingX, startingY, 'G');
			int** pixelValuesB = GetRgbValues(startingX, startingY, 'B');

			pixelValuesR = ApplyFilterToImageFragment(pixelValuesR);
			pixelValuesG = ApplyFilterToImageFragment(pixelValuesG);
			pixelValuesB = ApplyFilterToImageFragment(pixelValuesB);

			for (int y = 0; y < NetSize; y++)
			{
				for (int x = 0; x < NetSize; x++)
				{
					int middlePixelIndex = (i + j * BmpWidth) * 3;

					int index = middlePixelIndex + (BmpWidth * (y - 1) + (x - 1)) * 3;


					Output[index] = pixelValuesR[x][y];
					Output[index + 1] = pixelValuesG[x][y];
					Output[index + 2] = pixelValuesB[x][y];
				}
			}
		}
	}

	AddHeaderToOutput(bitmap);
	return Output;
}