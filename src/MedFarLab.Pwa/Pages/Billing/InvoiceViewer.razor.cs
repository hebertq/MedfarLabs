using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using MediatR;
using MudBlazor;
using MedFarLab.Application.Features.Reporting.Queries.GetInvoiceReport;
using MedFarLab.Application.Features.Reporting.DTOs;
using MedFarLab.Application.Features.Billing.DTOs;
using MedFarLab.Application.Features.Billing.Queries;
using MedfarLabs.Core.Domain.Enums;
using System.Net.Http.Json;
using System.Net.Http;

namespace MedFarLab.Pwa.Pages.Billing
{
    public partial class InvoiceViewer : ComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;
        
        [Parameter] public long InvoiceId { get; set; }

        protected InvoiceDto ViewModel { get; set; } = new();
        protected string PrintCssClass { get; set; } = "invoice-a4"; 
        protected decimal PatientBalance { get; set; }
        protected IEnumerable<object> Payments { get; set; } = new List<object>();

        protected override async Task OnInitializedAsync()
        {
            var invoice = await Mediator.Send(new GetInvoiceByIdQuery { InvoiceId = InvoiceId });
            if (invoice != null)
            {
                ViewModel = invoice;
                
                // Load patient balance directly from DB via CQRS
                if (ViewModel.PatientId > 0)
                {
                    PatientBalance = await Mediator.Send(new GetPatientBalanceQuery { PatientId = ViewModel.PatientId });
                }
                
                // Load invoice payments
                Payments = await Mediator.Send(new GetInvoicePaymentsQuery { InvoiceId = InvoiceId });
            }
        }

        protected bool IsPrinting { get; set; }

        protected async Task PrintInvoice()
        {
            if (InvoiceId <= 0) return;
            
            IsPrinting = true;
            try
            {
                var preferredTemplate = InvoiceTemplateStyle.Classic;
                try 
                {
                    var savedTemplate = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "DefaultInvoiceTemplate");
                    if (!string.IsNullOrEmpty(savedTemplate) && Enum.TryParse<InvoiceTemplateStyle>(savedTemplate, out var parsedTemplate)) 
                    {
                        preferredTemplate = parsedTemplate;
                    }
                }
                catch { /* Ignorar en caso de no existir jsruntime (ej API prerender) */ }

                var request = new InvoiceReportRequestDTO 
                { 
                    InvoiceId = InvoiceId, 
                    IsTicketFormat = false,
                    PreferredTemplateName = preferredTemplate,
                    OrganizationInfo = AppState.OrganizationInfo
                };

                var response = await Mediator.Send(new MedFarLab.Application.Features.Reporting.Commands.GenerateInvoicePDF.GenerateInvoicePDFCommand(request));

                if (response != null && response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.Base64Data))
                {
                    await JSRuntime.InvokeVoidAsync("window.downloadPdf", response.Data.FileName, response.Data.Base64Data);
                    Snackbar.Add("Factura A4 procesada y descargada exitosamente.", Severity.Success);
                }
                else
                {
                    Snackbar.Add(response?.Message ?? "Error al generar el PDF en el servidor.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Fallo al comunicar con servidor de Reportes: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsPrinting = false;
            }
        }
    }
}
