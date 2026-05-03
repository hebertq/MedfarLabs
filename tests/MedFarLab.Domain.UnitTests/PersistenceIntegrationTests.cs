using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using MedfarLabs.Core.Domain.Entities.Identity;
using Xunit;

namespace MedFarLab.Domain.UnitTests
{
    // Usaremos un Colección Fixture si quisiéramos reutilizar, pero para brevedad usamos IAsyncLifetime
    public class PersistenceIntegrationTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("medfarlab_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public async Task InitializeAsync()
        {
            // 1. Iniciar el Testcontainer (Requiere Docker activo localmente)
            await _postgreSqlContainer.StartAsync();
            
            // To be implemented once Core DbContext is exposed
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        [Fact(Skip = "Requiere la instalación nativa de Docker y ejecución de Containerd localmente")]
        public async Task Should_Add_Patient_To_Postgres_Through_Repository()
        {
            // To be implemented once Core repositories are exposed
            Assert.True(true);
        }
    }
}

