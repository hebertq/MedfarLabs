using System;

namespace MedFarLab.Application.Features.Laboratory.Models
{
    public class ServiceSampleConfigDTO
    {
        public long Id { get; set; }
        public long ServiceId { get; set; }
        public string SampleType { get; set; } = string.Empty;
        public string? ContainerType { get; set; }
        public string? RequiredVolume { get; set; }
        public string? SpecialInstructions { get; set; }
        public bool IsActive { get; set; }
    }
}
