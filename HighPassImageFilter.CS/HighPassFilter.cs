// Temat: Filtr górnoprzepustowy "HP1" dla obrazów typu Bitmap.
// Opis: Algorytm nakłada filtr "HP1" dla pikseli obrazu typu Bitmap, podanego przez użytkownika przy pomocy interfejsu graficznego.
// Autor: Rafał Klinowski, Informatyka, rok 3, sem. 5, gr. 5, data: [TODO]
// Wersja: 1.0.

using System.Drawing;
using System.Threading.Tasks;

namespace HighPassImageFilter.CS
{
	// https://stackoverflow.com/questions/44918349/how-to-invoke-c-sharp-function-from-another-project-without-adding-reference

	/// <summary>
	/// Źródło opracowania algorytmu: http://www.algorytm.org/przetwarzanie-obrazow/filtrowanie-obrazow.html
	/// </summary>
	public static class HighPassFilter
	{
		/// <summary>
		/// Rozmiar tablicy masek - wybrany filtr ("HP1") korzysta z maski 3x3.
		/// </summary>
		public const int NetSize = 3;

		/// <summary>
		/// Tablica o rozmiarze NetSize x Netsize, zawierająca maski - możemy je interpretować jako wagi poszczególnych pikseli we fragmencie obrazu.
		/// </summary>
		public static int[,] Masks = new int[NetSize,NetSize]
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
		
		/// <summary>
		/// Obraz wejściowy typu Bitmap.
		/// </summary>
		public static Bitmap Input { get; set; }
		
		/// <summary>
		/// Obraz wyjściowy, to znaczy po nałożeniu filtru na obraz wejściowy, typu Bitmap.
		/// </summary>
		public static Bitmap Output { get; set; }

		/// <summary>
		/// Funkcja, która wykonuje algorytm nałożenia filtru na obraz w sposób sekwencyjny, tj. bez wykorzystania wątków.
		/// </summary>
		/// <param name="bitmap">Obraz wejściowy typu Bitmap, który zostaje zapisany do zmiennej Input.</param>
		/// <returns>Obraz wyjściowy z nałożonym filtrem, będący zmienną Output.</returns>
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

