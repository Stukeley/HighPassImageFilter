using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HighPassImageFilter.UserInterface
{
	public static class CsCaller
	{
		public static Bitmap ApplyFilterToImage(Bitmap bitmap)
		{
			var dllName = "HighPassImageFilter.CS.dll";
			var dllType = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), dllName)).GetType("HighPassImageFilter.CS.HighPassFilter", true);
			var dllMethod = dllType.GetMethod("ApplyFilterToImageAsync");

			object newBitmapObject = dllMethod.Invoke(null, new object[] { bitmap });
			var newBitmapTask = newBitmapObject as Task<Bitmap>;
			var newBitmap = newBitmapTask.Result;

			return newBitmap;
		}
	}
}