using System.ComponentModel.DataAnnotations;

namespace MedFarLab.Application.Features.Clinical.Models
{
    public class DoctorConfigurationVM
    {
        public long DoctorUserId { get; set; }

        public string? AvailableHours { get; set; }

        [Required(ErrorMessage = "El tiempo mínimo por consulta es obligatorio.")]
        [Range(10, 120, ErrorMessage = "El tiempo debe estar entre 10 y 120 minutos.")]
        public int MinConsultationTimeMins { get; set; } = 15;

        // Flujo Corporativo
        public bool DoctorBillsDirectly { get; set; } = false;
        public bool AllowManualPriceEdit { get; set; } = false;

        // Formato Impresión
        public string PrintFormat { get; set; } = "A4"; // "A4" o "Ticket"
    }
}
