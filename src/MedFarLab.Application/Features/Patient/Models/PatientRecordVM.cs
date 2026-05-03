namespace MedFarLab.Application.Features.Patient.Models
{
    public class PatientRecordVM
    {
        public long PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        
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

        public List<string> Allergies { get; set; } = new();
        public List<AntecedentVM> Antecedents { get; set; } = new();

        public List<ClinicalHistoryItemVM> Consultations { get; set; } = new();
        public List<ActivePrescriptionVM> ActivePrescriptions { get; set; } = new();
        public List<PatientConsentVM> Consents { get; set; } = new();
        
        // Mock data for Vitals Chart
        public double[] BloodPressureSystolic { get; set; } = Array.Empty<double>();
        public double[] BloodPressureDiastolic { get; set; } = Array.Empty<double>();
        public string[] VitalsLabels { get; set; } = Array.Empty<string>();
    }

    public class ClinicalHistoryItemVM
    {
        public long ConsultationId { get; set; }
        public long DoctorUserId { get; set; }
        public int StatusId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
    }

    public class ActivePrescriptionVM
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string IconColor { get; set; } = "Success";
    }

    public class AntecedentVM
    {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PatientConsentVM
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string DigitalFormUrl { get; set; } = string.Empty;
        public DateTime SignedAt { get; set; }
    }
}
