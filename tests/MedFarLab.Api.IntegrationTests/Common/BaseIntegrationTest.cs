using Microsoft.Extensions.DependencyInjection;
using SharedFakers.Infrastructure;
using SharedFakers.Seeders;
using System.Text.Json;

namespace MedFarLab.Api.IntegrationTests.Common
{
    [Collection("ApiTests")]
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;
        protected IServiceProvider _serviceProvider;

        protected BaseIntegrationTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _serviceProvider = factory.Services;
        }

        public virtual async Task InitializeAsync()
        {
            // Truncate todas las tablas de Postgres para tests limpios
            await DbCleaner.TruncateAllTables(_serviceProvider);

            // Re-sembrar data falsa con Bogus (Seeders del Core)
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<MasterSeeder>();
            await seeder.SeedAsync();
            
            // Add custom initialization for derived tests
        }

        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}
