using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using WinRT;
using Windows.UI;

namespace FluentSakhrDictionary;

public partial class SettingsPage : Page
{
	private static readonly string[] AccentSwatches = new string[10] { "#00A6A6", "#0078D4", "#5B5FC7", "#8764B8", "#C239B3", "#E81123", "#F7630C", "#FFB900", "#10893E", "#69797E" };

	private readonly Settings _settings;

	private bool _ready;

	private static bool _applyingLanguage;

	public SettingsPage()
	{
		InitializeComponent();
		_settings = Settings.Load();
		RadioButtons themePicker = ThemePicker;
		string theme = _settings.Theme;
		themePicker.SelectedIndex = ((theme == "Light") ? 1 : ((theme == "Dark") ? 2 : 0));
		themePicker = BackdropPicker;
		theme = _settings.Backdrop;
		themePicker.SelectedIndex = ((theme == "MicaAlt") ? 1 : ((theme == "Acrylic") ? 2 : 0));
		InitAccentSwatches();
		WiktionaryToggle.IsOn = _settings.UseWiktionary;
		themePicker = LanguagePicker;
		theme = _settings.Language;
		themePicker.SelectedIndex = ((theme == "en") ? 1 : ((theme == "ar") ? 2 : 0));
		LocalizeStatic();
		_ready = true;
	}

	private void LocalizeStatic()
	{
		HeaderText.Text = Loc.T("Settings", "الإعدادات");
		LanguageHeader.Text = Loc.T("Language", "اللغة");
		LangSystem.Content = Loc.T("Match device (default)", "مطابق للغة الجهاز (الافتراضي)");
		ThemeHeader.Text = Loc.T("Theme", "السمة");
		ThemeSystem.Content = Loc.T("Use system setting", "مطابق لإعدادات الجهاز");
		ThemeLight.Content = Loc.T("Light", "فاتحة");
		ThemeDark.Content = Loc.T("Dark", "داكنة");
		AccentHeader.Text = Loc.T("Accent color", "اللون المميز");
		AccentCustomButton.Content = Loc.T("Custom color...", "لون مخصص...");
		AccentResetButton.Content = Loc.T("Reset to default", "إعادة للافتراضي");
		DictionaryHeader.Text = Loc.T("Dictionary", "القاموس");
		WiktionaryToggle.Header = Loc.T("Wiktionary database", "قاعدة بيانات Wiktionary");
		WiktionaryToggle.OnContent = Loc.T("On", "م\u064fفع\u064e\u0651ل");
		WiktionaryToggle.OffContent = Loc.T("Off", "م\u064fعط\u064e\u0651ل");
		WiktionaryNote.Text = Loc.T("Added so the dictionary can understand word inflections (e.g. ABANDONS <- Abandon, يكتبون <- كتب). When off, the app uses the original 1996 Sakhr database only, English to Arabic and back.", "أ\u064fضيفت قاعدة بيانات Wiktionary ليتمكن القاموس من فهم تصريفات الكلمات (مثل ABANDONS ← Abandon ويكتبون ← كتب). عند إيقاف هذا الخيار يعمل التطبيق بقاعدة بيانات صخر الأصلية لعام 1996 فقط، من الإنجليزية إلى العربية ومن العربية إلى الإنجليزية.");
		BackdropHeader.Text = Loc.T("Backdrop", "الخلفية");
		BackdropMica.Content = "Mica";
		BackdropMicaAlt.Content = "Mica Alt";
		BackdropAcrylic.Content = Loc.T("Desktop Acrylic", "أكريليك سطح المكتب");
	}

	private void Language_Changed(object sender, SelectionChangedEventArgs e)
	{

		if (!_ready || _applyingLanguage || LanguagePicker.SelectedItem is not RadioButton selectedLanguage)
		{
			return;
		}
		_applyingLanguage = true;
		string text = selectedLanguage.Tag as string ?? "system";
		if (text == _settings.Language)
		{
			_applyingLanguage = false;
			return;
		}
		_settings.Language = text;
		_settings.Save();
		Loc.Apply(_settings);
		MainWindow main = App.Main;
		if ((object)main != null)
		{
			main.ApplyLanguage();
			main.RefreshCurrentPage();
		}
		_applyingLanguage = false;
	}

	private void Wiktionary_Toggled(object sender, RoutedEventArgs e)
	{
		if (_ready && WiktionaryToggle.IsOn != _settings.UseWiktionary)
		{
			_settings.UseWiktionary = WiktionaryToggle.IsOn;
			_settings.Save();
		}
	}

	private void Setting_Changed(object sender, SelectionChangedEventArgs e)
	{
		if (!_ready)
		{
			return;
		}
		string text = ((ThemePicker.SelectedItem as RadioButton)?.Tag as string) ?? "Default";
		string text2 = ((BackdropPicker.SelectedItem as RadioButton)?.Tag as string) ?? "Mica";
		if (!(text == _settings.Theme) || !(text2 == _settings.Backdrop))
		{
			_settings.Theme = text;
			_settings.Backdrop = text2;
			_settings.Save();
			MainWindow main = App.Main;
			if ((object)main != null)
			{
				main.ApplyTheme(_settings.Theme);
				main.ApplyBackdrop(_settings.Backdrop);
			}
		}
	}

	private void InitAccentSwatches()
	{
		int num = 0;
		string[] accentSwatches = AccentSwatches;
		for (int i = 0; i < accentSwatches.Length; i++)
		{
			Color color = MainWindow.ParseAccent(accentSwatches[i]);
			Button button = new Button
			{
				Width = 40.0,
				Height = 40.0,
				Padding = new Thickness(0.0),
				CornerRadius = new CornerRadius(4.0),
				Background = new SolidColorBrush(color),
				Tag = color
			};
			button.Click += delegate(object s, RoutedEventArgs e)
			{
				App.Main?.SetAccent((Color)((Button)s).Tag, save: true);
			};
			((num++ < 5) ? AccentRow1 : AccentRow2).Children.Add(button);
		}
	}

	private async void AccentCustom_Click(object sender, RoutedEventArgs e)
	{
		ColorPicker colorPicker = new ColorPicker
		{
			Color = MainWindow.ParseAccent(_settings.Accent),
			IsAlphaEnabled = false,
			IsMoreButtonVisible = true,
			IsColorChannelTextInputVisible = true,
			IsHexInputVisible = true
		};
		colorPicker.ColorChanged += delegate(ColorPicker s2, ColorChangedEventArgs a2)
		{
			App.Main?.SetAccent(a2.NewColor, save: true);
		};
		ContentDialog contentDialog = new ContentDialog
		{
			XamlRoot = base.XamlRoot,
			Title = Loc.T("Custom accent color", "لون مميز مخصص"),
			Content = new ScrollViewer
			{
				Content = colorPicker,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
			},
			CloseButtonText = Loc.T("Done", "تم"),
			DefaultButton = ContentDialogButton.Close
		};
		try
		{
			await contentDialog.ShowAsync();
		}
		catch
		{
		}
	}

	private void AccentReset_Click(object sender, RoutedEventArgs e)
	{
		App.Main?.SetAccent(MainWindow.ParseAccent("#00A6A6"), save: true);
		_settings.Accent = "";
		_settings.Save();
	}

}
