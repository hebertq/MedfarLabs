using System;
using System.ComponentModel.DataAnnotations;

namespace MedFarLab.Application.Features.Care.Models
{
    public class AppointmentVM
    {
        [Required(ErrorMessage = "El paciente es obligatorio.")]
        public long PatientId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime? Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La hora es obligatoria.")]
        public TimeSpan? SelectedTime { get; set; } = TimeSpan.FromHours(9); 

        [StringLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres.")]
        [Required(ErrorMessage = "El motivo detallado es requerido.")]
        public string Reason { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "La prioridad debe estar entre 1 y 5.")]
        public int Priority { get; set; } = 1;

        public long DoctorUserId { get; set; } = 1; // Default to 1, overridden by UI

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Programada;
    }
}
