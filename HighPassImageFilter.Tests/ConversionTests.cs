using NUnit.Framework;

namespace HighPassImageFilter.Tests
{
	public class ConversionTests
	{
		public const string Path = @"C:\Programowanie\HighPassImageFilter\HighPassImageFilter.CS\Resources\Input.bmp";

		[Test]
		public void BitmapConversionTest()
		{
			var bitmapImage = BitmapConverter.LoadBitmap();

			var bitmapArray = BitmapConverter.ConvertBitmapToByteArray(bitmapImage);

			(byte B, byte G, byte R) firstPixelBGR = (bitmapArray[56], bitmapArray[55], bitmapArray[54]);

			Assert.AreEqual(bitmapImage.GetPixel(0, 0).B, firstPixelBGR.B);
			Assert.AreEqual(bitmapImage.GetPixel(0, 0).G, firstPixelBGR.G);
			Assert.AreEqual(bitmapImage.GetPixel(0, 0).R, firstPixelBGR.R);

			var newBitmapImage = BitmapConverter.ConvertByteArrayToBitmap(bitmapArray, bitmapImage);

			Assert.AreEqual(bitmapImage.GetPixel(0, 0).B, newBitmapImage.GetPixel(0, 0).B);
			Assert.AreEqual(bitmapImage.GetPixel(0, 0).G, newBitmapImage.GetPixel(0, 0).G);
			Assert.AreEqual(bitmapImage.GetPixel(0, 0).R, newBitmapImage.GetPixel(0, 0).R);

			int x = 1, y = 1;

			Assert.AreEqual(bitmapImage.GetPixel(x, y).R, bitmapArray[54 + (x + y * bitmapImage.Width) * 3]);
			Assert.AreEqual(bitmapImage.GetPixel(x, y).G, bitmapArray[54 + (x + y * bitmapImage.Width) * 3 + 1]);
			Assert.AreEqual(bitmapImage.GetPixel(x, y).B, bitmapArray[54 + (x + y * bitmapImage.Width) * 3 + 2]);

			var pixelsArrayManual = new int[3, 3];
			var pixelsArrayAutomatic = new int[3, 3];

			var bytesArrayWithoutHeader = new byte[bitmapImage.Width * bitmapImage.Height * 3];

			for (int i = 0; i < bitmapImage.Width * bitmapImage.Height * 3; i++)
			{
				bytesArrayWithoutHeader[i] = bitmapArray[i + 54];
			}

			for (int j = 0; j < 3; j++)
			{
				for (int i = 0; i < 3; i++)
				{
					// Sprawdzamy R czyli +0.

					var centerPixelIndex = (x + y * bitmapImage.Width) * 3;

					var index = centerPixelIndex + (bitmapImage.Width * (j - 1) + (i - 1)) * 3;

					pixelsArrayManual[i, j] = bytesArrayWithoutHeader[index];
				}
			}

			for (int j = -1; j < 2; j++)
			{
				for (int i = -1; i < 2; i++)
				{
					pixelsArrayAutomatic[i + 1, j + 1] = bitmapImage.GetPixel(x + i, y + j).R;
				}
			}

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Assert.AreEqual(pixelsArrayAutomatic[i, j], pixelsArrayManual[i, j]);
				}
			}
		}
	}
}