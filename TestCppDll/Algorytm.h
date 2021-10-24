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

	for (int j = 0; j < NetSize; j++)
	{
		for (int i = 0; i < NetSize; i++)
		{
			int middlePixelIndex = (startX + startY * BmpWidth) * 3;

			int index = middlePixelIndex + (BmpWidth * (j - 1) + (i - 1)) * 3;

			switch (colorCode)
			{
			case 'R':
				pixelValues[i][j] = Input[index];
				break;

			case 'G':
				pixelValues[i][j] = Input[index + 1];
				break;

			case 'B':
				pixelValues[i][j] = Input[index + 2];
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
	Input = new unsigned int[BmpWidth * BmpHeight * 3];
	Output = new unsigned int[BmpWidth * BmpHeight * 3];

	for (int i = 0; i < BmpWidth * BmpHeight * 3; i++)
	{
		Input[i] = bitmap[i + 54];
		Output[i] = Input[i];
	}
}

void AddHeaderToOutput(unsigned int* bitmap)
{
	unsigned int* newOutput = new unsigned int[54 + BmpWidth * BmpHeight * 3];

	for (int i = 0; i < 54; i++)
	{
		newOutput[i] = bitmap[i];
	}

	for (int i = 0; i < BmpWidth * BmpHeight * 3; i++)
	{
		newOutput[i + 54] = Output[i];
	}

	delete Output;
	Output = newOutput;
}