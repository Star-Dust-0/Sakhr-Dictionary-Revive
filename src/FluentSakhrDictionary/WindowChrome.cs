using System;
using System.Runtime.InteropServices;

namespace FluentSakhrDictionary;

internal static class WindowChrome
{
	private const int GWL_EXSTYLE = -20;

	private const long WS_EX_LAYOUTRTL = 4194304L;

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint h, int attr, ref int val, int size);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
	private static extern nint GetWindowLongPtr(nint h, int i);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
	private static extern nint SetWindowLongPtr(nint h, int i, nint v);

	public static void ClearNativeMirror(nint hwnd)
	{
		try
		{
			long num = ((IntPtr)GetWindowLongPtr(hwnd, -20)).ToInt64();
			SetWindowLongPtr(hwnd, -20, new IntPtr(num & -4194305));
		}
		catch
		{
		}
	}

	public static void Apply(nint hwnd)
	{
		try
		{
			int val = -2;
			int val2 = 2;
			DwmSetWindowAttribute(hwnd, 34, ref val, 4);
			DwmSetWindowAttribute(hwnd, 33, ref val2, 4);
		}
		catch
		{
		}
	}
}
