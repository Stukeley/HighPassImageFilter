using System.Runtime.InteropServices;

namespace HighPassImageFilter.UserInterface
{
	public static class CppCaller
	{
		[DllImport(@"C:\Programowanie\HighPassImageFilter\x64\Debug\TestCppDll.dll")]
		public static extern byte[] ApplyFilterToImage(byte[] bitmap, int width, int height);
	}
}
