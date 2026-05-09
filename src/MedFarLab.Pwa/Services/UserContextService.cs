using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedFarLab.Pwa.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly AuthenticationStateProvider _auth;

        public UserContextService(AuthenticationStateProvider auth) { _auth = auth; }

        private ClaimsPrincipal? _user;
        private async Task<ClaimsPrincipal> GetUserAsync()
        {
            if (_user != null) return _user;
            var state = await _auth.GetAuthenticationStateAsync();
            _user = state.User;
            return _user;
        }

        public long UserId => long.TryParse(GetClaim("nameid") ?? GetClaim(ClaimTypes.NameIdentifier), out var val) ? val : 0;
        public string FullName => GetClaim("name") ?? GetClaim(ClaimTypes.Name) ?? string.Empty;
        public string OrganizationName => GetClaim("org_name") ?? string.Empty;
        public int OrganizationTypeId => int.TryParse(GetClaim("org_type"), out var val) ? val : 1;
        public string OrgTypeName => OrganizationTypeId switch {
            124 => "Clínica", 15 => "Laboratorio", 3 => "Médico Independiente",
            125 => "Farmacia", 126 => "Clínica Odontológica", _ => "Organización"
        };
        public string PrimaryRole => GetClaim("role") ?? GetClaim(ClaimTypes.Role) ?? string.Empty;
        public bool IsAdmin => PrimaryRole.StartsWith("Admin");

        private string? GetClaim(string type)
        {
            // We use synchronous access here for simplicity in properties. 
            // In a real app we might need to await the AuthState.
            // But Blazor WASM usually has the auth state synchronously available after initial load.
            return _auth.GetAuthenticationStateAsync().GetAwaiter().GetResult().User?.FindFirst(type)?.Value;
        }
    }
}
