using System;
using System.ComponentModel.DataAnnotations;

namespace MedFarLab.Application.Features.Identity.Models
{
    public class PatientRegistrationVM
    {
        [Required(ErrorMessage = "El primer nombre es obligatorio.")]
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        public string LastName { get; set; } = string.Empty;

        public string SecondLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        public string DocumentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime? BirthDate { get; set; } = new DateTime(1990, 1, 1);

        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
        public int DepartmentId { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "El municipio es obligatorio.")]
        public int MunicipalityId { get; set; } = 0;

        public int? DistrictId { get; set; } = null;

        [Range(1, int.MaxValue, ErrorMessage = "El país de nacimiento es obligatorio.")]
        public int BirthCountryId { get; set; } = 0;

        [Range(1, 2, ErrorMessage = "Debe clasificar el sexo biológico.")]
        public int GenderId { get; set; } = 0;

        public string BloodType { get; set; } = string.Empty;

        public string Allergies { get; set; } = string.Empty;

        public string ChronicConditions { get; set; } = string.Empty;

        public string EmergencyContact { get; set; } = string.Empty;

        public string EmergencyPhone { get; set; } = string.Empty;
    }
}
