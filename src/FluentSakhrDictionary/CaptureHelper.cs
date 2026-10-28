using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FluentSakhrDictionary;

internal static class CaptureHelper
{
	private struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	[DllImport("user32.dll")]
	private static extern bool PrintWindow(nint hwnd, nint hdc, uint flags);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(nint hwnd, out RECT rect);

	[DllImport("user32.dll")]
	private static extern nint GetDC(nint hwnd);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(nint hwnd, nint hdc);

	[DllImport("gdi32.dll")]
	private static extern nint CreateCompatibleDC(nint hdc);

	[DllImport("gdi32.dll")]
	private static extern nint CreateCompatibleBitmap(nint hdc, int w, int h);

	[DllImport("gdi32.dll")]
	private static extern nint SelectObject(nint hdc, nint obj);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(nint obj);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteDC(nint hdc);

	public static void Save(nint hwnd, string path)
	{
		if (hwnd == IntPtr.Zero)
		{
			throw new InvalidOperationException("window handle is zero");
		}
		if (!GetWindowRect(hwnd, out var rect))
		{
			throw new InvalidOperationException("GetWindowRect failed");
		}
		int num = rect.Right - rect.Left;
		int num2 = rect.Bottom - rect.Top;
		if (num <= 0 || num2 <= 0)
		{
			throw new InvalidOperationException("window rect is empty: " + num + "x" + num2);
		}
		nint dC = GetDC(IntPtr.Zero);
		if (dC == IntPtr.Zero)
		{
			throw new InvalidOperationException("GetDC failed");
		}
		nint num3 = CreateCompatibleDC(dC);
		if (num3 == IntPtr.Zero)
		{
			ReleaseDC(IntPtr.Zero, dC);
			throw new InvalidOperationException("CreateCompatibleDC failed");
		}
		nint num4 = CreateCompatibleBitmap(dC, num, num2);
		if (num4 == IntPtr.Zero)
		{
			DeleteDC(num3);
			ReleaseDC(IntPtr.Zero, dC);
			throw new InvalidOperationException("CreateCompatibleBitmap failed");
		}
		nint obj = SelectObject(num3, num4);
		bool flag = PrintWindow(hwnd, num3, 2u);
		if (!flag)
		{
			flag = PrintWindow(hwnd, num3, 0u);
		}
		if (!flag)
		{
			SelectObject(num3, obj);
			DeleteObject(num4);
			DeleteDC(num3);
			ReleaseDC(IntPtr.Zero, dC);
			throw new InvalidOperationException("PrintWindow failed");
		}
		using (Bitmap bitmap = Image.FromHbitmap(num4))
		{
			bitmap.Save(path, ImageFormat.Png);
		}
		SelectObject(num3, obj);
		DeleteObject(num4);
		DeleteDC(num3);
		ReleaseDC(IntPtr.Zero, dC);
	}
}
