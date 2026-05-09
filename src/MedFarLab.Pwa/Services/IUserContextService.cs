namespace MedFarLab.Pwa.Services
{
    public interface IUserContextService
    {
        long UserId { get; }
        string FullName { get; }
        string OrganizationName { get; }
        int OrganizationTypeId { get; }
        string OrgTypeName { get; }
        string PrimaryRole { get; }
        bool IsAdmin { get; }

        // Helpers de tipo de org
        bool IsClinica => OrganizationTypeId is 124 or 3 or 126;
        bool IsLaboratorio => OrganizationTypeId == 15;
        bool IsFarmacia => OrganizationTypeId == 125;
    }
}
