namespace MedFarLab.Application.Features.Clinical.Models
{
    public class PatientVM
    {
        public long PersonId { get; set; }
        public long OrganizationId { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string AuditNotes { get; set; } = string.Empty;
    }
}
