using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace FluentSakhrDictionarySetup;

internal static class Program
{
	private const string Product = "Sakhr Dictionary Revive";

	private const string Version = "1.0.2";

	private static readonly HttpClient Http = new HttpClient
	{
		Timeout = TimeSpan.FromMinutes(15.0)
	};

	private static bool _dark;

	private static string Target => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SakhrDictionaryRevive");

	private static SolidColorBrush TextBrush => new SolidColorBrush(_dark ? Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue) : Color.FromRgb(0, 0, 0));

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint h, int a, ref int v, int s);

	[STAThread]
	private static void Main(string[] args)
	{
		//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3a: Unknown result type (might be due to invalid IL or missing references)
		if (args.Contains("--uninstall"))
		{
			if (MessageBox.Show("Remove Sakhr Dictionary Revive? Your personal settings stay untouched.", "Sakhr Dictionary Revive", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				string text = Path.Combine(Path.GetTempPath(), "SakhrDictionaryRevive-Uninstall-" + Guid.NewGuid().ToString() + ".exe");
				File.Copy(Environment.ProcessPath, text);
				Process.Start(new ProcessStartInfo(text, "--remove")
				{
					UseShellExecute = true
				});
			}
			return;
		}
		if (args.Contains("--remove"))
		{
			try
			{
				StopApp();
				Directory.Delete(Target, recursive: true);
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "Sakhr Dictionary Revive.lnk");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				string path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Sakhr Dictionary Revive.lnk");
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\SakhrDictionaryRevive", throwOnMissingSubKey: false);
				MessageBox.Show("Sakhr Dictionary Revive was removed. Your personal settings were kept.", "Sakhr Dictionary Revive");
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not remove: " + ex.Message, "Sakhr Dictionary Revive");
				return;
			}
		}
		if (args.Contains("--verify-install"))
		{
			Install(delegate
			{
			});
			return;
		}
		_dark = args.Contains("--preview-dark") || (Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", 1) is int num && num == 0);
		Application application = new Application();
		Window window = new Window
		{
			Title = "Sakhr Dictionary Revive Setup",
			Width = 490.0,
			Height = 615.0,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = WindowStartupLocation.CenterScreen,
			Background = new SolidColorBrush(_dark ? Color.FromRgb(32, 32, 32) : Color.FromRgb(243, 243, 246)),
			FontFamily = new FontFamily("Segoe UI")
		};
		window.SourceInitialized += delegate
		{
			nint handle = new WindowInteropHelper(window).Handle;
			int v = 2;
			int v2 = 2;
			int v3 = (_dark ? 1 : 0);
			try
			{
				DwmSetWindowAttribute(handle, 38, ref v, 4);
				DwmSetWindowAttribute(handle, 33, ref v2, 4);
				DwmSetWindowAttribute(handle, 20, ref v3, 4);
			}
			catch
			{
			}
		};
		StackPanel stackPanel = new StackPanel
		{
			Margin = new Thickness(28.0)
		};
		stackPanel.Children.Add(new TextBlock
		{
			Text = "About",
			FontSize = 24.0,
			FontWeight = FontWeights.SemiBold,
			Margin = new Thickness(0.0, 0.0, 0.0, 18.0),
			Foreground = TextBrush
		});
		StackPanel stackPanel2 = new StackPanel
		{
			Margin = new Thickness(20.0),
			HorizontalAlignment = HorizontalAlignment.Center
		};
		stackPanel2.Children.Add(new Border
		{
			Width = 104.0,
			Height = 104.0,
			CornerRadius = new CornerRadius(18.0),
			Background = new SolidColorBrush(Color.FromRgb(0, 166, 166)),
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(0.0, 0.0, 0.0, 14.0),
			Child = new Image
			{
				Width = 72.0,
				Height = 72.0,
				Source = new BitmapImage(new Uri("pack://application:,,,/logo.png"))
			}
		});
		stackPanel2.Children.Add(Text("Sakhr Dictionary Revive", 24.0));
		stackPanel2.Children.Add(Text("قاموس صخر", 16.0));
		stackPanel2.Children.Add(Text("Version 1.0.2", 14.0));
		stackPanel2.Children.Add(Text("brought to you by Instinct", 14.0));
		stackPanel2.Children.Add(Text("Developer: Mohamed Elnaggar", 14.0));
		stackPanel.Children.Add(new Border
		{
			CornerRadius = new CornerRadius(12.0),
			Background = new SolidColorBrush(_dark ? Color.FromRgb(43, 43, 43) : Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue)),
			BorderBrush = new SolidColorBrush(_dark ? Color.FromRgb(64, 64, 64) : Color.FromRgb(224, 224, 230)),
			BorderThickness = new Thickness(1.0),
			Child = stackPanel2
		});
		stackPanel.Children.Add(new TextBlock
		{
			Text = "The app installs into Program Files. Requirements are checked and downloaded from Microsoft when missing, so an internet connection may be needed once.",
			TextWrapping = TextWrapping.Wrap,
			FontSize = 13.0,
			Margin = new Thickness(0.0, 18.0, 0.0, 12.0),
			Foreground = new SolidColorBrush(_dark ? Color.FromRgb(160, 160, 160) : Color.FromRgb(100, 100, 100))
		});
		TextBlock status = new TextBlock
		{
			Text = "Ready to install",
			TextWrapping = TextWrapping.Wrap,
			FontSize = 13.0,
			Margin = new Thickness(0.0, 0.0, 0.0, 10.0),
			Foreground = TextBrush
		};
		stackPanel.Children.Add(status);
		ProgressBar progress = new ProgressBar
		{
			Height = 6.0,
			Visibility = Visibility.Collapsed,
			IsIndeterminate = true,
			Margin = new Thickness(0.0, 0.0, 0.0, 12.0),
			Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74)),
			Background = new SolidColorBrush(_dark ? Color.FromRgb(64, 64, 64) : Color.FromRgb(222, 222, 228)),
			BorderThickness = new Thickness(0.0)
		};
		stackPanel.Children.Add(progress);
		Button install = new Button
		{
			Content = "install now",
			Height = 44.0,
			FontSize = 16.0,
			FontWeight = FontWeights.SemiBold,
			Foreground = Brushes.Black,
			Background = new SolidColorBrush(Color.FromRgb(76, 194, byte.MaxValue)),
			BorderThickness = new Thickness(0.0)
		};
		stackPanel.Children.Add(install);
		StackPanel done = new StackPanel
		{
			Visibility = Visibility.Collapsed,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		done.Children.Add(new Border
		{
			Width = 44.0,
			Height = 44.0,
			CornerRadius = new CornerRadius(22.0),
			Background = new SolidColorBrush(Color.FromRgb(22, 163, 74)),
			HorizontalAlignment = HorizontalAlignment.Center,
			Child = new TextBlock
			{
				Text = "\ue73e",
				FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe Fluent Icons"),
				FontSize = 22.0,
				Foreground = Brushes.White,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			}
		});
		done.Children.Add(new TextBlock
		{
			Text = "Installed",
			FontSize = 16.0,
			FontWeight = FontWeights.SemiBold,
			Foreground = TextBrush,
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(0.0, 8.0, 0.0, 0.0)
		});
		stackPanel.Children.Add(done);
		Action showDone = delegate
		{
			progress.Visibility = Visibility.Collapsed;
			install.Visibility = Visibility.Collapsed;
			status.Visibility = Visibility.Collapsed;
			done.Visibility = Visibility.Visible;
		};
		bool busy = false;
		window.Closing += delegate(object? _, CancelEventArgs e)
		{
			if (busy)
			{
				e.Cancel = true;
			}
		};
		install.Click += async delegate
		{
			if (!install.IsEnabled)
			{
				return;
			}
			busy = true;
			install.IsEnabled = false;
			progress.Visibility = Visibility.Visible;
			try
			{
				await Task.Run(delegate
				{
					Install(delegate(string text2)
					{
						((DispatcherObject)window).Dispatcher.Invoke<string>((Func<string>)(() => status.Text = text2));
					});
				});
				showDone();
				LaunchApp();
			}
			catch (Exception ex2)
			{
				status.Text = "Setup failed: " + ex2.Message + " You can try again.";
				install.IsEnabled = true;
				progress.Visibility = Visibility.Collapsed;
			}
			finally
			{
				busy = false;
			}
		};
		window.Content = stackPanel;
		if (args.Length == 2 && (args[0] == "--preview" || args[0] == "--preview-dark" || args[0] == "--preview-busy" || args[0] == "--preview-done"))
		{
			if (args[0] == "--preview-busy")
			{
				install.IsEnabled = false;
				progress.Visibility = Visibility.Visible;
				status.Text = "Installing app files...";
			}
			if (args[0] == "--preview-done")
			{
				showDone();
			}
			window.Content = null;
			Border border = new Border
			{
				Width = 490.0,
				Height = 580.0,
				Background = window.Background,
				Child = stackPanel
			};
			border.Measure(new Size(490.0, 580.0));
			border.Arrange(new Rect(0.0, 0.0, 490.0, 580.0));
			border.UpdateLayout();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(490, 580, 96.0, 96.0, PixelFormats.Pbgra32);
			renderTargetBitmap.Render(border);
			PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			using FileStream stream = File.Create(args[1]);
			pngBitmapEncoder.Save(stream);
			return;
		}
		application.Run(window);
	}

	private static void LaunchApp()
	{
		try
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "\"" + Path.Combine(Target, "SakhrDictionaryRevive.exe") + "\"")
			{
				UseShellExecute = true
			});
		}
		catch
		{
		}
	}

	private static TextBlock Text(string text, double size)
	{
		return new TextBlock
		{
			Text = text,
			FontSize = size,
			TextAlignment = TextAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(0.0, 4.0, 0.0, 4.0),
			Foreground = TextBrush
		};
	}

	private static void StopApp()
	{
		Process[] processesByName = Process.GetProcessesByName("SakhrDictionaryRevive");
		foreach (Process obj in processesByName)
		{
			obj.Kill();
			obj.WaitForExit(10000);
			obj.Dispose();
		}
	}

	private static string Run(string exe, string arguments)
	{
		using Process process = Process.Start(new ProcessStartInfo(exe, arguments)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		});
		Task<string> task = process.StandardOutput.ReadToEndAsync();
		Task<string> task2 = process.StandardError.ReadToEndAsync();
		process.WaitForExit();
		Task.WaitAll(task, task2);
		if (process.ExitCode != 0 && process.ExitCode != 3010)
		{
			throw new Exception("A requirement failed to install (" + process.ExitCode + "). " + task2.Result);
		}
		return task.Result;
	}

	private static bool DotNetPresent()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.NETCore.App");
		Version result;
		if (Directory.Exists(path))
		{
			return Directory.GetDirectories(path).Any((string d) => System.Version.TryParse(Path.GetFileName(d), out result) && result.Major == 8);
		}
		return false;
	}

	private static bool VcPresent()
	{
		if (Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64")?.GetValue("Installed") is int num)
		{
			return num == 1;
		}
		return false;
	}

	private static bool WarPresent()
	{
		return Run("powershell.exe", "-NoProfile -NonInteractive -Command \"[bool](Get-AppxPackage -AllUsers -Name Microsoft.WindowsAppRuntime.1.6 | Where-Object { $_.Architecture -eq 'X64' -and $_.Status -eq 'Ok' })\"").Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
	}

	private static void Dependency(string url, string arguments)
	{
		string text = Path.Combine(Path.GetTempPath(), "SakhrDictionaryRevive-" + Guid.NewGuid().ToString() + ".exe");
		try
		{
			using (Stream stream = Http.GetStreamAsync(url).GetAwaiter().GetResult())
			{
				using FileStream destination = File.Create(text);
				stream.CopyTo(destination);
			}
			string text2 = "$s=Get-AuthenticodeSignature -LiteralPath '" + text.Replace("'", "''") + "'; if($s.Status -ne 'Valid' -or $s.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') { exit 1 }";
			Run("powershell.exe", "-NoProfile -NonInteractive -Command \"" + text2 + "\"");
			Run(text, arguments);
		}
		finally
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
		}
	}

	private static void Install(Action<string> status)
	{
		if (!Environment.Is64BitOperatingSystem)
		{
			throw new Exception("The app requires Windows x64.");
		}
		status("Checking requirements...");
		if (!DotNetPresent())
		{
			status("Downloading and installing .NET 8...");
			Dependency("https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe", "/install /quiet /norestart");
			if (!DotNetPresent())
			{
				throw new Exception(".NET 8 did not finish installing.");
			}
		}
		if (!VcPresent())
		{
			status("Downloading and installing Visual C++...");
			Dependency("https://aka.ms/vc14/vc_redist.x64.exe", "/install /quiet /norestart");
			if (!VcPresent())
			{
				throw new Exception("Visual C++ did not finish installing.");
			}
		}
		if (!WarPresent())
		{
			status("Downloading and installing Windows App Runtime 1.6...");
			Dependency("https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe", "--quiet");
			if (!WarPresent())
			{
				throw new Exception("Windows App Runtime 1.6 did not finish installing.");
			}
		}
		status("Installing app files...");
		StopApp();
		Directory.CreateDirectory(Target);
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetManifestResourceNames().Single((string n) => n.EndsWith(".payload.zip"))) ?? throw new Exception("App payload is missing.");
		using ZipArchive source = new ZipArchive(stream);
		source.ExtractToDirectory(Target, overwriteFiles: true);
		File.Copy(Environment.ProcessPath, Path.Combine(Target, "Uninstall.exe"), overwrite: true);
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "Sakhr Dictionary Revive.lnk");
		string text2 = Target.Replace("'", "''");
		Run("powershell.exe", "-NoProfile -NonInteractive -Command \"$s=(New-Object -ComObject WScript.Shell).CreateShortcut('" + text.Replace("'", "''") + "');$s.TargetPath='" + text2 + "\\SakhrDictionaryRevive.exe';$s.WorkingDirectory='" + text2 + "';$s.IconLocation='" + text2 + "\\app.ico';$s.Save()\"");
		string text3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Sakhr Dictionary Revive.lnk");
		Run("powershell.exe", "-NoProfile -NonInteractive -Command \"$s=(New-Object -ComObject WScript.Shell).CreateShortcut('" + text3.Replace("'", "''") + "');$s.TargetPath='" + text2 + "\\SakhrDictionaryRevive.exe';$s.WorkingDirectory='" + text2 + "';$s.IconLocation='" + text2 + "\\app.ico';$s.Save()\"");
		using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\SakhrDictionaryRevive");
		registryKey.SetValue("DisplayName", "Sakhr Dictionary Revive");
		registryKey.SetValue("DisplayVersion", "1.0.2");
		registryKey.SetValue("Publisher", "Mohamed Elnaggar");
		registryKey.SetValue("InstallLocation", Target);
		registryKey.SetValue("DisplayIcon", Path.Combine(Target, "app.ico"));
		registryKey.SetValue("UninstallString", "\"" + Path.Combine(Target, "Uninstall.exe") + "\" --uninstall");
		registryKey.SetValue("NoModify", 1);
		registryKey.SetValue("NoRepair", 1);
	}
}
