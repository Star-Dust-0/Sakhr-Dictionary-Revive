using System.IO;
using System.Text.Json;

namespace FluentSakhrDictionary;

public class Settings
{
	public string Theme { get; set; } = "Default";

	public string Backdrop { get; set; } = "Mica";

	public string Accent { get; set; } = "";

	public bool UseWiktionary { get; set; }

	public string Language { get; set; } = "system";

	private static string FilePath => Path.Combine(App.AppData, "settings.json");

	public static Settings Load()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
			}
		}
		catch
		{
		}
		return new Settings();
	}

	public void Save()
	{
		try
		{
			Directory.CreateDirectory(App.AppData);
			File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
		}
		catch
		{
		}
	}
}
