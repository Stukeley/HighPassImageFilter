using System.Drawing;
using System.Linq;

namespace HighPassImageFilter.Code
{
	// Filtrację przeprowadza się osobno dla każdej składowej obrazu. Zatem jeżeli mamy obraz reprezentowany w modelu RGB,
	// to wówczas będziemy wykonywać oddzielne przekształcenia dla składowej R, G oraz B.
	// Jak łatwo zauważyć próba zastosowania filtracji dla punktów położonych na krawędzi obrazu, prowadzi do sytuacji, w której maska "wystaje" poza przetwarzany obraz. Istnieje kilka sposobów obejścia tego problemu. Jednym z nich jest pominięcie procesu filtracji dla takich punktów, innym jest zmniejszenie obrazu po filtracji o punkty, dla których proces ten nie mógł być wykonany. Kolejnym sposobem jest dodanie do filtrowanego obrazu zduplikowanych pikseli znajdujących się na jego brzegu.


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
				-1,9,-1
			},
			{
				-1,-1,-1
			}
		};
		
		// Input image.
		public static Bitmap Input { get; set; }
		
		// Output image.
		public static Bitmap Output { get; set; }
		
		static HighPassFilter()
		{
			Input = new Bitmap(@"C:\Programowanie\HighPassImageFilter\HighPassImageFilter.Code\Resources\Input.bmp");
			Output = new Bitmap(Input);
		}

		public static void ApplyFilterToImage()
		{
			// Cut the edges.
			int amountOfHorizontalNets = Input.Width - 2;
			int amountOfVerticalNets = Input.Height - 2;

			for (int i = 0; i < amountOfHorizontalNets; i++)
			{
				for (int j = 0; j < amountOfVerticalNets; j++)
				{
					int startingX = i;
					int startingY = j;

					var pixelValuesR = new int[NetSize, NetSize]
					{
						{
							Input.GetPixel(startingX, startingY).R, Input.GetPixel(startingX + 1, startingY).R, Input.GetPixel(startingX + 2, startingY).R
						},
						{
							Input.GetPixel(startingX, startingY + 1).R, Input.GetPixel(startingX + 1, startingY + 1).R, Input.GetPixel(startingX + 2, startingY + 2).R
						},
						{
							Input.GetPixel(startingX, startingY + 2).R, Input.GetPixel(startingX + 1, startingY + 1).R, Input.GetPixel(startingX + 2, startingY + 2).R
						}
					};
					
					var pixelValuesG = new int[NetSize, NetSize]
					{
						{
							Input.GetPixel(startingX, startingY).G, Input.GetPixel(startingX + 1, startingY).G, Input.GetPixel(startingX + 2, startingY).G
						},
						{
							Input.GetPixel(startingX, startingY + 1).G, Input.GetPixel(startingX + 1, startingY + 1).G, Input.GetPixel(startingX + 2, startingY + 2).G
						},
						{
							Input.GetPixel(startingX, startingY + 2).G, Input.GetPixel(startingX + 1, startingY + 1).G, Input.GetPixel(startingX + 2, startingY + 2).G
						}
					};
					
					var pixelValuesB = new int[NetSize, NetSize]
					{
						{
							Input.GetPixel(startingX, startingY).B, Input.GetPixel(startingX + 1, startingY).B, Input.GetPixel(startingX + 2, startingY).B
						},
						{
							Input.GetPixel(startingX, startingY + 1).B, Input.GetPixel(startingX + 1, startingY + 1).B, Input.GetPixel(startingX + 2, startingY + 2).B
						},
						{
							Input.GetPixel(startingX, startingY + 2).B, Input.GetPixel(startingX + 1, startingY + 1).B, Input.GetPixel(startingX + 2, startingY + 2).B
						}
					};

					ApplyFilterToImageFragment(ref pixelValuesR);
					ApplyFilterToImageFragment(ref pixelValuesG);
					ApplyFilterToImageFragment(ref pixelValuesB);

					for (int x = 0; x < NetSize; x++)
					{
						for (int y = 0; y < NetSize; y++)
						{
							Output.SetPixel(startingX + x, startingY + y, Color.FromArgb(pixelValuesR[x,y], pixelValuesG[x,y], pixelValuesB[x,y]));
						}
					}
				}
			}

			SaveImageToFile();
		}

		public static void ApplyFilterToImageFragment(ref int[,] imageFragment)
		{
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
					
					imageFragment[x, y] = newValue;
				}
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

		public static void SaveImageToFile()
		{
			Output.Save(@"C:\Programowanie\HighPassImageFilter\HighPassImageFilter.Code\Resources\Output.bmp");
		}
	}
}