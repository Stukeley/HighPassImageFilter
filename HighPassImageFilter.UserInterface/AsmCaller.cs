using System.Drawing;

namespace HighPassImageFilter.UserInterface
{
	public static class AsmCaller
	{
		public static Bitmap ApplyFilterToImage(Bitmap bitmap)
		{
			// Konwertujemy obiekt Bitmap na tablicę byte[] (unsigned int), następnie przekazujemy ją do funkcji.

			var converter = new ImageConverter();
			var bytes = converter.ConvertTo(bitmap, typeof(byte[])) as byte[];
			
			// Po wywołaniu funkcji zamieniamy tablicę byte[] z powrotem na obraz Bitmap.

			return null;
		}
	}
}