		/// <summary>
		/// Funkcja, która wykonuje algorytm nałożenia filtru na obraz w sposób asynchroniczny, tj. z wykorzystaniem wątków.
		/// Ponieważ obliczenia należy wykonać dla wartości R, G i B każdego piksela, wówczas dzielimy na wątki wykonywanie obliczeń
		/// dla każdej z tych wartości. Oznacza to, że nowe wartości R dla pikseli zostaną policzone równolegle do nowych wartości B i G.
		/// </summary>
		/// <param name="bitmap">Obraz wejściowy typu Bitmap, który zostaje zapisany do zmiennej Input.</param>
		/// <returns>Obraz wyjściowy z nałożonym filtrem, będący zmienną Output.</returns>
		public static async Task<Bitmap> ApplyFilterToImageAsync(Bitmap bitmap)
		{
			InitializeBitmaps(bitmap);
			
			// Ponieważ wybieramy siatki o rozmiarach 3x3, zaczynamy od drugiego piksela (o indeksie 1) a kończymy na pikselu przedostatnim
			// (o indeksie Width - 2 lub Height - 2).

			for (int i = 1; i < Input.Width - 2; i++)
			{
				for (int j = 1; j < Input.Height - 2; j++)
				{
					int startingX = i;
					int startingY = j;

					var pixelValuesR = GetRgbValues(startingX, startingY, 'R');
					var pixelValuesG = GetRgbValues(startingX, startingY, 'G');
					var pixelValuesB = GetRgbValues(startingX, startingY, 'B');

					var pixelValuesRTask = Task.Run(() => ApplyFilterToImageFragment(pixelValuesR));
					var pixelValuesGTask = Task.Run(() => ApplyFilterToImageFragment(pixelValuesG));
					var pixelValuesBTask = Task.Run(() => ApplyFilterToImageFragment(pixelValuesB));

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
		
		/// <summary>
		/// Funkcja, która wykonuje algorytm nałożenia filtru na fragment obrazu. Funkcja ta może być wywołana zarówno przez
		/// synchroniczne, jak i asynchroniczne uruchomienie algorytmu.
		/// Dla podanego jako parametr fragmentu obrazu, o rozmiarze NetSize x NetSize, obliczane są nowe wartości pikseli na podstawie tablicy masek.
		/// Nowa wartość jest dzielona przez sumę masek, by zapobiec zmianie jasności obrazu.
		/// </summary>
		/// <param name="imageFragment">Dwuwymiarowa tablica zawierająca wartości pikseli</param>
		/// <returns></returns>
		/// <remarks>Funkcja wywoływana jest osobno dla wartości R, G i B poszczególnych pikseli.</remarks>
		/// <remarks>Dla wybranego filtra ("HP1"), suma masek jest równa 1, dzielenie więc nie zmienia wartości piksela.</remarks>
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

		/// <summary>
		/// Funkcja ta inicjalizuje zmienne w klasie - Input i Output.
		/// Domyślnie, Input jest ustawiany na obiekt przekazany z projektu UI (wprowadzony przez użytkownika), a Output - na obiekt równy Input.
		/// </summary>
		/// <param name="bitmap">Przekazany, domyślny obraz (wprowadzony przez użytkownika w projekcie UI).</param>
		private static void InitializeBitmaps(Bitmap bitmap)
		{
			Input = bitmap;
			Output = new Bitmap(Input);
		}

		/// <summary>
		///  
		/// </summary>
		/// <param name="imageFragment"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Funkcja pomocnicza zwracająca sumę elementów w tablicy dwuwymiarowej.
		/// Wykorzystywana m.in. przy dzieleniu nowych wartości pikseli przez sumę masek w tablicy Masks.
		/// </summary>
		/// <param name="numbers">Tablica, której elementy chcemy zsumować.</param>
		/// <returns>Suma elementów w tablicy.</returns>
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

		/// <summary>
		/// Funkcja wyznacza i zwraca tablicę wartości R, G lub B dla pikseli, gdzie piksel o pozycji podanej jako parametr funkcji jest środkowym.
		/// </summary>
		/// <param name="startX">Pozycja X (horyzontalna) piksela środkowego, dla którego obliczamy nowe wartości.</param>
		/// <param name="startY">Pozycja Y (wertykalna) piksela środkowego, dla którego obliczamy nowe wartości.</param>
		/// <param name="colorCode">Znak oznaczający, czy chcemy uzyskać wartości R, G czy B dla danych pikseli.</param>
		/// <returns>Tablica dwuwymiarowa zawierająca tylko dane wartości (R, G lub B) dla tych pikseli.</returns>
		/// <remarks>Funkcja nie jest wywoływana dla pikseli brzegowych. Piksel o podanej jako parametr pozycji będzie pikselem środkowym,
		/// to znaczy znajdzie się na indeksie [1,1] zwracanej tablicy.</remarks>
		/// <example>GetRgbValues(5, 5, 'R') zwróci tablicę dwuwymiarową wartości R dla pikseli, gdzie piksel o pozycji (5, 5) jest środkowym.</example>
		public static int[,] GetRgbValues(int startX, int startY, char colorCode)
		{
			var values = new int[NetSize, NetSize];

			for (int i = -1; i < NetSize - 1; i++)
			{
				for (int j = -1; j < NetSize - 1; j++)
				{
					var pixel = Input.GetPixel(startX + i, startY + j);

					switch (colorCode)
					{
						case 'R':
							values[i+1, j+1] = pixel.R;
							break;
						case 'G':
							values[i+1, j+1] = pixel.G;
							break;
						case 'B':
							values[i+1, j+1] = pixel.B;
							break;
					}
				}
			}

			return values;
		}

		/// <summary>
		/// Funkcja zapisuje bitmapę, przekazaną jako parametr, do ścieżki na dysku twardym przekazanej jako parametr.
		/// </summary>
		/// <param name="bitmap">Obraz typu Bitmap do zapisania.</param>
		/// <param name="fileName">Ścieżka do pliku, do której zapisany zostanie obraz po nałożeniu filtru.</param>
		public static void SaveImageToFile(Bitmap bitmap, string fileName)
		{
			bitmap.Save(fileName);
		}
	}
}