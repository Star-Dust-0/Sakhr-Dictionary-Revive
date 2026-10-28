using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT;
using WinRT.Interop;
using Windows.Graphics;
using Windows.UI;

namespace FluentSakhrDictionary;

public partial class MainWindow : Window
{
	private readonly Settings _settings;

	public const string DefaultAccentHex = "#00A6A6";

	public MainWindow()
	{
		InitializeComponent();
		_settings = Settings.Load();
		_ = DictionaryService.CoreReady;
		Loc.Apply(_settings);
		base.Title = "Sakhr Dictionary Revive - قاموس صخر الحديث";
		base.ExtendsContentIntoTitleBar = true;
		base.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
		SetTitleBar(AppTitleBar);
		base.AppWindow.Changed += delegate
		{
			UpdateMaximizeGlyph();
		};
		base.AppWindow.Resize(new SizeInt32(1200, 700));
		try
		{
			base.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "app.ico"));
		}
		catch
		{
		}
		ApplyTheme(_settings.Theme);
		ApplyBackdrop(_settings.Backdrop);
		SetAccent(ParseAccent(_settings.Accent), save: false);
		try
		{
			WindowChrome.Apply(WindowNative.GetWindowHandle(this));
		}
		catch
		{
		}
		ApplyLanguage();
		ContentFrame.Navigate(typeof(SearchPage));
		base.Activated += delegate
		{
			try
			{
				nint windowHandle = WindowNative.GetWindowHandle(this);
				WindowChrome.Apply(windowHandle);
				WindowChrome.ClearNativeMirror(windowHandle);
			}
			catch
			{
			}
			if (ContentFrame.Content is SearchPage searchPage)
			{
				searchPage.FocusSearchBox();
			}
		};
		if (App.ScreenshotPath != null)
		{
			RunScreenshot();
		}
	}

	public void ApplyLanguage()
	{
		Root.FlowDirection = (Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
		try
		{
			WindowChrome.ClearNativeMirror(WindowNative.GetWindowHandle(this));
		}
		catch
		{
		}
		AutomationProperties.SetName(MinimizeButton, Loc.T("Minimize", "تصغير"));
		AutomationProperties.SetName(MaximizeButton, Loc.T("Maximize or restore", "تكبير أو استعادة"));
		AutomationProperties.SetName(CloseButton, Loc.T("Close", "إغلاق"));
		NavSearch.Content = Loc.T("Search", "بحث");
		NavSettings.Content = Loc.T("Settings", "الإعدادات");
		NavAbout.Content = Loc.T("About", "حول");
	}

	private void Minimize_Click(object sender, RoutedEventArgs e)
	{
		if (base.AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
		{
			overlappedPresenter.Minimize();
		}
	}

	private void Maximize_Click(object sender, RoutedEventArgs e)
	{
		if (base.AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
		{
			if (overlappedPresenter.State == OverlappedPresenterState.Maximized)
			{
				overlappedPresenter.Restore();
			}
			else
			{
				overlappedPresenter.Maximize();
			}
			UpdateMaximizeGlyph();
		}
	}

	private void UpdateMaximizeGlyph()
	{
		if (base.AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
		{
			MaximizeGlyph.Glyph = ((overlappedPresenter.State == OverlappedPresenterState.Maximized) ? "\ue923" : "\ue922");
		}
	}

	private void Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	public void RefreshCurrentPage()
	{
		if (ContentFrame.Content != null)
		{
			ContentFrame.Navigate(ContentFrame.Content.GetType());
		}
	}

	private async void RunScreenshot()
	{
		_ = 4;
		try
		{
			Log("start theme=" + App.ShotTheme + " word=" + App.ShotWord + " path=" + App.ScreenshotPath);
			if (App.ShotWidth == 0)
			{
				base.AppWindow.Resize(new SizeInt32(1000, 650));
			}
			if (App.ShotTheme != null)
			{
				ApplyTheme(App.ShotTheme);
			}
			if (App.ShotWidth > 0)
			{
				base.AppWindow.Resize(new SizeInt32(App.ShotWidth, App.ShotHeight));
				Log("resized " + App.ShotWidth + "x" + App.ShotHeight);
			}
			await Task.Delay(1200);
			if (App.ShotPage == "settings")
			{
				ContentFrame.Navigate(typeof(SettingsPage));
				Log("settings page");
			}
			if (App.ShotPage == "settings-nav")
			{
				Nav.SelectedItem = NavSettings;
				Log("settings page via nav transition");
			}
			if (App.ShotPage == "about")
			{
				Nav.SelectedItem = NavAbout;
				Log("about page via nav transition");
			}
			if (App.ShotWord != null && ContentFrame.Content is SearchPage searchPage)
			{
				await searchPage.TypeAndSelect(App.ShotWord);
				Log("word typed");
			}
			await Task.Delay(1500);
			if (App.ShotPage == "about-end")
			{
				Nav.SelectedItem = NavAbout;
				await Task.Delay(900);
				if (ContentFrame.Content is AboutPage aboutPage)
				{
					aboutPage.ScrollToBottom();
					Log("about scrolled to end");
				}
				await Task.Delay(400);
			}
			nint windowHandle = WindowNative.GetWindowHandle(this);
			Log("hwnd=" + (IntPtr)windowHandle);
			CaptureHelper.Save(windowHandle, App.ScreenshotPath);
			Log("saved");
		}
		catch (Exception ex)
		{
			Log("FAILED " + ex);
		}
		Environment.Exit(0);
		static void Log(string line)
		{
			try
			{
				Directory.CreateDirectory(App.AppData);
				File.AppendAllText(Path.Combine(App.AppData, "crash.log"), DateTime.Now.ToString() + " SHOT " + line + "\n");
			}
			catch
			{
			}
		}
	}

	private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (args.SelectedItem is NavigationViewItem navigationViewItem)
		{
			string text = navigationViewItem.Tag as string;
			Type type = ((text == "settings") ? typeof(SettingsPage) : ((!(text == "about")) ? typeof(SearchPage) : typeof(AboutPage)));
			Type sourcePageType = type;
			ContentFrame.Navigate(sourcePageType, null, new EntranceNavigationTransitionInfo());
		}
	}

	public static Color ParseAccent(string? hex)
	{
		try
		{
			string text = (hex ?? "").TrimStart('#');
			if (text.Length == 6)
			{
				return Color.FromArgb(byte.MaxValue, Convert.ToByte(text.Substring(0, 2), 16), Convert.ToByte(text.Substring(2, 2), 16), Convert.ToByte(text.Substring(4), 16));
			}
		}
		catch
		{
		}
		return Color.FromArgb(byte.MaxValue, 0, 166, 166);
	}

	private static Color Mix(Color c, byte tr, byte tg, byte tb, double t)
	{
		return Color.FromArgb(byte.MaxValue, (byte)((double)(int)c.R + (double)(tr - c.R) * t), (byte)((double)(int)c.G + (double)(tg - c.G) * t), (byte)((double)(int)c.B + (double)(tb - c.B) * t));
	}

	private static void ApplyAccentResources(Color c)
	{
		ResourceDictionary resources = Application.Current.Resources;
		resources["SystemAccentColor"] = c;
		resources["SystemAccentColorDark1"] = Mix(c, 0, 0, 0, 0.18);
		resources["SystemAccentColorDark2"] = Mix(c, 0, 0, 0, 0.36);
		resources["SystemAccentColorDark3"] = Mix(c, 0, 0, 0, 0.54);
		resources["SystemAccentColorLight1"] = Mix(c, byte.MaxValue, byte.MaxValue, byte.MaxValue, 0.18);
		resources["SystemAccentColorLight2"] = Mix(c, byte.MaxValue, byte.MaxValue, byte.MaxValue, 0.36);
		resources["SystemAccentColorLight3"] = Mix(c, byte.MaxValue, byte.MaxValue, byte.MaxValue, 0.54);
	}

	public void SetAccent(Color c, bool save)
	{
		ApplyAccentResources(c);
		if (save)
		{
			Settings settings = Settings.Load();
			settings.Accent = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
			if (settings.Accent.Equals("#00A6A6", StringComparison.OrdinalIgnoreCase))
			{
				settings.Accent = "";
			}
			settings.Save();
		}
		RefreshAccent();
	}

	private void RefreshAccent()
	{
		if (base.Content is FrameworkElement { RequestedTheme: var requestedTheme } frameworkElement)
		{
			frameworkElement.RequestedTheme = ((requestedTheme == ElementTheme.Dark) ? ElementTheme.Light : ElementTheme.Dark);
			frameworkElement.RequestedTheme = requestedTheme;
		}
	}

	public void ApplyTheme(string theme)
	{
		if (base.Content is FrameworkElement frameworkElement)
		{
			FrameworkElement frameworkElement2 = frameworkElement;
			ElementTheme requestedTheme = ((theme == "Light") ? ElementTheme.Light : ((theme == "Dark") ? ElementTheme.Dark : ElementTheme.Default));
			frameworkElement2.RequestedTheme = requestedTheme;
		}
	}

	public void ApplyBackdrop(string backdrop)
	{
		SystemBackdrop systemBackdrop = ((backdrop == "MicaAlt") ? new MicaBackdrop
		{
			Kind = MicaKind.BaseAlt
		} : ((!(backdrop == "Acrylic")) ? ((SystemBackdrop)new MicaBackdrop()) : ((SystemBackdrop)new DesktopAcrylicBackdrop())));
		base.SystemBackdrop = systemBackdrop;
	}

}
