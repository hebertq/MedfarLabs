using System;
using System.Reflection;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;

class Program {
    static void Main() {
        var type = typeof(InvoiceRequestDTO);
        Console.WriteLine("Properties of " + type.Name + ":");
        foreach(var prop in type.GetProperties()) {
            Console.WriteLine(prop.Name + " (" + prop.PropertyType.Name + ")");
        }
    }
}
