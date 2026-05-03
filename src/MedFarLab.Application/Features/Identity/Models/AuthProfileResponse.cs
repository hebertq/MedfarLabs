using System.Text.Json.Serialization;

namespace MedFarLab.Application.Features.Identity.Models
{
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
        public bool IsDoctor { get; set; }

        [JsonPropertyName("isMasterAdmin")]
        public bool IsMasterAdmin { get; set; }

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
