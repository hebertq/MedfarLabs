using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Auth-Token", "123");
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        
        var response = await client.GetAsync("http://localhost:8080/api/Billing/3010");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine("STATUS: " + response.StatusCode);
        Console.WriteLine("CONTENT: " + content);
    }
}

