using Microsoft.AspNetCore.Components;
using MediatR;
using MudBlazor;
using MedFarLab.Application.Features.Clinical.Queries.GetClinicalDashboard;
using MedFarLab.Application.Features.Clinical.Models;

namespace MedFarLab.Pwa.Pages.Clinical
{
    public partial class ClinicalDashboard : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected ClinicalDashboardVM Model { get; set; } = new();

        protected bool ShowVitalsModal { get; set; }
        protected bool IsSending { get; set; }
        
        // Temporarily hold Vitals input
        protected VitalsInputModel VitalsModel { get; set; } = new();
        protected long CurrentAppointmentId { get; set; }
        protected long CurrentPatientId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Trigger the MediatR Query logic
            // By default passing branch 1 and today's date
            var query = new GetClinicalDashboardQuery { BranchId = 1, Date = DateTime.Today };
            var response = await Mediator.Send(query);

            if (response != null && response.IsSuccess && response.Data != null)
            {
                Model = response.Data;
            }
            else
            {
                // Fallback initialized models
                Model = new ClinicalDashboardVM();
            }
        }

        protected void OpenVitalsModal(long appointmentId, long patientId)
        {
            CurrentAppointmentId = appointmentId;
            CurrentPatientId = patientId;
            VitalsModel = new VitalsInputModel(); // Reset
            ShowVitalsModal = true;
        }

        protected void CloseVitalsModal()
        {
            ShowVitalsModal = false;
        }

        protected async Task OpenPatientSearch()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Shared.PatientSearchDialog>("Buscar", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var patient = result.Data as MedFarLab.Pwa.Shared.PatientSearchDialog.PatientSearchResultVM;
                if (patient != null)
                {
                    Snackbar.Add($"Paciente Seleccionado: {patient.FullName} ({patient.RecordId})", Severity.Success);
                    NavManager.NavigateTo($"/patients/record/{patient.PatientId}");
                }
            }
        }

        protected async Task OpenPendingInvoices()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Shared.PendingInvoicesDialog>("Facturas", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is MedFarLab.Pwa.Shared.PendingInvoicesDialog.PendingInvoiceVM invoice)
            {
                NavManager.NavigateTo($"/billing/invoice/{invoice.Id}");
            }
        }

        protected async Task HandleSubmitVitals()
        {
            IsSending = true;
            StateHasChanged();

            try 
            {
                var payload = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.VitalSignsRequestDTO
                {
                    PatientId = CurrentPatientId,
                    Systolic = (int)(VitalsModel.Systolic ?? 0),
                    Diastolic = (int)(VitalsModel.Diastolic ?? 0),
                    HeartRate = (int)(VitalsModel.HeartRate ?? 0),
                    Temperature = VitalsModel.Temperature ?? 0m,
                    Weight = VitalsModel.Weight ?? 0m,
                    Height = VitalsModel.Height ?? 0m
                };

                var command = new MedFarLab.Application.Features.Clinical.Commands.RegisterVitals.RegisterVitalsCommand(payload);
                var response = await Mediator.Send(command);

                if (response != null && response.IsSuccess)
                {
                    Snackbar.Add("Signos vitales guardados exitosamente.", Severity.Success);
                    CloseVitalsModal();
                }
                else
                {
                    Snackbar.Add($"Error guardando signos: {response?.Message}", Severity.Error);
                }
            }
            catch(Exception ex)
            {
                Snackbar.Add($"Error de conexión: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsSending = false;
                StateHasChanged();
            }
        }

        public class VitalsInputModel
        {
            public decimal? Systolic { get; set; }
            public decimal? Diastolic { get; set; }
            public decimal? HeartRate { get; set; }
            public decimal? Temperature { get; set; }
            public decimal? Weight { get; set; }
            public decimal? Height { get; set; }
        }
    }
}
