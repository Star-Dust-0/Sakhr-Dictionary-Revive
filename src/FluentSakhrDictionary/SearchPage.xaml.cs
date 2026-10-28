using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using WinRT;
using Windows.System;

namespace FluentSakhrDictionary;

public partial class SearchPage : Page
{
	private readonly DispatcherTimer _debounce = new DispatcherTimer
	{
		Interval = TimeSpan.FromMilliseconds(120.0)
	};

	private IReadOnlyList<Entry> _current = Array.Empty<Entry>();

	private IReadOnlyList<ArabicResult> _currentRev = Array.Empty<ArabicResult>();

	private Inflection? _currentInflection;

	private ArabicInflection? _currentArInflection;

	private bool _useWiktionary = true;

	private int _searchVersion;

	public SearchPage()
	{
		InitializeComponent();
		SearchBox.PlaceholderText = Loc.T("Type a word - اكتب كلمة", "اكتب كلمة - Type a word");
		SakhrHeader.Text = Loc.T("From the Sakhr dictionary:", "من قاموس صخر:");
		WikHeader.Text = Loc.T("From Wiktionary:", "من Wiktionary:");
		HomeOfflineText.Text = Loc.T("Works fully offline - no internet needed.", "يعمل تمام\u064bا دون اتصال بالإنترنت.");
		_debounce.Tick += delegate
		{
			_debounce.Stop();
			RunSearch();
		};
		base.Loaded += delegate
		{
			_ = DictionaryService.CoreReady;
			SearchBox.Focus(FocusState.Programmatic);
		};
	}

	public void FocusSearchBox()
	{
		SearchBox.Focus(FocusState.Programmatic);
	}

	public async Task TypeAndSelect(string word)
	{
		SearchBox.Text = word;
		await RunSearch();
		if (_current.Count > 0)
		{
			ResultsList.SelectedIndex = 0;
			ShowEntry(_current[0]);
		}
		else if (_currentRev.Count > 0)
		{
			ResultsList.SelectedIndex = 0;
			ShowReverse(_currentRev[0]);
		}
		else if (_currentInflection != null)
		{
			ShowInflection(_currentInflection);
		}
		else if (_currentArInflection != null)
		{
			ShowReverseInflection(_currentArInflection);
		}
	}

