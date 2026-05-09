using Microsoft.AspNetCore.Mvc;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Identity;
using MedfarLabs.Core.Domain.Entities.Identity;
using System.Text.Json.Serialization;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IBranchRepository _branchRepository;

        public AuthController(IUserRepository userRepository, IBranchRepository branchRepository)
        {
            _userRepository = userRepository;
            _branchRepository = branchRepository;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Credenciales incorrectas o usuario inactivo." });
            }

            var config = HttpContext.RequestServices.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            var hashService = HttpContext.RequestServices.GetService<MedfarLabs.Core.Domain.Interfaces.Security.IHashService>();
            if (hashService != null && user.PasswordHash != null)
            {
                var hashToMatch = hashService.GenerateHash(request.Password);
                if (!user.PasswordHash.SequenceEqual(hashToMatch))
                {
                    return Unauthorized(new { message = "Credenciales incorrectas o usuario inactivo." });
                }
            }

            var newSessionGuid = Guid.NewGuid().ToString();
            user.SessionToken = newSessionGuid;

            Console.WriteLine($"[DEBUG-AUTH] Username: {user.Username}, Id: {user.Id}, RowVersion: {user.RowVersion}, OrgId: {user.OrganizationId}");

            await _userRepository.UpdateAsync(user);

            bool isDoctor = await _userRepository.IsDoctorAsync(user.Id);

            var branches = await _branchRepository.GetByOrganizationAsync(user.OrganizationId);
            long primaryBranchId = branches.FirstOrDefault()?.Id ?? user.OrganizationId;

            List<string> roles = isDoctor ? new List<string> { "Admin-Clinical" } : new List<string> { "Admin-Recepcion" };
            List<string> modules = new List<string> { "All", "Appointments", "Clinical", "Patients", "Laboratory", "Pharmacy", "Admin-Billing", "Inventory" };

            var profile = new AuthProfileResponse
            {
                UserId = user.Id,
                OrganizationId = user.OrganizationId,
                BranchId = primaryBranchId,
                Username = user.Username,
                Token = newSessionGuid,
                IsDoctor = isDoctor,
                IsMasterAdmin = user.Username == "masteradmin",
                Roles = roles,
                Modules = modules
            };

            return Ok(MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<AuthProfileResponse>.Success(profile));
        }

        [HttpGet("Doctors")]
        public async Task<IActionResult> GetDoctors([FromQuery] long organizationId)
        {
            // Nota: En producción el organizationId debería salir del contexto del token actual.
            var doctors = await _userRepository.GetDoctorsAsync(organizationId);
            return Ok(doctors);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthProfileResponse
    {
        [JsonPropertyName("userId")]
        public long UserId { get; set; }

        [JsonPropertyName("organizationId")]
        public long OrganizationId { get; set; }

        [JsonPropertyName("branchId")]
        public long BranchId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("isDoctor")]
        public bool IsDoctor { get; set; } = false;

        [JsonPropertyName("isMasterAdmin")]
        public bool IsMasterAdmin { get; set; } = false;

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new();

        [JsonPropertyName("modules")]
        public List<string> Modules { get; set; } = new();

        [JsonPropertyName("organizationName")]
        public string OrganizationName { get; set; } = string.Empty;

        [JsonPropertyName("organizationAddress")]
        public string OrganizationAddress { get; set; } = string.Empty;

        [JsonPropertyName("organizationPhone")]
        public string OrganizationPhone { get; set; } = string.Empty;

        [JsonPropertyName("organizationEmail")]
        public string OrganizationEmail { get; set; } = string.Empty;

        [JsonPropertyName("organizationLogoUrl")]
        public string OrganizationLogoUrl { get; set; } = string.Empty;
    }
}
