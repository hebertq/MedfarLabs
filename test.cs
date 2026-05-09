using System;
using System.Linq;
using System.Reflection;

public interface IBaseRepository<T> {}
public class User {}
public interface IUserRepository : IBaseRepository<User> {}

class Program
{
    static void Main()
    {
        var type = typeof(IUserRepository);
        var interfaces = type.GetInterfaces();
        bool inherits = interfaces.Any(i => i.Name == "IBaseRepository`1");
        Console.WriteLine(inherits);
    }
}
