using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedFakers.Seeders;
using Testcontainers.PostgreSql;
using Migrations;

namespace MedFarLab.Api.IntegrationTests.Common
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("medfarlab_test_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            var connString = _dbContainer.GetConnectionString();
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connString);

            // Run migrations automatically on the container
            MigratorHelper.Run(connString);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("EXECUTION_CONTEXT", "Main");
            
            builder.ConfigureTestServices(services =>
            {
                var connString = _dbContainer.GetConnectionString();
                
                var testConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connString,
                        ["SecuritySettings:EncryptionKey"] = "zH+V6r2K5b7D0e9F3c1M8xQ4wP5j8tS2aK9vL1Xy7Z0=",
                        ["SecuritySettings:HashSalt"] = "zH+V6r2K5b7D0e9F3c1M8xQ4wP5j8tS2aK9vL1Xy7Z0="
                    })
                    .Build();

                services.AddSingleton<IConfiguration>(testConfig);
                services.AddScoped<MasterSeeder>();
                
                // MOCK PARA SQS (Evita falla de resolución en QueueOutputAction)
                services.AddScoped<Amazon.SQS.IAmazonSQS>(sp => new Amazon.SQS.AmazonSQSClient("fake", "fake", Amazon.RegionEndpoint.USEast1));
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }
    }
}
