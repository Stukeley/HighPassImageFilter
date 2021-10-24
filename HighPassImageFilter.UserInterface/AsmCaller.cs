using System.Runtime.InteropServices;

namespace HighPassImageFilter.UserInterface
{
	public static class AsmCaller
	{
		[DllImport(@"C:\Programowanie\HighPassImageFilter\x64\Debug\HighPassImageFilter.ASM.dll")]
		public static extern int SumElementsIn2DArray(byte[] array);
	}
}
