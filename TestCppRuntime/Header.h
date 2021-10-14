#pragma once

unsigned int* ApplyFilterToImage(unsigned int* bitmap, int width, int height);

int** ApplyFilterToImageFragment(int** imageFragment);

int GetNewPixelValue(int** imageFragment);

int SumElementsIn2dArray(int** numbers);

int** GetRgbValues(int startX, int startY, char colorCode);

void InitializeMasks();

void InitializeInputAndOutput(unsigned int* bitmap);

void AddHeaderToOutput(unsigned int* bitmap);