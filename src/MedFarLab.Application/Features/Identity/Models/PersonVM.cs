using System;

namespace MedFarLab.Application.Features.Identity.Models
{
    public class PersonVM
    {
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SecondLastName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int GenderId { get; set; }
        public int BirthCountryId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string MobilePhone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int? GeolocationId { get; set; }
    }
}
