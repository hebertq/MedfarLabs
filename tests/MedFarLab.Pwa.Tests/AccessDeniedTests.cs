using Bunit;
using Xunit;
using MedFarLab.Pwa.Pages.Common.Errors;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;

namespace MedFarLab.Pwa.Tests
{
    public class AccessDeniedTests : IDisposable
    {
        private Bunit.TestContext _ctx;

        public AccessDeniedTests()
        {
            _ctx = new Bunit.TestContext();
            _ctx.Services.AddMudServices();
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void AccessDenied_ShouldRender_CorrectMessageAndIcon()
        {
            // Act
            var cut = _ctx.RenderComponent<AccessDenied>();

            // Assert
            var markup = cut.Markup;
            
            // Verificamos elementos del texto renderizado
            Assert.Contains("Acceso Restringido", markup);
            Assert.Contains("No cuentas con los privilegios clínicos o administrativos necesarios", markup);
            
            // Verificamos que tenga el enlace correcto para volver
            Assert.Contains("href=\"/clinical/dashboard\"", markup);
        }
    }
}
