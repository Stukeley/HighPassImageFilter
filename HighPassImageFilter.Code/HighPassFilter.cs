using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace HighPassImageFilter.Code
{
	// Filtrację przeprowadza się osobno dla każdej składowej obrazu. Zatem jeżeli mamy obraz reprezentowany w modelu RGB,
	// to wówczas będziemy wykonywać oddzielne przekształcenia dla składowej R, G oraz B.
	// Jak łatwo zauważyć próba zastosowania filtracji dla punktów położonych na krawędzi obrazu, prowadzi do sytuacji, w której maska "wystaje" poza przetwarzany obraz. Istnieje kilka sposobów obejścia tego problemu. Jednym z nich jest pominięcie procesu filtracji dla takich punktów, innym jest zmniejszenie obrazu po filtracji o punkty, dla których proces ten nie mógł być wykonany. Kolejnym sposobem jest dodanie do filtrowanego obrazu zduplikowanych pikseli znajdujących się na jego brzegu.

	// https://stackoverflow.com/questions/44918349/how-to-invoke-c-sharp-function-from-another-project-without-adding-reference

	public static class HighPassFilter
	{
		// The "Mean Removal" filter uses 3x3 masks.
		public const int NetSize = 3;

		// Mask weights for the "Mean Removal" filter.
		public static int[,] Masks = new int[NetSize,NetSize]
		{
			{
				-1, -1, -1
			},
			{
				-1, 9, -1
			},
			{
				-1, -1, -1
			}
		};
		
		// Input image.
		public static Bitmap Input { get; set; }
		
		// Output image.
		public static Bitmap Output { get; set; }

		public static Bitmap ApplyFilterToImage(Bitmap bitmap)
		{
			InitializeBitmaps(bitmap);
			
			// Cut the edges.
			int amountOfHorizontalNets = Input.Width - 2;
			int amountOfVerticalNets = Input.Height - 2;

			for (int i = 0; i < amountOfHorizontalNets; i++)
			{
				for (int j = 0; j < amountOfVerticalNets; j++)
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
							Output.SetPixel(startingX + x, startingY + y, Color.FromArgb(pixelValuesR[x,y], pixelValuesG[x,y], pixelValuesB[x,y]));
						}
					}
				}
			}
			
			return Output;
		}

		public static async Task<Bitmap> ApplyFilterToImageAsync(Bitmap bitmap)
		{
			InitializeBitmaps(bitmap);
			
			// Cut the edges.
			int amountOfHorizontalNets = Input.Width - 2;
			int amountOfVerticalNets = Input.Height - 2;

			for (int i = 0; i < amountOfHorizontalNets; i++)
			{
				for (int j = 0; j < amountOfVerticalNets; j++)
				{
					int startingX = i;
					int startingY = j;

					var pixelValuesR = GetRgbValues(startingX, startingY, 'R');
					var pixelValuesG = GetRgbValues(startingX, startingY, 'G');
					var pixelValuesB = GetRgbValues(startingX, startingY, 'B');

					var pixelValuesRTask = ApplyFilterToImageFragmentAsync(pixelValuesR);
					var pixelValuesGTask = ApplyFilterToImageFragmentAsync(pixelValuesG);
					var pixelValuesBTask = ApplyFilterToImageFragmentAsync(pixelValuesB);

					await Task.WhenAll(pixelValuesRTask, pixelValuesGTask, pixelValuesBTask);

					pixelValuesR = await pixelValuesRTask;
					pixelValuesG = await pixelValuesGTask;
					pixelValuesB = await pixelValuesBTask;
					
					for (int x = 0; x < NetSize; x++)
					{
						for (int y = 0; y < NetSize; y++)
						{
							Output.SetPixel(startingX + x, startingY + y, Color.FromArgb(pixelValuesR[x, y], pixelValuesG[x, y], pixelValuesB[x, y]));
						}
					}
				}
			}

			return Output;
		}
		
		private static void InitializeBitmaps(Bitmap bitmap)
		{
			Input = bitmap;
			Output = new Bitmap(Input);
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
		
		public static async Task<int[,]> ApplyFilterToImageFragmentAsync(int[,] imageFragment)
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
					var pixel = Input.GetPixel(startX + i, startY + j);

					switch (colorCode)
					{
						case 'R':
							values[i, j] = pixel.R;
							break;
						case 'G':
							values[i, j] = pixel.G;
							break;
						case 'B':
							values[i, j] = pixel.B;
							break;
					}
				}
			}

			return values;
		}

		public static void SaveImageToFile(Bitmap bitmap, string fileName)
		{
			bitmap.Save(fileName);
		}
	}
}