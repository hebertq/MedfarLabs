using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@""C:\Users\GLOBALPRO\.nuget\packages_temp\mudblazor\7.15.0\lib\net8.0\MudBlazor.dll"");
        var types = asm.GetTypes().Where(t => t.Name.Contains(""MudDialogInstance""));
        foreach(var t in types) {
            Console.WriteLine(t.FullName);
        }
    }
}
