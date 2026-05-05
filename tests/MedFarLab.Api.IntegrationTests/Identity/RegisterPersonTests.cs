using MedFarLab.Api.IntegrationTests.Common;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using System.Net.Http.Json;
using System.Net;

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MedfarLabs.Core.Domain.Interfaces.Security;

namespace MedFarLab.Api.IntegrationTests.Identity
{
    public class TestAdminUserContext : IUserContext
    {
        public long UserId { get; set; } = 1;
        public long OrganizationId { get; set; } = 1;
        public long BranchId { get; set; }
        public int OrganizationTypeId { get; set; } = 1;

        public Task<bool> HasPermissionAsync(int actionId)
        {
            return Task.FromResult(true);
        }
    }

    [Collection("ApiTests")]
    public class RegisterPersonTests : BaseIntegrationTest
    {
        public RegisterPersonTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task RegisterPerson_WithValidPayload_ShouldReturnSuccess()
        {
            // Arrange: Construimos un cliente especializado para saltar la seguridad solo en esta prueba
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<IUserContext, TestAdminUserContext>();
                });
            }).CreateClient();

            var validPayload = new PersonRequestDTO(
                FirstName: "Integration",
                MiddleName: "",
                LastName: "Test",
                SecondLastName: "Data",
                BirthDate: new DateTime(1990, 5, 20),
                GenderId: 1, 
                BirthCountryId: 1,
                Email: $"integration.test.{Guid.NewGuid().ToString().Substring(0,8)}@medfarlab.com",
                MobilePhone: "0999999999",
                Address: "Test Address",
                GeolocationId: 101
            );
            
            int actionCode = (int)AppAction.Identity.RegistrarPersona;

            // Act: Inyectando la constante de la arquitectura
            var response = await client.PostAsJsonAsync($"api/Identity/{actionCode}", validPayload);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            if (!result.GetProperty("isSuccess").GetBoolean())
            {
                Assert.Fail($"The API returned failure. Response: {result}");
            }
        }
    }
}

