using Microsoft.AspNetCore.Components;
using MediatR;
using MudBlazor;
using MedFarLab.Application.Features.Inventory.Models;
using MedFarLab.Application.Features.Inventory.Queries.GetServiceCatalogQuery;
using MedFarLab.Application.Features.Reporting.DTOs;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using MedfarLabs.Core.Domain.Enums;

namespace MedFarLab.Pwa.Pages.Billing
{
    public partial class InvoiceGenerator : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = default!;

        [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

        [Parameter] public long? InvoiceId { get; set; }

        protected InvoiceGeneratorVM ViewModel { get; set; } = new();
        protected string PatientName { get; set; } = string.Empty;
        protected string CurrentInvoiceStatus { get; set; } = string.Empty;
        protected List<ServiceItemVM> GlobalCatalog { get; set; } = new();
        
        protected bool IsSending { get; set; }
        protected bool AllowManualPriceEdit { get; set; } = true;
        protected string PrintCssClass { get; set; } = "invoice-ticket"; // "invoice-a4" o "invoice-ticket"

        protected override async Task OnInitializedAsync()
        {
            var response = await Mediator.Send(new GetServiceCatalogQuery(AppState.CurrentTenantRoute));
            if (response != null && response.IsSuccess && response.Data != null)
                GlobalCatalog = response.Data;

            if (InvoiceId.HasValue)
            {
                var invoice = await Mediator.Send(new MedFarLab.Application.Features.Billing.Queries.GetInvoiceByIdQuery { InvoiceId = InvoiceId.Value });
                if (invoice != null)
                {
                    ViewModel.PatientId = invoice.PatientId;
                    ViewModel.InvoiceNumber = invoice.InvoiceNumber ?? string.Empty;
                    PatientName = invoice.PatientName ?? string.Empty;
                    CurrentInvoiceStatus = invoice.Status ?? "Pendiente";
                    
                    ViewModel.Items.Clear();
                    foreach(var item in invoice.Items)
                    {
                        var catalogService = GlobalCatalog.FirstOrDefault(x => x.Name == item.Description || x.Code == item.Description);
                        ViewModel.Items.Add(new InvoiceItemVM {
                             SelectedService = catalogService,
                             Description = item.Description ?? string.Empty,
                             UnitPrice = item.UnitPrice,
                             Quantity = item.Quantity
                        });
                    }
                    RecalculateTotals();
                }
            }
            else
            {
                ViewModel.Items.Add(new InvoiceItemVM());
            }
        }
        
        protected void RemoveItem(InvoiceItemVM item)
        {
            if (ViewModel.Items.Count > 1)
            {
                ViewModel.Items.Remove(item);
                RecalculateTotals();
            }
        }

        protected async Task GoBack()
        {
            await JSRuntime.InvokeVoidAsync("history.back");
        }

        protected async Task<IEnumerable<ServiceItemVM>> SearchServices(string value, CancellationToken token)
        {
            await Task.Delay(10, token); // min delay
            if (string.IsNullOrEmpty(value))
                return GlobalCatalog;
                
            return GlobalCatalog.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || 
                                            x.Code.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        protected async Task AddItem()
        {
            await OpenItemModalAsync(null);
        }

        protected async Task EditItem(InvoiceItemVM item)
        {
            await OpenItemModalAsync(item);
        }

        protected async Task OpenItemModalAsync(InvoiceItemVM? itemToEdit)
        {
            bool isNew = itemToEdit == null;
            var clone = isNew ? new InvoiceItemVM() : new InvoiceItemVM
            {
                SelectedService = itemToEdit!.SelectedService,
                Description = itemToEdit.Description,
                Quantity = itemToEdit.Quantity,
                UnitPrice = itemToEdit.UnitPrice
            };

            var parameters = new DialogParameters<InvoiceItemModal>
            {
                { x => x.Model, clone },
                { x => x.GlobalCatalog, GlobalCatalog },
                { x => x.AllowManualPriceEdit, AllowManualPriceEdit }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<InvoiceItemModal>(isNew ? "Añadir Servicio" : "Editar Servicio", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is InvoiceItemVM updatedItem)
            {
                if (isNew)
                {
                    ViewModel.Items.Add(updatedItem);
                }
                else
                {
                    itemToEdit!.SelectedService = updatedItem.SelectedService;
                    itemToEdit.Description = updatedItem.Description;
                    itemToEdit.Quantity = updatedItem.Quantity;
                    itemToEdit.UnitPrice = updatedItem.UnitPrice;
                }
                RecalculateTotals();
                StateHasChanged();
            }
        }

        protected void RecalculateTotals()
        {
            ViewModel.Subtotal = ViewModel.Items.Sum(x => x.Subtotal);
            float taxRate = ViewModel.Items.Any(x => x.SelectedService != null && x.SelectedService.IsTaxable) ? 0.15f : 0.0f;
            ViewModel.Tax = ViewModel.Subtotal * (decimal)taxRate; 
            ViewModel.Total = ViewModel.Subtotal + ViewModel.Tax;
        }

        protected async Task HandleSubmitInvoice()
        {
            IsSending = true;
            StateHasChanged();

            // Mapper: ViewModel -> DTO (Backend Format)
            var requestDto = new MedfarLabs.Core.Application.Features.Billing.Dtos.Request.InvoiceRequestDTO
            {
                PatientId = ViewModel.PatientId,
                Subtotal = ViewModel.Subtotal,
                Tax = ViewModel.Tax,
                Total = ViewModel.Total,
                InvoiceNumber = string.Empty, // El backend autogenera la secuencia real
                AuditNotes = ViewModel.AuditNotes
            };

            var updateDto = new MedfarLabs.Core.Application.Features.Billing.Dtos.Request.UpdateInvoiceRequestDTO
            {
                InvoiceId = InvoiceId ?? 0,
                Subtotal = ViewModel.Subtotal,
                Tax = ViewModel.Tax,
                Total = ViewModel.Total,
                AuditNotes = ViewModel.AuditNotes
            };

            foreach (var item in ViewModel.Items)
            {
                if (!string.IsNullOrEmpty(item.Description) && item.SelectedService != null)
                {
                    var itemDto = new MedfarLabs.Core.Application.Features.Billing.Dtos.Request.InvoiceItemRequestDTO
                    {
                        ServiceId = item.SelectedService.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    requestDto.Items.Add(itemDto);
                    updateDto.Items.Add(itemDto);
                }
            }

            try
            {
                MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<long>? apiResponse;

                if (InvoiceId.HasValue)
                {
                    apiResponse = await Mediator.Send(new MedFarLab.Application.Features.Billing.Commands.UpdateInvoiceCommand 
                       { InvoiceRequest = updateDto });
                }
                else
                {
                    apiResponse = await Mediator.Send(new MedFarLab.Application.Features.Billing.Commands.CreateInvoiceCommand 
                       { InvoiceRequest = requestDto });
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data > 0)
                {
                    Snackbar.Add(InvoiceId.HasValue ? "Factura actualizada exitosamente." : "Factura emitida y registrada en Mayor Contable exitosamente vía API.", Severity.Success);

                    // Redirigir a la grilla de facturas tras guardar exitosamente
                    NavManager.NavigateTo("/billing/invoices");
                }
                else
                {
                    var errorMsg = apiResponse?.Message ?? "Error al registrar factura en el servidor.";
                    var subErrores = apiResponse?.Errors != null && apiResponse.Errors.Any() 
                        ? string.Join(", ", apiResponse.Errors) 
                        : "";
                        
                    Snackbar.Add($"{errorMsg} {subErrores}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Fallo de conexión al API: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsSending = false;
                StateHasChanged();
            }
        }

        protected async Task HandlePayInvoice()
        {
            if (!InvoiceId.HasValue) return;
            
            var result = await DialogService.ShowMessageBox(
                "Confirmar Pago", 
                "¿Estás seguro que deseas registrar el pago de esta factura?", 
                yesText: "Sí", cancelText: "Cancelar");

            if (result == true)
            {
                IsSending = true;
                StateHasChanged();

                try
                {
                    var apiResponse = await Mediator.Send(new MedFarLab.Application.Features.Billing.Commands.PayInvoiceCommand 
                    { 
                        InvoiceId = InvoiceId.Value, 
                        AmountPaid = ViewModel.Total 
                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        Snackbar.Add("Factura pagada exitosamente.", Severity.Success);
                        // Recargar datos o redirigir
                        NavManager.NavigateTo("/billing/invoices");
                    }
                    else
                    {
                        var errorMsg = apiResponse?.Message ?? "Error al registrar el pago.";
                        Snackbar.Add(errorMsg, Severity.Error);
                    }
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Fallo de conexión al API: {ex.Message}", Severity.Error);
                }
                finally
                {
                    IsSending = false;
                    StateHasChanged();
                }
            }
        }

        protected async Task HandleCancelInvoice()
        {
            if (!InvoiceId.HasValue) return;

            var result = await DialogService.ShowMessageBox(
                "Anular Factura", 
                "¿Estás seguro que deseas anular/cancelar esta factura? Esta acción no se puede deshacer.", 
                yesText: "Anular", cancelText: "Cancelar");

            if (result == true)
            {
                IsSending = true;
                StateHasChanged();

                try
                {
                    var apiResponse = await Mediator.Send(new MedFarLab.Application.Features.Billing.Commands.CancelInvoiceCommand 
                    { 
                        InvoiceId = InvoiceId.Value 
                    });

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        Snackbar.Add("Factura anulada exitosamente.", Severity.Success);
                        NavManager.NavigateTo("/billing/invoices");
                    }
                    else
                    {
                        var errorMsg = apiResponse?.Message ?? "Error al anular la factura.";
                        Snackbar.Add(errorMsg, Severity.Error);
                    }
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Fallo de conexión al API: {ex.Message}", Severity.Error);
                }
                finally
                {
                    IsSending = false;
                    StateHasChanged();
                }
            }
        }

        protected async Task OpenPatientSearch()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Shared.PatientSearchDialog>("Buscar Paciente", options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data is MedFarLab.Pwa.Shared.PatientSearchDialog.PatientSearchResultVM patient)
            {
                ViewModel.PatientId = patient.PatientId ?? 0;
                PatientName = $"{patient.FullName} ({patient.RecordId})";
                StateHasChanged();
            }
        }

        protected async Task PrintA4ServerAsync()
        {
            await FetchAndDownloadPdf(false);
        }

        protected async Task PrintTicketServerAsync()
        {
            await FetchAndDownloadPdf(true);
        }

        private async Task FetchAndDownloadPdf(bool isTicket)
        {
            if (!InvoiceId.HasValue) return;
            
            IsSending = true;
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
                catch { /* Ignorar si no existe JSRuntime */ }

                var request = new InvoiceReportRequestDTO 
                { 
                    InvoiceId = InvoiceId.Value, 
                    IsTicketFormat = isTicket,
                    PreferredTemplateName = preferredTemplate
                };

                var response = await Mediator.Send(new MedFarLab.Application.Features.Reporting.Commands.GenerateInvoicePDF.GenerateInvoicePDFCommand(request));

                if (response != null && response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.Base64Data))
                {
                    await JSRuntime.InvokeVoidAsync("window.downloadPdf", response.Data.FileName, response.Data.Base64Data);
                    Snackbar.Add($"Factura {(isTicket ? "Ticket" : "A4")} procesada y descargada.", Severity.Success);
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
                IsSending = false;
            }
        }
    }

    public class InvoiceGeneratorVM
    {
        public long PatientId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string AuditNotes { get; set; } = string.Empty;
        public List<InvoiceItemVM> Items { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    public class InvoiceItemVM
    {
        public ServiceItemVM? SelectedService { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal Subtotal => Quantity * UnitPrice;
    }
}




