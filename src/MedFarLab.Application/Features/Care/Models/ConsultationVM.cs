using System.ComponentModel.DataAnnotations;

namespace MedFarLab.Application.Features.Care.Models
{
    public class ConsultationVM
    {
        [Required(ErrorMessage = "El expediente es obligatorio.")]
        public long MedicalRecordId { get; set; }

        [Required(ErrorMessage = "El médico es obligatorio.")]
        public long DoctorUserId { get; set; }

        [Required(ErrorMessage = "Los datos subjetivos (motivo) son obligatorios.")]
        public string Subjective { get; set; } = string.Empty;

        public string Objective { get; set; } = string.Empty;

        [Required(ErrorMessage = "El análisis médico es obligatorio.")]
        public string Analysis { get; set; } = string.Empty;

        public string Plan { get; set; } = string.Empty;
    }
}
