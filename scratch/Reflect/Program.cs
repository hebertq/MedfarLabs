using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"C:\Users\GLOBALPRO\.gemini\antigravity\scratch\MedFarLab\src\MedFarLab.Api\bin\Release\net9.0\Infrastructure.dll");
        var userContextType = asm.GetType("MedfarLabs.Core.Infrastructure.Shared.Security.UserContext");
        if (userContextType != null) {
            foreach(var ctor in userContextType.GetConstructors()) {
                Console.WriteLine("Ctor:");
                foreach(var p in ctor.GetParameters()) {
                    Console.WriteLine(" - " + p.ParameterType.Name);
                }
            }
        }
    }
}
