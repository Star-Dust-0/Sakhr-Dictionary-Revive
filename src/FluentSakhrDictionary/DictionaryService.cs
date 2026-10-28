using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluentSakhrDictionary;

public static class DictionaryService
{
	private sealed class RevBucket
	{
		public string Display = "";

		public readonly List<string> Lemmas = new List<string>();
	}

	private sealed class WikBucket
	{
		public string Display = "";

		public string Pos = "";

		public readonly List<string> Glosses = new List<string>();
	}

	private sealed class InfBucket
	{
		public string Lemma = "";

		public string Note = "";
	}

	private static Entry[] _entries = Array.Empty<Entry>();

	private static string[] _words = Array.Empty<string>();

	private static string[] _revKeys = Array.Empty<string>();

	private static RevBucket[] _revBuckets = Array.Empty<RevBucket>();

	private static string[] _wikKeys = Array.Empty<string>();

	private static WikBucket[] _wikBuckets = Array.Empty<WikBucket>();

	private static string[] _infForms = Array.Empty<string>();

	private static string[] _trForms = Array.Empty<string>();

	private static string[][] _trBuckets = Array.Empty<string[]>();

	private static InfBucket[] _infBuckets = Array.Empty<InfBucket>();

	private static readonly Dictionary<string, (string Lemma, string Note)> _infAr = new Dictionary<string, (string, string)>(StringComparer.Ordinal);

	private static readonly object _gate = new object();

	private static Task? _coreTask;

	private static Task? _wikTask;

	public static Task CoreReady
	{
		get
		{
			lock (_gate)
			{
				return _coreTask ?? (_coreTask = Task.Run((Action)LoadCore));
			}
		}
	}

	public static Task WikReady
	{
		get
		{
			lock (_gate)
			{
				return _wikTask ?? (_wikTask = Task.Run((Action)LoadWik));
			}
		}
	}

	public static int Count => _entries.Length;

	public static bool HasArabicText(string text)
	{
		foreach (char c in text)
		{
			if (c >= '\u0600' && c <= 'ۿ')
			{
				return true;
			}
		}
		return false;
	}

	private static void EnsureCore()
	{
		CoreReady.GetAwaiter().GetResult();
	}

	private static void EnsureWik()
	{
		WikReady.GetAwaiter().GetResult();
	}

