using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System;

namespace MedFarLab.Pwa.Pages.Billing
{
    public partial class InvoicesList : ComponentBase
    {
        protected List<InvoiceHistoryDto> Invoices { get; set; } = new();
        protected List<InvoiceHistoryDto> PendingInvoices { get; set; } = new();

        [Inject] private MedFarLab.Pwa.Services.IExportService ExportService { get; set; } = default!;
        [Inject] private MediatR.IMediator Mediator { get; set; } = default!;

        protected override async System.Threading.Tasks.Task OnInitializedAsync()
        {
            var response = await Mediator.Send(new MedFarLab.Application.Features.Billing.Queries.GetAllInvoicesQuery());
            
            if (response != null)
            {
                Invoices = new List<InvoiceHistoryDto>();
                PendingInvoices = new List<InvoiceHistoryDto>();

                foreach (var inv in response)
                {
                    var dto = new InvoiceHistoryDto 
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceNumber = inv.InvoiceNumber,
                        IssuedDate = inv.IssuedDate,
                        PatientName = inv.PatientName,
                        TotalAmount = inv.TotalAmount,
                        Status = inv.Status
                    };

                    if (inv.Status == "Pendiente")
                    {
                        PendingInvoices.Add(dto);
                    }
                    else
                    {
                        Invoices.Add(dto);
                    }
                }
            }
        }

        protected async System.Threading.Tasks.Task ExportToExcelAsync()
        {
            // Combinar pendientes y pagadas, o exportar la vista actual. Por ahora, todas.
            var allInvoices = new List<InvoiceHistoryDto>();
            allInvoices.AddRange(PendingInvoices);
            allInvoices.AddRange(Invoices);

            await ExportService.ExportToCsvAsync(allInvoices, $"Facturas_Export_{DateTime.Now:yyyyMMdd_HHmm}");
        }
    }

    public partial class InvoiceHistoryDto
    {
        public long InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public string? PatientName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
