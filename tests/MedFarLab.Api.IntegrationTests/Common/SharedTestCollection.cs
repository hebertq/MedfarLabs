using Xunit;

namespace MedFarLab.Api.IntegrationTests.Common
{
    [CollectionDefinition("ApiTests", DisableParallelization = true)]
    public class SharedTestCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
        // Esta clase es un marcador para que xUnit no corra en paralelo los tests
        // que mutan la misma base de datos en memoria (Testcontainters)
    }
}
