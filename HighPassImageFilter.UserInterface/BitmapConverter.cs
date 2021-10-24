using System.Drawing;
using System.IO;

namespace HighPassImageFilter.UserInterface
{
	public static class BitmapConverter
	{
		public static byte[] ConvertBitmapToByteArray(Bitmap bitmap, string filePath)
		{
			byte[] bitmapArray = new byte[54 + bitmap.Width * bitmap.Height * 3];
			int index = 54;

			using (var fs = File.Open(filePath, FileMode.Open))
			using (var reader = new BinaryReader(fs))
			{
				byte[] header = reader.ReadBytes(54);

				for (int i = 0; i < 54; i++)
				{
					bitmapArray[i] = header[i];
				}
			}

			for (int j = 0; j < bitmap.Height; j++)
			{
				for (int i = 0; i < bitmap.Width; i++)
				{
					var pixel = bitmap.GetPixel(i, j);
					bitmapArray[index++] = pixel.R;
					bitmapArray[index++] = pixel.G;
					bitmapArray[index++] = pixel.B;
				}
			}

			return bitmapArray;
		}

		public static Bitmap ConvertByteArrayToBitmap(byte[] bitmap, Bitmap bitmapImage)
		{
			var newBitmap = new Bitmap(bitmapImage);

			byte[] header = new byte[54];

			int index = 54;

			for (int i = 0; i < 54; i++)
			{
				header[i] = bitmap[i];
			}

			for (int j = 0; j < newBitmap.Height; j++)
			{
				for (int i = 0; i < newBitmap.Width; i++)
				{
					newBitmap.SetPixel(i, j, Color.FromArgb(bitmap[index], bitmap[index + 1], bitmap[index + 2]));
					index += 3;
				}
			}

			return newBitmap;
		}

		public static Bitmap LoadBitmap(string filePath)
		{
			using (var fs = File.Open(filePath, FileMode.Open))
			{
				return new Bitmap(fs);
			}
		}
	}
}
