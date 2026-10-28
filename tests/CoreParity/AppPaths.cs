namespace FluentSakhrDictionary;
// Core test isolates settings writes from a real installed application's data.
public static class App { public static string AppData { get; } = Path.Combine(Path.GetTempPath(),"SakhrCoreTests-"+Guid.NewGuid()); }
