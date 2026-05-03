namespace MedFarLab.Application.Features.Reporting.DTOs
{
    public class ReportResponseDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = "application/pdf";
        public string Base64Data { get; set; } = string.Empty;
    }
}
