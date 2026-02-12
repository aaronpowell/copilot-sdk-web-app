using System.Reflection;
var asm = Assembly.LoadFrom(@"C:\Users\aapowell\.nuget\packages\github.copilot.sdk\0.1.23\lib\net8.0\GitHub.Copilot.SDK.dll");
foreach (var t in asm.GetTypes().Where(t => t.IsPublic && !t.IsNested))
    Console.WriteLine(t.FullName);
