using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

using MedfarLabs.Core.Domain.Const;
using System.Text.Json;

using MediatR;
using MedFarLab.Application.Features.Billing.Queries;
using MedFarLab.Application.Features.Billing.Commands;

namespace MedFarLab.Pwa.Pages.Billing
{
    public partial class DailyClosingReport
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IDialogService _dialogService { get; set; } = default!;

        private bool IsLoading { get; set; } = false;
        private DateTime? _filterDate = DateTime.Today;
        private DateTime? FilterDate
        {
            get => _filterDate;
            set
            {
                if (_filterDate != value)
                {
                    _filterDate = value;
                    _ = LoadDataAsync();
                }
            }
        }

        private List<Models.DailyClosingRow> ClosingRows { get; set; } = new();
        private decimal TotalAmount => ClosingRows.Sum(x => x.TotalAmount);

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (FilterDate == null) return;
            
            IsLoading = true;
            try
            {
                var query = new GetDailyClosingQuery { Date = FilterDate.Value, BranchId = 0 }; // BranchId 0 or context injected
                var response = await Mediator.Send(query);
                
                if (response != null)
                {
                    ClosingRows = response.Select(r => new Models.DailyClosingRow {
                        PaymentMethod = r.payment_method,
                        Count = r.payment_count,
                        TotalAmount = r.total_amount
                    }).ToList();
                }
                else
                {
                    ClosingRows = new();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error al cargar el cierre diario: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        private async Task CloseBillingPeriodAsync()
        {
            bool? result = await _dialogService.ShowMessageBox(
                "Cerrar Periodo de Facturación",
                $"¿Está seguro de que desea cerrar el periodo de facturación hasta {FilterDate?.ToString("dd/MM/yyyy")}? Esta acción no se puede deshacer.",
                yesText: "Sí, Cerrar Periodo", cancelText: "Cancelar");

            if (result != true) return;

            IsLoading = true;
            try
            {
                var command = new CloseBillingPeriodCommand { EndDate = FilterDate.Value };
                var response = await Mediator.Send(command);

                if (response != null)
                {
                    _snackbar.Add($"Periodo cerrado exitosamente. Consultas facturadas: {response.ConsultationsCounted}. Monto: ${response.TotalAmount}", Severity.Success);
                    await LoadDataAsync();
                }
                else
                {
                    _snackbar.Add($"Error al cerrar el periodo.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error inesperado: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetIconForMethod(string method)
        {
            if (string.IsNullOrEmpty(method)) return Icons.Material.Filled.AttachMoney;
            
            method = method.ToLower();
            if (method.Contains("efectivo") || method.Contains("cash")) return Icons.Material.Filled.Money;
            if (method.Contains("tarjeta") || method.Contains("card")) return Icons.Material.Filled.CreditCard;
            if (method.Contains("transferencia") || method.Contains("wire")) return Icons.Material.Filled.AccountBalance;
            if (method.Contains("cheque") || method.Contains("check")) return Icons.Material.Filled.RequestQuote;
            
            return Icons.Material.Filled.AttachMoney;
        }
    }
}

namespace MedFarLab.Pwa.Pages.Billing.Models
{
    public class DailyClosingRow
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class CloseBillingPeriodResponseDTO
    {
        public int InvoicesProcessed { get; set; }
        public decimal TotalAmountClosed { get; set; }
    }
}
