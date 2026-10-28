using System.Globalization;

namespace FluentSakhrDictionary;

public static class Loc
{
	public static bool IsArabic { get; private set; }

	public static string NotFound => T("Not found.", "غير موجود.");

	public static void Apply(Settings s)
	{
		string language = s.Language;
		bool isArabic = language == "ar" || (!(language == "en") && CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar");
		IsArabic = isArabic;
	}

	public static string T(string en, string ar)
	{
		if (!IsArabic)
		{
			return en;
		}
		return ar;
	}

	public static string Meanings(int n)
	{
		if (!IsArabic)
		{
			if (n != 1)
			{
				return n + " meanings";
			}
			return "1 meaning";
		}
		if (n != 1)
		{
			return n + " معنى";
		}
		return "معنى واحد";
	}

	public static string EnglishWords(int n)
	{
		if (!IsArabic)
		{
			if (n != 1)
			{
				return n + " English words";
			}
			return "1 English word";
		}
		if (n != 1)
		{
			return n + " كلمات إنجليزية";
		}
		return "كلمة إنجليزية واحدة";
	}

	public static string EntryNote(string note)
	{
		if (!(note == "Translated"))
		{
			return note;
		}
		if (!IsArabic)
		{
			return "Added translation";
		}
		return "ترجمة مضافة";
	}

	public static string MeaningsFromWiktionary(int n)
	{
		if (!IsArabic)
		{
			if (n != 1)
			{
				return n + " meanings from Wiktionary";
			}
			return "1 meaning from Wiktionary";
		}
		if (n != 1)
		{
			return n + " معاني من Wiktionary";
		}
		return "معنى واحد من Wiktionary";
	}
}
