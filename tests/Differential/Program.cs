using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Collections;
using R=FluentSakhrDictionary.DictionaryService;
var dir=Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving += (c,n) => File.Exists(Path.Combine(dir,n.Name+".dll"))? c.LoadFromAssemblyPath(Path.Combine(dir,n.Name+".dll")):null;
var asm=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(dir,"SakhrDictionaryRevive.dll"));
var t=asm.GetType("FluentSakhrDictionary.DictionaryService")!;
await (Task)t.GetProperty("CoreReady")!.GetValue(null)!; await R.CoreReady;
await (Task)t.GetProperty("WikReady")!.GetValue(null)!; await R.WikReady;
object? Plain(object? v) {
 if(v==null || v is string || v is bool || v is int) return v;
 if(v is IEnumerable en)return en.Cast<object>().Select(Plain).ToArray();
 var d=new SortedDictionary<string,object?>();
 foreach(var p in v.GetType().GetProperties().Where(p=>new[]{"Word","Meanings","Note","FormDisplay","Lemma","ArabicTerm","Glosses","Pos","SakhrLemmas","FromWiktionary"}.Contains(p.Name)))d[p.Name]=Plain(p.GetValue(v));
 return d;
}
int checks=0;
void Compare(string method,params object[] args){
 var a=t.GetMethod(method)!.Invoke(null,args); var b=typeof(R).GetMethod(method)!.Invoke(null,args);
 var x=JsonSerializer.Serialize(Plain(a));var y=JsonSerializer.Serialize(Plain(b));
 if(x!=y)throw new Exception($"Mismatch {method} {JsonSerializer.Serialize(args)}\n{x}\n{y}");checks++;
}
var rows=File.ReadLines(Path.Combine(AppContext.BaseDirectory,"data","dictionary.jsonl")).Where(x=>x.Length>0).Select(x=>JsonDocument.Parse(x).RootElement.GetProperty("word").GetString()!).ToArray();
foreach(var word in rows)Compare("Exact",word);
foreach(var word in rows.Where((_,i)=>i%97==0)) {
 Compare("Search",word[..Math.Min(3,word.Length)],17);Compare("TranslationsOf",word);Compare("InflectionOf",word.ToLowerInvariant());
}
foreach(var q in new[]{""," ","قط","كتاب","أَكَلَ إِنْسَان","يكتبون","مدرسة","ZZZZ", "ABANDONS", "cats", "  cat  "}) {
 Compare("NormalizeArabic",q);Compare("SearchArabic",q,false,17);Compare("SearchArabic",q,true,17);Compare("InflectionOfArabic",q);Compare("InflectionOf",q);
}
Console.WriteLine($"PASS {checks} original-IL-vs-reconstructed differential comparisons ({rows.Length} dictionary entries).");
