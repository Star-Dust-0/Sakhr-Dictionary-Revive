using Microsoft.UI.Xaml;

namespace FluentSakhrDictionary;

public record ArabicResult(string ArabicTerm, string[] Glosses, string Pos, string[] SakhrLemmas)
{
	public string DisplayWord => ArabicTerm;

	public HorizontalAlignment ItemHorizontalAlignment => HorizontalAlignment.Right;

	public FlowDirection ItemFlow => FlowDirection.RightToLeft;

	public bool FromWiktionary => Glosses.Length != 0;
}
