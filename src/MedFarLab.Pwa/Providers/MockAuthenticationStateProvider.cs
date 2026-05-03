using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace MedFarLab.Pwa.Providers
{
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Creamos un Mock de un Doctor Logeado provisorio para que la UI funcione al 100%
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Médico Mock (Desarrollo)"),
                new Claim(ClaimTypes.Role, "Doctor"),
                new Claim(ClaimTypes.Email, "doctor@medfarlabs.com"),
                new Claim("OrganizationId", "1")
            }, "MockAuthType");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
