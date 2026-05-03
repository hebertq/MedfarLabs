using MedfarLabs.Core.Domain.Common.Attributes;
using MedfarLabs.Core.Domain.Enums;
using MedfarLabs.Core.Domain.Const;

namespace MedFarLab.Application.Features.Reporting.DTOs
{
    [ActionMapping(AppModule.Report, AppAction.Report.BillingInvoice)]
    public class InvoiceReportRequestDTO
    {
        public long InvoiceId { get; set; }
        public bool IsTicketFormat { get; set; }
        // Se cambió a enum a pedido del usuario
        public MedfarLabs.Core.Domain.Enums.InvoiceFormatType? PreferredFormat { get; set; }
        public MedfarLabs.Core.Domain.Enums.InvoiceTemplateStyle? PreferredTemplateName { get; set; }
        public MedfarLabs.Core.Domain.Models.Reporting.OrganizationInfoModel OrganizationInfo { get; set; } = new();
    }
}
