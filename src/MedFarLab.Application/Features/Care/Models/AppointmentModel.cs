using System;

namespace MedFarLab.Application.Features.Care.Models
{
    public class AppointmentModel
    {
        public long Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string ConsultationReason { get; set; } = string.Empty;
    }
}
