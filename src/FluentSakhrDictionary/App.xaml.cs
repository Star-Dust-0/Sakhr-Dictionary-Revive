using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using WinRT;

namespace FluentSakhrDictionary;

public partial class App : Application
{
	public static MainWindow? Main;

	public static string? ScreenshotPath;

	public static string? ShotWord;

	public static string? ShotTheme;

	public static string? ShotPage;

	public static int ShotWidth;

	public static int ShotHeight;

	private static Mutex? _mutex;

	private MainWindow? _window;

	public static string AppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SakhrDictionaryRevive");

	public App()
	{
		InitializeComponent();
		base.UnhandledException += delegate(object s, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			try
			{
				Directory.CreateDirectory(AppData);
				File.AppendAllText(Path.Combine(AppData, "crash.log"), DateTime.Now.ToString() + "\n" + e.Exception?.ToString() + "\n\n");
			}
			catch
			{
			}
		};
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text = commandLineArgs[i];
			if (text.StartsWith("--screenshot="))
			{
				ScreenshotPath = text.Substring(13);
			}
			else if (text == "--screenshot" && i + 1 < commandLineArgs.Length)
			{
				ScreenshotPath = commandLineArgs[++i];
			}
			if (text.StartsWith("--shot-word="))
			{
				ShotWord = text.Substring(12);
			}
			else if (text == "--shot-word" && i + 1 < commandLineArgs.Length)
			{
				ShotWord = commandLineArgs[++i];
			}
			if (text.StartsWith("--theme="))
			{
				ShotTheme = text.Substring(8);
			}
			else if (text == "--theme" && i + 1 < commandLineArgs.Length)
			{
				ShotTheme = commandLineArgs[++i];
			}
			if (text.StartsWith("--shot-page="))
			{
				ShotPage = text.Substring(12);
			}
			else if (text == "--shot-page" && i + 1 < commandLineArgs.Length)
			{
				ShotPage = commandLineArgs[++i];
			}
			if (text.StartsWith("--shot-size="))
			{
				string[] array = text.Substring(12).Split('x');
				ShotWidth = int.Parse(array[0]);
				ShotHeight = int.Parse(array[1]);
			}
		}
		_mutex = new Mutex(initiallyOwned: true, "SakhrDictionaryRevive.SingleInstance", out var createdNew);
		if (!createdNew)
		{
			Environment.Exit(0);
			return;
		}
		_window = new MainWindow();
		Main = _window;
		_window.Activate();
	}

}
