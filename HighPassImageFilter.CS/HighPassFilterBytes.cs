// Temat: Filtr górnoprzepustowy "HP1" dla obrazów typu Bitmap.
// Opis: Algorytm nakłada filtr "HP1" dla pikseli obrazu typu Bitmap, podanego przez użytkownika przy pomocy interfejsu graficznego.
// Autor: Rafał Klinowski, Informatyka, rok 3, sem. 5, gr. 5, data: [TODO]
// Wersja: 1.0.

using System.Drawing;
using System.IO;

namespace HighPassImageFilter.CS
{
	// https://stackoverflow.com/questions/44918349/how-to-invoke-c-sharp-function-from-another-project-without-adding-reference

	/// <summary>
	/// Źródło opracowania algorytmu: http://www.algorytm.org/przetwarzanie-obrazow/filtrowanie-obrazow.html
	/// </summary>
	public static class HighPassFilterBytes
	{
		public const int NetSize = 3;

		public static int[,] Masks = new int[NetSize, NetSize]
		{
			{
				0, -1, 0
			},
			{
				-1, 5, -1
			},
			{
				0, -1, 0
			}
		};

		public static byte[] Input { get; set; }

		public static byte[] Output { get; set; }

		public static int Width { get; set; }
		public static int Height { get; set; }

		public static byte[] ApplyFilterToImage(byte[] bitmap, int width, int height)
		{
			InitializeBitmaps(bitmap);

			// Ponieważ wybieramy siatki o rozmiarach 3x3, zaczynamy od drugiego piksela (o indeksie 1) a kończymy na pikselu przedostatnim
			// (o indeksie Width - 2 lub Height - 2).

			for (int i = 1; i < Width - 2; i++)
			{
				for (int j = 1; j < Height - 2; j++)
				{
					int startingX = i;
					int startingY = j;

					var pixelValuesR = GetRgbValues(startingX, startingY, 'R');
					var pixelValuesG = GetRgbValues(startingX, startingY, 'G');
					var pixelValuesB = GetRgbValues(startingX, startingY, 'B');

					pixelValuesR = ApplyFilterToImageFragment(pixelValuesR);
					pixelValuesG = ApplyFilterToImageFragment(pixelValuesG);
					pixelValuesB = ApplyFilterToImageFragment(pixelValuesB);

					for (int x = 0; x < NetSize; x++)
					{
						for (int y = 0; y < NetSize; y++)
						{
							Output.SetPixel(startingX + x, startingY + y, Color.FromArgb(pixelValuesR[x, y], pixelValuesG[x, y], pixelValuesB[x, y]));
						}
					}
				}
			}

			AddHeaderToOutput(bitmap);

			return Output;
		}

		public static int[,] ApplyFilterToImageFragment(int[,] imageFragment)
		{
			var modifiedFragment = new int[NetSize, NetSize];

			// http://www.algorytm.org/przetwarzanie-obrazow/filtrowanie-obrazow.html
			int sumOfMasks = SumElementsIn2DArray(Masks);

			for (int x = 0; x < imageFragment.GetLength(0); x++)
			{
				for (int y = 0; y < imageFragment.GetLength(1); y++)
				{
					int newValue = GetNewPixelValue(imageFragment);

					if (sumOfMasks != 0)
					{
						newValue = newValue / sumOfMasks;
					}

					modifiedFragment[x, y] = newValue;
				}
			}

			return modifiedFragment;
		}

		private static void InitializeBitmaps(byte[] bitmap)
		{
			Input = new byte[Width * Height * 3];
			Output = new byte[Width * Height * 3];

			for (int i = 0; i < Width * Height * 3; i++)
			{
				Input[i] = bitmap[i + 54];
				Output[i] = Input[i];
			}
		}

		private static int GetNewPixelValue(int[,] imageFragment)
		{
			int newPixelWeightedValue = 0;

			for (int x = 0; x < imageFragment.GetLength(0); x++)
			{
				for (int y = 0; y < imageFragment.GetLength(1); y++)
				{
					int factor = imageFragment[x, y] * Masks[x, y];

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

		public static int SumElementsIn2DArray(int[,] numbers)
		{
			int sum = 0;

			for (int x = 0; x < numbers.GetLength(0); x++)
			{
				for (int y = 0; y < numbers.GetLength(1); y++)
				{
					sum += numbers[x, y];
				}
			}

			return sum;
		}

		public static int[,] GetRgbValues(int startX, int startY, char colorCode)
		{
			var values = new int[NetSize, NetSize];

			for (int i = 0; i < NetSize; i++)
			{
				for (int j = 0; j < NetSize; j++)
				{
					var centerPixelIndex = (startX + startY * Width) * 3;

					// Kolejne indeksy:
					// c b c
					// a x a
					// c b c
					// iks) calculated index
					// a) poprzedni i następny
					// b) calculatedIndex +- BmpWidth
					// c) poprzedni i następny dla +- BmpWidth

					//pixelValues[0][0] = calculatedIndex - BmpWidth - 1;
					//pixelValues[0][1] = calculatedIndex - BmpWidth;
					//pixelValues[0][2] = calculatedIndex - BmpWidth + 1;

					//pixelValues[1][0] = calculatedIndex - 1;
					//pixelValues[1][1] = calculatedIndex;
					//pixelValues[1][2] = calculatedIndex + 1;

					//pixelValues[2][0] = calculatedIndex + BmpWidth - 1;
					//pixelValues[2][1] = calculatedIndex + BmpWidth;
					//pixelValues[2][2] = calculatedIndex + BmpWidth + 1;

					//var pixel = Input.GetPixel(startX + i, startY + j);

					var index = centerPixelIndex + (Width * (i - 1)) + ((j - 1));

					switch (colorCode)
					{
						case 'R':
							values[i, j] = Input[index + 2];
							break;
						case 'G':
							values[i, j] = Input[index + 1];
							break;
						case 'B':
							values[i, j] = Input[index];
							break;
					}
				}
			}

			return values;
		}

		public static void SaveImageToFile(byte[] bitmap, string fileName)
		{
			File.WriteAllBytes(fileName, bitmap);
		}

		public static void AddHeaderToOutput(byte[] bitmap)
		{
			var newOutput = new byte[54 + Width * Height * 3];

			for (int i = 0; i < 54; i++)
			{
				newOutput[i] = bitmap[i];
			}

			for (int i = 0; i < Width * Height * 3; i++)
			{
				newOutput[i + 54] = Output[i];
			}

			Output = newOutput;
		}
	}
}