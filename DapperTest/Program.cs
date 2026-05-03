using Dapper;
using Npgsql;
using System;
using System.Data;

public class BaseEntity {
    public int RowVersion { get; set; } = 1;
}

public class User : BaseEntity {
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
}

class Program {
    static void Main() {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=medfarlab;Username=medfarlab;Password=root765*");
        conn.Open();
        var user = conn.QueryFirstOrDefault<User>("SELECT id as Id, username as Username, row_version FROM identity.mst_user WHERE username = 'clinicadmin'");
        Console.WriteLine($"Result => Id: {user.Id}, RowVersion: {user.RowVersion}");
    }
}
