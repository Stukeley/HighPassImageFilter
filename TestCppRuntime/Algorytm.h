#pragma once
#include <iostream>
#include <fstream>
#include <Windows.h>
#include "Header.h"
using namespace std;

const int NetSize = 3;

int** Masks;

unsigned int* Input;
unsigned int* Output;

unsigned int BmpWidth;
unsigned int BmpHeight;

unsigned int* ApplyFilterToImage(unsigned int* bitmap, int width, int height)
{
	BmpWidth = width;
	BmpHeight = height;

	InitializeMasks();

	InitializeInputAndOutput(bitmap);

	for (int i = 1; i < width - 2; i++)
	{
		for (int j = 1; j < height - 2; j++)
		{
			int startingX = i;
			int startingY = j;

			int** pixelValuesR = GetRgbValues(startingX, startingY, 'R');
			int** pixelValuesG = GetRgbValues(startingX, startingY, 'G');
			int** pixelValuesB = GetRgbValues(startingX, startingY, 'B');

			pixelValuesR = ApplyFilterToImageFragment(pixelValuesR);
			pixelValuesG = ApplyFilterToImageFragment(pixelValuesG);
			pixelValuesB = ApplyFilterToImageFragment(pixelValuesB);

			for (int x = 0; x < NetSize; x++)
			{
				for (int y = 0; y < NetSize; y++)
				{
					Output[startingX + x + (startingY + y) * BmpWidth] = pixelValuesR[x][y] + pixelValuesG[x][y] + pixelValuesB[x][y];
				}
			}
		}
	}

	AddHeaderToOutput(bitmap);
	
	return Output;
}

int** ApplyFilterToImageFragment(int** imageFragment)
{
	int** modifiedFragment = new int* [NetSize];

	for (int i = 0; i < NetSize; i++)
	{
		modifiedFragment[i] = new int[NetSize];
	}

	int sumOfMasks = SumElementsIn2dArray(Masks);

	for (int x = 0; x < NetSize; x++)
	{
		for (int y = 0; y < NetSize; y++)
		{
			int newValue = GetNewPixelValue(imageFragment);

			if (sumOfMasks != 0)
			{
				newValue /= sumOfMasks;
			}

			modifiedFragment[x][y] = newValue;
		}
	}

	return modifiedFragment;
}

int GetNewPixelValue(int** imageFragment)
{
	int newPixelWeightedValue = 0;

	for (int x = 0; x < NetSize; x++)
	{
		for (int y = 0; y < NetSize; y++)
		{
			int factor = imageFragment[x][y] * Masks[x][y];

			newPixelWeightedValue += factor;
		}
	}

	if (newPixelWeightedValue < 0)
	{
		newPixelWeightedValue = 0;
	}
	else if (newPixelWeightedValue > 255)
	{
		newPixelWeightedValue = 255;
	}

	return newPixelWeightedValue;
}

int SumElementsIn2dArray(int** numbers)
{
	int sum = 0;

	for (int i = 0; i < NetSize; i++)
	{
		for (int j = 0; j < NetSize; j++)
		{
			sum += numbers[i][j];
		}
	}

	return sum;
}

int** GetRgbValues(int startX, int startY, char colorCode)
{
	int** pixelValues = new int* [NetSize];

	for (int i = 0; i < NetSize; i++)
	{
		pixelValues[i] = new int[NetSize];
	}

	// Indeks œrodkowego piksela.
	int calculatedIndex = startX + (startY)*BmpWidth;

	// Kolejne indeksy:
	// c b c
	// a x a
	// c b c
	// iks) calculated index
	// a) poprzedni i nastêpny
	// b) calculatedIndex +- BmpWidth
	// c) poprzedni i nastêpny dla +- BmpWidth

	//pixelValues[0][0] = calculatedIndex - BmpWidth - 1;
	//pixelValues[0][1] = calculatedIndex - BmpWidth;
	//pixelValues[0][2] = calculatedIndex - BmpWidth + 1;

	//pixelValues[1][0] = calculatedIndex - 1;
	//pixelValues[1][1] = calculatedIndex;
	//pixelValues[1][2] = calculatedIndex + 1;

	//pixelValues[2][0] = calculatedIndex + BmpWidth - 1;
	//pixelValues[2][1] = calculatedIndex + BmpWidth;
	//pixelValues[2][2] = calculatedIndex + BmpWidth + 1;

	for (int i = 0; i < NetSize; i++)
	{
		for (int j = 0; j < NetSize; j++)
		{
			pixelValues[i][j] = Input[calculatedIndex + ((-1 + i) * BmpWidth) + (j - 1)];

			// Kolejnoœæ pixeli: 0 B G R

			switch (colorCode)
			{
				case 'R':
					pixelValues[i][j] = (pixelValues[i][j] >> 0) & 0xFF;
					break;

				case 'G':
					pixelValues[i][j] = (pixelValues[i][j] >> 8) & 0xFF;
					break;

				case 'B':
					pixelValues[i][j] = (pixelValues[i][j] >> 16) & 0xFF;
					break;
			}
		}
	}

	return pixelValues;
}

void InitializeMasks()
{
	Masks = new int* [NetSize];

	for (int i = 0; i < NetSize; i++)
	{
		Masks[i] = new int[NetSize];

		for (int j = 0; j < NetSize; j++)
		{
			Masks[i][j] = 0;
		}
	}

	Masks[1][1] = 5;
	Masks[0][1] = -1;
	Masks[1][0] = -1;
	Masks[1][2] = -1;
	Masks[2][1] = -1;
}

void InitializeInputAndOutput(unsigned int* bitmap)
{
	// Input becomes bitmap without the header. Header is separated and later added back.
	// Output initially becomes Input.
	Input = new unsigned int[BmpWidth * BmpHeight];
	Output = new unsigned int[BmpWidth * BmpHeight];

	for (int i = 0; i < BmpWidth * BmpHeight; i++)
	{
		Input[i] = bitmap[i + 54];
		Output[i] = Input[i];
	}
}

void AddHeaderToOutput(unsigned int* bitmap)
{
	unsigned int* newOutput = new unsigned int[54 + BmpWidth * BmpHeight];

	for (int i = 0; i < 54; i++)
	{
		newOutput[i] = bitmap[i];
	}

	for (int i=0;i< BmpWidth * BmpHeight;i++)
	{
		newOutput[i + 54] = Output[i];
	}

	delete Output;
	
	Output = newOutput;
}