using System;
using Microsoft.UI.Xaml;

namespace FluentSakhrDictionary;

public record Entry(string Word, string[] Meanings)
{
	public string Note { get; init; } = "";

	public bool Found => Meanings.Length != 0;

	public string DisplayWord => TitleCase(Word);

	public HorizontalAlignment ItemHorizontalAlignment
	{
		get
		{
			if (!DictionaryService.HasArabicText(DisplayWord))
			{
				return HorizontalAlignment.Left;
			}
			return HorizontalAlignment.Right;
		}
	}

	public FlowDirection ItemFlow
	{
		get
		{
			if (!DictionaryService.HasArabicText(DisplayWord))
			{
				return FlowDirection.LeftToRight;
			}
			return FlowDirection.RightToLeft;
		}
	}

	public static string TitleCase(string w)
	{
		string[] array = w.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((array[i].Length == 1) ? array[i].ToUpperInvariant() : (char.ToUpperInvariant(array[i][0]) + array[i].Substring(1).ToLowerInvariant()));
		}
		return string.Join(' ', array);
	}
}
