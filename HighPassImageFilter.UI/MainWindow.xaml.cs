using System;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HighPassImageFilter.UI
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

		public async void CallCsAlgorithm()
		{
			var newBitmap = await Task.Run(() => Code.HighPassFilter.ApplyFilterToImageAsync(_bitmap));

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

		public async void CallAsmAlgorithm()
		{
			
		}

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

		private void SaveBitmapButton_OnClick(object sender, RoutedEventArgs e)
		{
			var saveFileDialog = new SaveFileDialog()
			{
				Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*",
				InitialDirectory = Directory.GetCurrentDirectory()
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				if (Path.GetExtension(saveFileDialog.FileName) == String.Empty)
				{
					saveFileDialog.FileName += ".bmp";
				}
				
				Code.HighPassFilter.SaveImageToFile(_bitmap, saveFileDialog.FileName);
			}
		}

		private void AsmAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			CsAlgorithmBox.IsChecked = true;
			_asmAlgorithm = false;
		}

		private void CsAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = true;
			_asmAlgorithm = true;
		}

		private void AsmAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			CsAlgorithmBox.IsChecked = false;
			_asmAlgorithm = false;
		}

		private void CsAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = false;
			_asmAlgorithm = true;
		}
	}
}