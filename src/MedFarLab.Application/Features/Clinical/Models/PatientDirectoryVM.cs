namespace MedFarLab.Application.Features.Clinical.Models
{
    public class PatientDirectoryVM
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string LastVisit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MainDiagnosis { get; set; } = string.Empty;

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "?";
                var parts = FullName.Split(' ', global::System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
                return "?";
            }
        }
    }
}
