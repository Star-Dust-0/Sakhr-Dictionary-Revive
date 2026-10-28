using Mono.Cecil;
using Mono.Cecil.Cil;
var a=AssemblyDefinition.ReadAssembly(args[0]);
a.MainModule.Attributes &= ~ModuleAttributes.ILLibrary;
a.MainModule.Attributes |= ModuleAttributes.ILOnly;
a.MainModule.Architecture=TargetArchitecture.I386;
var mod=a.MainModule.Types.Single(t=>t.Name=="<Module>");
foreach(var m in mod.Methods.ToArray())mod.Methods.Remove(m);
a.Write(args[1]);