	private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		_debounce.Stop();
		_debounce.Start();
	}

	private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		if (e.Key == VirtualKey.Enter)
		{
			if (_current.Count > 0)
			{
				ResultsList.SelectedIndex = 0;
				ShowEntry(_current[0]);
			}
			else if (_currentRev.Count > 0)
			{
				ResultsList.SelectedIndex = 0;
				ShowReverse(_currentRev[0]);
			}
			else if (_currentInflection != null)
			{
				ShowInflection(_currentInflection);
			}
			else if (_currentArInflection != null)
			{
				ShowReverseInflection(_currentArInflection);
			}
		}
		if (e.Key == VirtualKey.Down && ResultsList.Items.Count > 0)
		{
			ResultsList.SelectedIndex = Math.Min(ResultsList.SelectedIndex + 1, ResultsList.Items.Count - 1);
			ResultsList.Focus(FocusState.Programmatic);
		}
	}

	private async Task<Inflection?> InflectionOfEn(string q)
	{
		await DictionaryService.WikReady;
		return DictionaryService.InflectionOf(q);
	}

	private static bool HasArabic(string q)
	{
		return DictionaryService.HasArabicText(q);
	}

	private async Task RunSearch()
	{
		int version = ++_searchVersion;
		string q = SearchBox.Text.Trim();
		_useWiktionary = Settings.Load().UseWiktionary;
		await DictionaryService.CoreReady;
		if (version != _searchVersion)
		{
			return;
		}
		HomeOfflineText.Visibility = Visibility.Collapsed;
		if (q.Length == 0)
		{
			_current = Array.Empty<Entry>();
			_currentRev = Array.Empty<ArabicResult>();
			_currentInflection = null;
			_currentArInflection = null;
			ResultsList.ItemsSource = null;
			ResetPane();
			return;
		}
		if (HasArabic(q))
		{
			_current = Array.Empty<Entry>();
			_currentInflection = null;
			_currentArInflection = null;
			if (_useWiktionary)
			{
				await DictionaryService.WikReady;
				if (version != _searchVersion)
				{
					return;
				}
			}
			_currentRev = DictionaryService.SearchArabic(q, _useWiktionary);
			ResultsList.ItemsSource = _currentRev;
			ArabicResult arabicResult = _currentRev.FirstOrDefault((ArabicResult x) => DictionaryService.NormalizeArabic(x.ArabicTerm) == DictionaryService.NormalizeArabic(q));
			if (_currentRev.Count == 1 || arabicResult != null)
			{
				ArabicResult arabicResult2 = arabicResult ?? _currentRev[0];
				ResultsList.SelectedItem = arabicResult2;
				ShowReverse(arabicResult2);
				return;
			}
			if (_useWiktionary)
			{
				ArabicInflection arabicInflection = DictionaryService.InflectionOfArabic(q);
				if ((object)arabicInflection != null)
				{
					_currentArInflection = arabicInflection;
					ShowReverseInflection(arabicInflection);
					return;
				}
			}
			if (_currentRev.Count == 0)
			{
				ShowNotFound(q);
			}
			return;
		}
		_currentRev = Array.Empty<ArabicResult>();
		_current = DictionaryService.Search(q);
		ResultsList.ItemsSource = _current;
		Entry exact = _current.FirstOrDefault((Entry x) => x.Word.Equals(q, StringComparison.OrdinalIgnoreCase));
		_currentArInflection = null;
		if ((object)exact != null && exact.Found)
		{
			ResultsList.SelectedItem = exact;
			ShowEntry(exact);
			_currentInflection = null;
			return;
		}
		bool flag = _useWiktionary;
		Inflection inflection = default(Inflection);
		if (flag)
		{
			inflection = await InflectionOfEn(q);
			flag = (object)inflection != null;
		}
		if (flag)
		{
			Entry entry = DictionaryService.Exact(inflection.Lemma);
			if ((object)entry != null && entry.Found)
			{
				_currentInflection = inflection;
				ShowInflection(inflection);
				return;
			}
		}
		if (_current.Count == 1 || exact != null)
		{
			Entry entry2 = exact ?? _current[0];
			ResultsList.SelectedItem = entry2;
			ShowEntry(entry2);
			_currentInflection = null;
		}
		else
		{
			_currentInflection = null;
			if (_current.Count == 0)
			{
				ShowNotFound(q);
			}
		}
	}

	private void ShowNotFound(string q)
	{
		WordTitle.FlowDirection = (HasArabic(q) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
		WordTitle.Text = q;
		WordSubtitle.Text = "";
		MeaningsRepeater.ItemsSource = null;
		MeaningsRepeater.Visibility = Visibility.Collapsed;
		EnglishRepeater.ItemsSource = null;
		EnglishRepeater.Visibility = Visibility.Collapsed;
		WikHeader.Visibility = Visibility.Collapsed;
		SakhrHeader.Visibility = Visibility.Collapsed;
		SakhrRepeater.ItemsSource = null;
		SakhrRepeater.Visibility = Visibility.Collapsed;
		NotFoundText.Text = Loc.NotFound;
		NotFoundText.Visibility = Visibility.Visible;
	}

	private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		object selectedItem = ResultsList.SelectedItem;
		if (!(selectedItem is Entry entry))
		{
			if (selectedItem is ArabicResult rev)
			{
				ShowReverse(rev);
			}
		}
		else
		{
			ShowEntry(entry);
		}
	}

	private void ShowEntry(Entry entry)
	{
		WordTitle.FlowDirection = FlowDirection.LeftToRight;
		WordTitle.Text = entry.DisplayWord;
		EnglishRepeater.ItemsSource = null;
		EnglishRepeater.Visibility = Visibility.Collapsed;
		WikHeader.Visibility = Visibility.Collapsed;
		SakhrHeader.Visibility = Visibility.Collapsed;
		SakhrRepeater.ItemsSource = null;
		SakhrRepeater.Visibility = Visibility.Collapsed;
		if (entry.Found)
		{
			WordSubtitle.FlowDirection = (Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
			WordSubtitle.Text = ((entry.Note.Length > 0) ? (Loc.EntryNote(entry.Note) + " - ") : "") + Loc.Meanings(entry.Meanings.Length);
			MeaningsRepeater.Visibility = Visibility.Visible;
			MeaningsRepeater.ItemsSource = entry.Meanings;
			NotFoundText.Visibility = Visibility.Collapsed;
			return;
		}
		string[] array = (_useWiktionary ? DictionaryService.TranslationsOf(entry.Word) : null);
		if (array != null && array.Length > 0)
		{
			WordSubtitle.Text = Loc.MeaningsFromWiktionary(array.Length);
			MeaningsRepeater.Visibility = Visibility.Visible;
			MeaningsRepeater.ItemsSource = array;
			NotFoundText.Visibility = Visibility.Collapsed;
		}
		else
		{
			WordSubtitle.Text = "";
			MeaningsRepeater.ItemsSource = null;
			NotFoundText.Text = Loc.NotFound;
			NotFoundText.Visibility = Visibility.Visible;
		}
	}

	private void ShowInflection(Inflection inf)
	{
		Entry entry = DictionaryService.Exact(inf.Lemma);
		if ((object)entry == null || !entry.Found)
		{
			_currentInflection = null;
			ResetPane();
			return;
		}
		WordTitle.FlowDirection = FlowDirection.LeftToRight;
		WordTitle.Text = inf.FormDisplay;
		EnglishRepeater.ItemsSource = null;
		EnglishRepeater.Visibility = Visibility.Collapsed;
		WikHeader.Visibility = Visibility.Collapsed;
		SakhrHeader.Visibility = Visibility.Collapsed;
		SakhrRepeater.ItemsSource = null;
		SakhrRepeater.Visibility = Visibility.Collapsed;
		string text = ((inf.Note.Length > 0) ? (char.ToUpperInvariant(inf.Note[0]) + inf.Note.Substring(1)) : "Form");
		WordSubtitle.FlowDirection = (Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
		WordSubtitle.Text = text + " of " + Entry.TitleCase(inf.Lemma) + " - " + Loc.Meanings(entry.Meanings.Length);
		MeaningsRepeater.Visibility = Visibility.Visible;
		MeaningsRepeater.ItemsSource = entry.Meanings;
		NotFoundText.Visibility = Visibility.Collapsed;
	}

	private void ShowReverse(ArabicResult rev)
	{
		WordTitle.FlowDirection = FlowDirection.RightToLeft;
		WordTitle.Text = rev.ArabicTerm;
		RenderArabicSections(rev);
	}

	private void ShowReverseInflection(ArabicInflection inf)
	{
		WordTitle.FlowDirection = FlowDirection.RightToLeft;
		WordTitle.Text = inf.FormDisplay;
		RenderArabicSections(inf.Lemma);
		string text = ((inf.Note.Length > 0) ? (char.ToUpperInvariant(inf.Note[0]) + inf.Note.Substring(1)) : "Form");
		WordSubtitle.Text = text + " of " + inf.Lemma.ArabicTerm + ((WordSubtitle.Text.Length > 0) ? (" - " + WordSubtitle.Text) : "");
	}

	private void RenderArabicSections(ArabicResult rev)
	{
		MeaningsRepeater.ItemsSource = null;
		MeaningsRepeater.Visibility = Visibility.Collapsed;
		NotFoundText.Visibility = Visibility.Collapsed;
		List<string> list = new List<string>();
		if (rev.SakhrLemmas.Length != 0)
		{
			SakhrHeader.Visibility = Visibility.Visible;
			SakhrRepeater.Visibility = Visibility.Visible;
			SakhrRepeater.ItemsSource = rev.SakhrLemmas.Select(Entry.TitleCase).ToArray();
			list.Add(Loc.EnglishWords(rev.SakhrLemmas.Length));
		}
		else
		{
			SakhrHeader.Visibility = Visibility.Collapsed;
			SakhrRepeater.Visibility = Visibility.Collapsed;
			SakhrRepeater.ItemsSource = null;
		}
		if (_useWiktionary && rev.FromWiktionary)
		{
			WikHeader.Visibility = Visibility.Visible;
			EnglishRepeater.Visibility = Visibility.Visible;
			EnglishRepeater.ItemsSource = rev.Glosses;
			int n = rev.Glosses.Length;
			list.Add(Loc.MeaningsFromWiktionary(n));
		}
		else
		{
			WikHeader.Visibility = Visibility.Collapsed;
			EnglishRepeater.Visibility = Visibility.Collapsed;
			EnglishRepeater.ItemsSource = null;
		}
		WordSubtitle.FlowDirection = (Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
		WordSubtitle.Text = ((rev.Pos.Length > 0 && list.Count > 0) ? (rev.Pos + " - ") : "") + string.Join(" + ", list);
		if (list.Count == 0)
		{
			NotFoundText.Text = Loc.NotFound;
			NotFoundText.Visibility = Visibility.Visible;
		}
	}

	private void ResetPane()
	{
		WordTitle.FlowDirection = FlowDirection.RightToLeft;
		WordTitle.Text = "قاموس صخر الحديث";
		WordSubtitle.FlowDirection = FlowDirection.LeftToRight;
		WordSubtitle.Text = "Sakhr Dictionary Revive - English ⇄ Arabic";
		HomeOfflineText.Visibility = Visibility.Visible;
		MeaningsRepeater.Visibility = Visibility.Visible;
		MeaningsRepeater.ItemsSource = null;
		EnglishRepeater.ItemsSource = null;
		EnglishRepeater.Visibility = Visibility.Collapsed;
		WikHeader.Visibility = Visibility.Collapsed;
		SakhrHeader.Visibility = Visibility.Collapsed;
		SakhrRepeater.ItemsSource = null;
		SakhrRepeater.Visibility = Visibility.Collapsed;
		NotFoundText.Visibility = Visibility.Collapsed;
	}

}
