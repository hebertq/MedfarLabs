namespace MedFarLab.Application.Features.Clinical.Models
{
    public class ClinicalDashboardVM
    {
        public int TotalAppointmentsToday { get; set; } = 0;
        public int PatientsWaiting { get; set; } = 0;
        public int LabResultsReady { get; set; } = 0;
        public List<PatientQueueVM> PatientQueue { get; set; } = new();
    }

    public class PatientQueueVM
    {
        public long AppointmentId { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string ConsultationReason { get; set; } = string.Empty;
    }
}