	private static void LoadCore()
	{
		string path = Path.Combine(AppContext.BaseDirectory, "data", "dictionary.jsonl");
		List<Entry> list = new List<Entry>(65536);
		Dictionary<string, RevBucket> rev = new Dictionary<string, RevBucket>(StringComparer.Ordinal);
		foreach (string item in File.ReadLines(path))
		{
			if (item.Length == 0)
			{
				continue;
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(item);
			JsonElement rootElement = jsonDocument.RootElement;
			string word = rootElement.GetProperty("word").GetString() ?? "";
			List<string> list2 = new List<string>();
			foreach (JsonElement item2 in rootElement.GetProperty("meanings").EnumerateArray())
			{
				string text = item2.GetString();
				if (!string.IsNullOrWhiteSpace(text) && text.Any((char c) => c >= '\u0600' && c <= 'ۿ'))
				{
					list2.Add(text);
				}
			}
			Entry entry = new Entry(word, list2.ToArray());
			list.Add(entry);
			string[] meanings = entry.Meanings;
			foreach (string text2 in meanings)
			{
				IndexArabic(text2, entry.Word);
				string[] array = text2.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
				foreach (string text3 in array)
				{
					if (text3.Length > 1)
					{
						IndexArabic(text3, entry.Word);
					}
				}
			}
		}
		FillEmptyEntries(list);
		list.Sort((Entry a, Entry b) => string.CompareOrdinal(a.Word, b.Word));
		_entries = list.ToArray();
		_words = _entries.Select((Entry e) => e.Word).ToArray();
		_revKeys = rev.Keys.ToArray();
		Array.Sort(_revKeys, (IComparer<string>?)StringComparer.Ordinal);
		_revBuckets = _revKeys.Select((string k) => rev[k]).ToArray();
		Dictionary<string, string[]> tr = new Dictionary<string, string[]>(StringComparer.Ordinal);
		string path2 = Path.Combine(AppContext.BaseDirectory, "data", "translations-en-ar.jsonl");
		if (File.Exists(path2))
		{
			foreach (string item3 in File.ReadLines(path2))
			{
				if (item3.Length == 0)
				{
					continue;
				}
				using JsonDocument jsonDocument2 = JsonDocument.Parse(item3);
				JsonElement rootElement2 = jsonDocument2.RootElement;
				string text4 = rootElement2.GetProperty("word").GetString() ?? "";
				string[] array2 = (from m in rootElement2.GetProperty("meanings").EnumerateArray()
					select m.GetString() ?? "" into m
					where m.Length > 0
					select m).ToArray();
				if (text4.Length > 0 && array2.Length != 0)
				{
					tr[text4] = array2;
				}
			}
		}
		_trForms = tr.Keys.ToArray();
		Array.Sort(_trForms, (IComparer<string>?)StringComparer.Ordinal);
		_trBuckets = _trForms.Select((string k) => tr[k]).ToArray();
		void IndexArabic(string raw, string lemma)
		{
			string text5 = NormalizeArabic(raw);
			if (text5.Length != 0)
			{
				if (!rev.TryGetValue(text5, out RevBucket value))
				{
					value = new RevBucket
					{
						Display = raw.Trim()
					};
					rev[text5] = value;
				}
				if (!value.Lemmas.Contains(lemma))
				{
					value.Lemmas.Add(lemma);
				}
			}
		}
	}

	private static void FillEmptyEntries(List<Entry> entries)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(entries.Count, StringComparer.Ordinal);
		for (int i = 0; i < entries.Count; i++)
		{
			dictionary[entries[i].Word] = i;
		}
		string path = Path.Combine(AppContext.BaseDirectory, "data", "derived-forms.jsonl");
		if (File.Exists(path))
		{
			foreach (string item in File.ReadLines(path))
			{
				if (item.Length == 0)
				{
					continue;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(item);
				JsonElement rootElement = jsonDocument.RootElement;
				string text = rootElement.GetProperty("word").GetString() ?? "";
				string text2 = rootElement.GetProperty("lemma").GetString() ?? "";
				JsonElement value;
				string text3 = (rootElement.TryGetProperty("relation", out value) ? (value.GetString() ?? "") : "");
				if (dictionary.TryGetValue(text, out var value2) && dictionary.TryGetValue(text2, out var value3) && !entries[value2].Found && entries[value3].Found)
				{
					string note = ((text3.Length > 0) ? (char.ToUpperInvariant(text3[0]) + text3.Substring(1)) : "Form") + " of " + Entry.TitleCase(text2);
					entries[value2] = new Entry(text, entries[value3].Meanings)
					{
						Note = note
					};
				}
			}
		}
		string path2 = Path.Combine(AppContext.BaseDirectory, "data", "translations-supplement.jsonl");
		if (!File.Exists(path2))
		{
			return;
		}
		foreach (string item2 in File.ReadLines(path2))
		{
			if (item2.Length == 0)
			{
				continue;
			}
			using JsonDocument jsonDocument2 = JsonDocument.Parse(item2);
			JsonElement rootElement2 = jsonDocument2.RootElement;
			string text4 = rootElement2.GetProperty("word").GetString() ?? "";
			string[] array = (from m in rootElement2.GetProperty("meanings").EnumerateArray()
				select m.GetString() ?? "" into m
				where m.Length > 0
				select m).ToArray();
			if (array.Length != 0 && dictionary.TryGetValue(text4, out var value4) && !entries[value4].Found)
			{
				entries[value4] = new Entry(text4, array)
				{
					Note = "Translated"
				};
			}
		}
	}

	private static void LoadWik()
	{
		Dictionary<string, WikBucket> wik = new Dictionary<string, WikBucket>(StringComparer.Ordinal);
		string path = Path.Combine(AppContext.BaseDirectory, "data", "wiktionary-ar.jsonl");
		if (File.Exists(path))
		{
			foreach (string item2 in File.ReadLines(path))
			{
				if (item2.Length == 0)
				{
					continue;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(item2);
				JsonElement rootElement = jsonDocument.RootElement;
				string text = rootElement.GetProperty("word").GetString() ?? "";
				JsonElement value;
				string text2 = (rootElement.TryGetProperty("pos", out value) ? (value.GetString() ?? "") : "");
				string text3 = NormalizeArabic(text);
				if (text3.Length == 0)
				{
					continue;
				}
				if (!wik.TryGetValue(text3, out WikBucket value2))
				{
					value2 = new WikBucket
					{
						Display = text,
						Pos = text2
					};
					wik[text3] = value2;
				}
				else if (text2.Length > 0 && !value2.Pos.Split(" · ").Contains(text2))
				{
					value2.Pos = ((value2.Pos.Length == 0) ? text2 : (value2.Pos + " · " + text2));
				}
				foreach (JsonElement item3 in rootElement.GetProperty("glosses").EnumerateArray())
				{
					string text4 = item3.GetString();
					if (!string.IsNullOrWhiteSpace(text4) && !value2.Glosses.Contains(text4))
					{
						value2.Glosses.Add(text4);
					}
				}
			}
		}
		_wikKeys = wik.Keys.ToArray();
		Array.Sort(_wikKeys, (IComparer<string>?)StringComparer.Ordinal);
		_wikBuckets = _wikKeys.Select((string k) => wik[k]).ToArray();
		Dictionary<string, InfBucket> inf = new Dictionary<string, InfBucket>(StringComparer.Ordinal);
		string path2 = Path.Combine(AppContext.BaseDirectory, "data", "inflections-en.jsonl");
		if (File.Exists(path2))
		{
			foreach (string item4 in File.ReadLines(path2))
			{
				if (item4.Length == 0)
				{
					continue;
				}
				using JsonDocument jsonDocument2 = JsonDocument.Parse(item4);
				JsonElement rootElement2 = jsonDocument2.RootElement;
				string text5 = rootElement2.GetProperty("form").GetString() ?? "";
				string text6 = rootElement2.GetProperty("lemma").GetString() ?? "";
				JsonElement value3;
				string note = (rootElement2.TryGetProperty("note", out value3) ? (value3.GetString() ?? "") : "");
				if (text5.Length != 0 && text6.Length != 0 && !inf.ContainsKey(text5))
				{
					inf[text5] = new InfBucket
					{
						Lemma = text6,
						Note = note
					};
				}
			}
		}
		_infForms = inf.Keys.ToArray();
		Array.Sort(_infForms, (IComparer<string>?)StringComparer.Ordinal);
		_infBuckets = _infForms.Select((string k) => inf[k]).ToArray();
		string path3 = Path.Combine(AppContext.BaseDirectory, "data", "inflections-ar.jsonl");
		if (!File.Exists(path3))
		{
			return;
		}
		foreach (string item5 in File.ReadLines(path3))
		{
			if (item5.Length == 0)
			{
				continue;
			}
			using JsonDocument jsonDocument3 = JsonDocument.Parse(item5);
			JsonElement rootElement3 = jsonDocument3.RootElement;
			string? s = rootElement3.GetProperty("form").GetString() ?? "";
			string text7 = rootElement3.GetProperty("lemma").GetString() ?? "";
			JsonElement value4;
			string item = (rootElement3.TryGetProperty("note", out value4) ? (value4.GetString() ?? "") : "");
			string text8 = NormalizeArabic(s);
			if (text8.Length != 0 && text7.Length != 0 && !_infAr.ContainsKey(text8))
			{
				_infAr[text8] = (text7, item);
			}
		}
	}

	public static string NormalizeArabic(string s)
	{
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		bool flag = false;
		foreach (char c in s)
		{
			if (char.IsWhiteSpace(c))
			{
				if (stringBuilder.Length > 0)
				{
					flag = true;
				}
				continue;
			}
			char c2;
			switch (c)
			{
			case 'ـ':
			case '\u064b':
			case '\u064c':
			case '\u064d':
			case '\u064e':
			case '\u064f':
			case '\u0650':
			case '\u0651':
			case '\u0652':
			case '\u0670':
				c2 = '\0';
				break;
			case 'آ':
			case 'أ':
			case 'إ':
			case 'ٱ':
				c2 = 'ا';
				break;
			case 'ئ':
			case 'ى':
				c2 = 'ي';
				break;
			case 'ؤ':
				c2 = 'و';
				break;
			default:
				c2 = c;
				break;
			}
			char c3 = c2;
			if (c3 != 0 && c3 >= 'ء' && c3 <= 'ي')
			{
				if (flag)
				{
					stringBuilder.Append(' ');
					flag = false;
				}
				stringBuilder.Append(c3);
			}
		}
		return stringBuilder.ToString();
	}

	public static IReadOnlyList<Entry> Search(string query, int max = 200)
	{
		EnsureCore();
		query = query.Trim().ToUpperInvariant();
		if (query.Length == 0)
		{
			return Array.Empty<Entry>();
		}
		int num = 0;
		int num2 = _words.Length - 1;
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = string.CompareOrdinal(_words[num4], query);
			if (num5 >= 0 && _words[num4].StartsWith(query, StringComparison.Ordinal))
			{
				num3 = num4;
				num2 = num4 - 1;
			}
			else if (num5 < 0)
			{
				num = num4 + 1;
			}
			else
			{
				num2 = num4 - 1;
			}
		}
		if (num3 >= 0)
		{
			List<Entry> list = new List<Entry>();
			for (int i = num3; i < _words.Length && _words[i].StartsWith(query, StringComparison.Ordinal); i++)
			{
				if (list.Count >= max)
				{
					break;
				}
				list.Add(_entries[i]);
			}
			return list;
		}
		List<Entry> list2 = new List<Entry>();
		for (int j = 0; j < _words.Length; j++)
		{
			if (list2.Count >= max)
			{
				break;
			}
			if (_words[j].Contains(query, StringComparison.Ordinal))
			{
				list2.Add(_entries[j]);
			}
		}
		return list2;
	}

	public static IReadOnlyList<ArabicResult> SearchArabic(string query, bool includeWik, int max = 200)
	{
		EnsureCore();
		if (includeWik)
		{
			EnsureWik();
		}
		string text = NormalizeArabic(query);
		if (text.Length == 0)
		{
			return Array.Empty<ArabicResult>();
		}
		SortedSet<string> sortedSet = new SortedSet<string>(StringComparer.Ordinal);
		if (includeWik)
		{
			CollectPrefix(_wikKeys, text, sortedSet, max);
		}
		CollectPrefix(_revKeys, text, sortedSet, max);
		List<ArabicResult> list = new List<ArabicResult>();
		foreach (string item in sortedSet)
		{
			if (list.Count >= max)
			{
				break;
			}
			ArabicResult arabicResult = ArabicByKey(item);
			if ((object)arabicResult != null)
			{
				list.Add(arabicResult);
			}
		}
		return list;
	}

	private static ArabicResult? ArabicByKey(string key)
	{
		string text = "";
		string pos = "";
		string[] glosses = Array.Empty<string>();
		string[] sakhrLemmas = Array.Empty<string>();
		int num = Array.BinarySearch(_wikKeys, key, (IComparer<string>?)StringComparer.Ordinal);
		if (num >= 0)
		{
			WikBucket obj = _wikBuckets[num];
			text = obj.Display;
			pos = obj.Pos;
			glosses = obj.Glosses.ToArray();
		}
		int num2 = Array.BinarySearch(_revKeys, key, (IComparer<string>?)StringComparer.Ordinal);
		if (num2 >= 0)
		{
			sakhrLemmas = _revBuckets[num2].Lemmas.ToArray();
			if (text.Length == 0)
			{
				text = _revBuckets[num2].Display;
			}
		}
		if (num < 0 && num2 < 0)
		{
			return null;
		}
		return new ArabicResult(text, glosses, pos, sakhrLemmas);
	}

	public static ArabicInflection? InflectionOfArabic(string query)
	{
		EnsureWik();
		EnsureCore();
		string text = NormalizeArabic(query);
		if (text.Length == 0 || !_infAr.TryGetValue(text, out (string, string) value))
		{
			return null;
		}
		ArabicResult arabicResult = ArabicByKey(NormalizeArabic(value.Item1));
		if ((object)arabicResult == null)
		{
			return null;
		}
		return new ArabicInflection(query.Trim(), arabicResult, value.Item2);
	}

	private static void CollectPrefix(string[] keys, string q, SortedSet<string> into, int max)
	{
		int num = 0;
		int num2 = keys.Length - 1;
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = string.CompareOrdinal(keys[num4], q);
			if (num5 >= 0 && keys[num4].StartsWith(q, StringComparison.Ordinal))
			{
				num3 = num4;
				num2 = num4 - 1;
			}
			else if (num5 < 0)
			{
				num = num4 + 1;
			}
			else
			{
				num2 = num4 - 1;
			}
		}
		if (num3 < 0)
		{
			return;
		}
		for (int i = num3; i < keys.Length && keys[i].StartsWith(q, StringComparison.Ordinal); i++)
		{
			if (into.Count >= max)
			{
				break;
			}
			into.Add(keys[i]);
		}
	}

	public static Entry? Exact(string word)
	{
		EnsureCore();
		word = word.Trim().ToUpperInvariant();
		int num = Array.BinarySearch(_words, word, (IComparer<string>?)StringComparer.Ordinal);
		if (num < 0)
		{
			return null;
		}
		return _entries[num];
	}

	public static string[]? TranslationsOf(string word)
	{
		EnsureCore();
		word = word.Trim().ToUpperInvariant();
		if (word.Length == 0)
		{
			return null;
		}
		int num = Array.BinarySearch(_trForms, word, (IComparer<string>?)StringComparer.Ordinal);
		if (num >= 0)
		{
			return _trBuckets[num];
		}
		return null;
	}

	public static Inflection? InflectionOf(string query)
	{
		EnsureWik();
		query = query.Trim().ToUpperInvariant();
		if (query.Length == 0)
		{
			return null;
		}
		int num = Array.BinarySearch(_infForms, query, (IComparer<string>?)StringComparer.Ordinal);
		if (num < 0)
		{
			return null;
		}
		InfBucket infBucket = _infBuckets[num];
		return new Inflection(Entry.TitleCase(query), infBucket.Lemma, infBucket.Note);
	}
}
