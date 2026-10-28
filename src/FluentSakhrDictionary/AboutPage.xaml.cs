using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using WinRT;

namespace FluentSakhrDictionary;

public partial class AboutPage : Page
{

	private static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";

	public void ScrollToBottom()
	{
		RootScroll.ChangeView(null, RootScroll.ScrollableHeight, null);
	}

	public AboutPage()
	{
		InitializeComponent();
		string currentVersion = CurrentVersion;
		VersionText.Text = Loc.T("Version ", "الإصدار ") + currentVersion;
		HeaderText.Text = Loc.T("About", "حول");
		DescText.Text = Loc.T("A modern rebirth of the classic 1996 Sakhr English-Arabic dictionary: 59,427 lemmas, 125,896 Arabic meanings with full diacritics.", "إحياء عصري لقاموس صخر الإنجليزي-العربي الصادر عام 1996: 59,427 مدخلة و125,896 معنى عربي\u064bا بالتشكيل الكامل.");
		OfflineText.Text = Loc.T("Works fully offline - both databases are bundled inside the app, no internet needed.", "يعمل دون اتصال بالإنترنت إطلاق\u064bا - كلتا قاعدتي البيانات مضم\u0651نتان داخل التطبيق.");
		DevText.Text = Loc.T("Developer: Mohamed Elnaggar", "المطور: Mohamed Elnaggar");
		ContribText.Text = Loc.T("Contributors: app.instinct.com", "المساهمون: app.instinct.com");
		LicenseText.Text = Loc.T("License: MIT", "الترخيص: MIT");
		DataEnText.Text = Loc.T("English-Arabic data: the original 1996 Sakhr dictionary", "بيانات إنجليزي-عربي: قاموس صخر الأصلي لعام 1996");
		DataArText.Text = Loc.T("Arabic-English data: English Wiktionary via kaikki.org (wiktextract), CC BY-SA 4.0", "بيانات عربي-إنجليزي: قاموس Wiktionary الإنجليزي عبر kaikki.org (wiktextract)، رخصة CC BY-SA 4.0");
		CreditLink.Content = Loc.T("brought to you by Instinct", "مقد\u064e\u0651م لكم من Instinct");
		RepoLink.Content = Loc.T("Github Repo Link", "رابط المستودع على Github");
		UpdatesHeader.Text = Loc.T("Updates", "التحديثات");
		UpdatesNote.Text = Loc.T("The app never checks for updates on its own. Press the button to check once, right now.", "لا يتحقق التطبيق من التحديثات من تلقاء نفسه. اضغط الزر للتحقق مرة واحدة الآن.");
		CheckButton.Content = Loc.T("Check for updates", "التحقق من التحديثات");
		UpdateLink.Content = Loc.T("Open download page", "فتح صفحة التنزيل");
	}

	private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
	{
		CheckButton.IsEnabled = false;
		UpdateLink.Visibility = Visibility.Collapsed;
		UpdateResult.Text = Loc.T("Checking...", "جار\u064d التحقق...");
		try
		{
			using HttpClient http = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(20.0)
			};
			http.DefaultRequestHeaders.UserAgent.ParseAdd("SakhrDictionaryRevive/" + CurrentVersion);
			http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
			using HttpResponseMessage response = await http.GetAsync("https://api.github.com/repos/StarsDust01/Sakhr-Dictionary-Revive/releases?per_page=1");
			response.EnsureSuccessStatusCode();
			using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
			string text = "";
			using (JsonElement.ArrayEnumerator arrayEnumerator = jsonDocument.RootElement.EnumerateArray().GetEnumerator())
			{
				if (arrayEnumerator.MoveNext())
				{
					text = (arrayEnumerator.Current.GetProperty("tag_name").GetString() ?? "").TrimStart('v', 'V');
				}
			}
			if (Version.TryParse(text, out Version result) && Version.TryParse(CurrentVersion, out Version result2) && result > result2)
			{
				UpdateResult.Text = Loc.T("Version " + text + " is available.", "الإصدار " + text + " متاح.");
				UpdateLink.NavigateUri = new Uri("https://github.com/StarsDust01/Sakhr-Dictionary-Revive/releases/tag/v" + text);
				UpdateLink.Visibility = Visibility.Visible;
			}
			else
			{
				UpdateResult.Text = ((text.Length > 0) ? Loc.T("You're up to date (v" + CurrentVersion + ").", "أنت على أحدث إصدار (v" + CurrentVersion + ").") : Loc.T("No releases found yet.", "لا توجد إصدارات بعد."));
			}
		}
		catch
		{
			UpdateResult.Text = Loc.T("Couldn't check for updates. Check your internet connection and try again.", "تعذر التحقق من التحديثات. تحقق من اتصالك بالإنترنت وحاول مرة أخرى.");
		}
		finally
		{
			CheckButton.IsEnabled = true;
		}
	}

}
