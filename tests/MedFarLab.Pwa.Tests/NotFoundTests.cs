using Bunit;
using Xunit;
using MedFarLab.Pwa.Pages.Common.Errors;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;

namespace MedFarLab.Pwa.Tests
{
    public class NotFoundTests : IDisposable
    {
        private Bunit.TestContext _ctx;

        public NotFoundTests()
        {
            _ctx = new Bunit.TestContext();
            _ctx.Services.AddMudServices();
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void NotFound_ShouldRender_404Message()
        {
            // Act
            var cut = _ctx.RenderComponent<NotFound>();

            // Assert
            var markup = cut.Markup;
            
            Assert.Contains("404", markup);
            Assert.Contains("Documento Clínico No Encontrado", markup);
            Assert.Contains("La ruta que intentas buscar no existe", markup);
            
            // Verifica que el MudButton tenga al menos la acción de regresar
            Assert.Contains("Regresar al Tablero", markup);
            Assert.Contains("href=\"/clinical/dashboard\"", markup);
        }
    }
}
