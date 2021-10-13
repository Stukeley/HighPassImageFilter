// Temat: Filtr górnoprzepustowy "HP1" dla obrazów typu Bitmap.
// Opis: Algorytm nakłada filtr "HP1" dla pikseli obrazu typu Bitmap, podanego przez użytkownika przy pomocy interfejsu graficznego.
// Autor: Rafał Klinowski, Informatyka, rok 3, sem. 5, gr. 5, data: [TODO]
// Wersja: 1.0.

using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HighPassImageFilter.UserInterface
{
	// todo: pomiar czasu
	// todo: dynamiczne przeładowanie
	// todo: exception handling
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Bitmapa wejściowa, na której nakładany jest filtr.
		/// </summary>
		private static Bitmap _bitmap;
		
		/// <summary>
		/// Zmienna bool informująca o tym, czy algorytm jest wywoływany w języku C# czy ASM.
		/// </summary>
		private static bool _asmAlgorithm;

		/// <summary>
		/// Konstruktor okna uruchamiający się automatycznie na początku działania programu.
		/// Przygotowuje interfejs i ustawia wartości początkowe zmiennych.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			CsAlgorithmBox.IsChecked = true;
			_asmAlgorithm = false;
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie naciśnięcia przez użytkownika przycisku Browse.
		/// Funkcja ta otwiera okno wyboru pliku na dysku użytkownika. Użytkownik wybiera plik, na który nakładany będzie filtr.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający przycisk, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void FileBrowserButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*",
				InitialDirectory = Directory.GetCurrentDirectory()
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var fileName = openFileDialog.FileName;

				if (Path.GetExtension(fileName) != ".bmp")
				{
					MessageBox.Show("A file with an incorrect extension has been selected. This program only accepts bitmap (.bmp) files.", "Wrong file extension", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				
				ContentPanel.Children.Clear();
				SaveBitmapButton.IsEnabled = false;

				FilePathBox.Text = fileName;
				_bitmap = new Bitmap(fileName);
				ContentPanel.Children.Add(new System.Windows.Controls.Image()
				{
					Source = ConvertBitmapToImageSource(_bitmap),
					Width = _bitmap.Width,
					Height = _bitmap.Height
				});
				FilterBitmapButton.IsEnabled = true;
				SaveBitmapButton.IsEnabled = false;
			}

		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie naciśnięcia przez użytkownika przycisku Filter.
		/// Funkcja ta powoduje wywołanie algorytmu filtru górnoprzepustowego dla podanego przez użytkownika obrazu.
		/// W zależności od tego, który algorytm został wybrany (C# czy ASM), zostanie załadowana odpowiednia biblioteka.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający przycisk, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void FilterBitmapButton_Click(object sender, RoutedEventArgs e)
		{
			if (_asmAlgorithm)
			{
				CallAsmAlgorithm();
			}
			else
			{
				CallCsAlgorithm();
			}
		}

		/// <summary>
		/// Funkcja wywołuje algorytm filtru obrazu w bibliotece napisanej w C#.
		/// Jako parametr przekazywana jest zmienna _bitmap zawierająca podany przez użytkownika obraz.
		/// </summary>
		public async void CallCsAlgorithm()
		{
			var newBitmap = await Task.Run(() => CS.HighPassFilter.ApplyFilterToImageAsync(_bitmap));

			ContentPanel.Children.Add(new System.Windows.Controls.Image()
			{
				Source = ConvertBitmapToImageSource(newBitmap),
				Width = newBitmap.Width,
				Height = newBitmap.Height,
				Margin = new Thickness(20, 0, 0, 0)
			});

			_bitmap = newBitmap;
			SaveBitmapButton.IsEnabled = true;
		}

		/// <summary>
		/// Funkcja wywołuje algorytm filtru obrazu w bibliotece napisanej w ASM.
		/// Jako parametr przekazywana jest zmienna _bitmap zawierająca podany przez użytkownika obraz.
		/// </summary>
		public async void CallAsmAlgorithm()
		{
			
		}

		/// <summary>
		/// Funkcja konwertuje bitmapę, zwróconą przez algorytm filtru, na obiekt ImageSource potrzebny do wyświetlenia obrazu na interfejsie.
		/// </summary>
		/// <param name="bitmap">Bitmapa, która ma zostać przekonwertowana i wyświetlona na ekran.</param>
		/// <returns>Obiekt ImageSource, który następnie zostanie wyświetlony na interfejsie.</returns>
		private static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
		{
			if (bitmap is null)
			{
				return null;
			}

			using (var memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
				memoryStream.Position = 0;
				var bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memoryStream;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();

				return bitmapimage;
			}
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie naciśnięcia przez użytkownika przycisku Save.
		/// Funkcja ta otwiera okno wyboru ścieżki do pliku na dysku użytkownika. Bitmapa, na którą został nałożony filtr, zostanie następnie zapisana na dysk
		/// w danym miejscu.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający przycisk, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void SaveBitmapButton_Click(object sender, RoutedEventArgs e)
		{
			var saveFileDialog = new SaveFileDialog()
			{
				Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*",
				InitialDirectory = Directory.GetCurrentDirectory(),
				FileName = "Output.bmp"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				if (Path.GetExtension(saveFileDialog.FileName) != ".bmp")
				{
					saveFileDialog.FileName += ".bmp";
				}
				
				CS.HighPassFilter.SaveImageToFile(_bitmap, saveFileDialog.FileName);
			}
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie odznaczenia checkboxa "ASM".
		/// Funkcja ta powoduje zanaczenie checkboxa "C#" tak, by dokładnie jeden z nich był aktywny w danym momencie.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający checkbox, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void AsmAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			CsAlgorithmBox.IsChecked = true;
			_asmAlgorithm = false;
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie odznaczenia checkboxa "C#".
		/// Funkcja ta powoduje zaznaczenie checkboxa "ASM" tak, by dokładnie jeden z nich był aktywny w danym momencie.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający checkbox, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void CsAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = true;
			_asmAlgorithm = true;
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie zaznaczenia checkboxa "ASM".
		/// Funkcja ta powoduje odznaczenie checkboxa "C#" tak, by dokładnie jeden z nich był aktywny w danym momencie.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający checkbox, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void AsmAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			CsAlgorithmBox.IsChecked = false;
			_asmAlgorithm = false;
		}

		/// <summary>
		/// Zdarzenie wywołujące się w momencie zaznaczenia checkboxa "C#".
		/// Funkcja ta powoduje odznaczenie checkboxa "ASM" tak, by dokładnie jeden z nich był aktywny w danym momencie.
		/// </summary>
		/// <param name="sender">Automatycznie wygenerowany parametr oznaczający checkbox, który wywołał zdarzenie.</param>
		/// <param name="e">Automatycznie wygenerowany parametr zawierający argumenty zdarzenia.</param>
		private void CsAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = false;
			_asmAlgorithm = true;
		}
	}
}