using System.Drawing;
using System.IO;

namespace HighPassImageFilter.UserInterface
{
	public static class BitmapConverter
	{
		public static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
		{
			using (var memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
				return memoryStream.ToArray();
			}
		}

		public static Bitmap ConvertByteArrayToBitmap(byte[] bitmap)
		{
			using (var memoryStream = new MemoryStream(bitmap))
			{
				return new Bitmap(memoryStream);
			}
		}
	}
}
