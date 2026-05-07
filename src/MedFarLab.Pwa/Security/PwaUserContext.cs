using MedfarLabs.Core.Domain.Interfaces.Security;

namespace MedFarLab.Pwa.Security
{
    public class PwaUserContext : IUserContext
    {
        public long UserId { get; set; } = 1;
        public long OrganizationId { get; set; } = 1;
        public long BranchId { get; set; } = 1;
        public int OrganizationTypeId { get; set; } = 124; // Clínica

        public Task<bool> HasPermissionAsync(int actionId)
        {
            // El PWA confía en el backend para la autorización real, así que en modo local siempre devuelve true.
            return Task.FromResult(true);
        }
    }
}
