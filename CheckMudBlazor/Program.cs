using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"C:\Users\GLOBALPRO\.nuget\packages_temp\mudblazor\7.15.0\lib\net8.0\MudBlazor.dll");
        var type = asm.GetTypes().First(t => t.Name == "Typography");
        var prop1 = type.GetProperty("Default");
        var prop2 = type.GetProperty("Button");
        Console.WriteLine(prop1.PropertyType.Name);
        Console.WriteLine(prop2.PropertyType.Name);
    }
}
