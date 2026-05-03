using System; using System.Reflection; class Program { static void Main() { 
    var asm = Assembly.Load("MedfarLabs.Core.Domain");
    var type = asm.GetType("MedfarLabs.Core.Domain.Interfaces.Security.IUserContext");
    if (type != null) {
        foreach(var p in type.GetProperties()) {
            Console.WriteLine(p.PropertyType.Name + " " + p.Name);
        }
    }
} }
