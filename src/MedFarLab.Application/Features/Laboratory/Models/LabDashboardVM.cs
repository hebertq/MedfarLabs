namespace MedFarLab.Application.Features.Laboratory.Models
{
    public class LabDashboardVM
    {
        public int PendingExamsToday { get; set; } = 0;
        public int CompletedExamsToday { get; set; } = 0;
        public int CriticalResults { get; set; } = 0;
        public List<LabPendingQueueVM> PendingQueue { get; set; } = new();
    }

    public class LabPendingQueueVM
    {
        public long OrderId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendiente";
    }
}
