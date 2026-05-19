using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;
using MedFarLab.Application.Features.Patient.Queries.GetPatientRecord;
using MedFarLab.Application.Features.Patient.Models;
using MedfarLabs.Core.Domain.Interfaces.Http;
using Microsoft.JSInterop;
using MedFarLab.Pwa.State;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class PatientRecord : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISender Mediator { get; set; } = default!;
    [Inject] private AppState AppState { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IExternalServiceClient ApiClient { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string PatientId { get; set; } = string.Empty;

    protected bool IsLoading { get; set; } = true;

    protected PatientRecordVM? Patient { get; set; }
    
    // Alertas clínicas conectadas al StickyPatientAlerts
    protected List<string> PatientAllergies => Patient?.Allergies ?? new List<string>();
    protected List<string> CriticalRiskAlerts { get; set; } = new();

    protected List<ClinicalHistoryItemVM> Consultas => Patient?.Consultations ?? new List<ClinicalHistoryItemVM>();

    public List<ChartSeries> VitalsSeries = new List<ChartSeries>();
    public string[] VitalsLabels = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        IsLoading = true;
        try
        {
            if (long.TryParse(PatientId, out long pId))
            {
                var query = new GetPatientRecordQuery { PatientId = pId, OrganizationId = AppState.OrganizationId };
                var response = await Mediator.Send(query);
                
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    Patient = response.Data;
                    
                    // Ejemplo de lógica para alertas críticas:
                    CriticalRiskAlerts.Clear();
                    if (Patient.Age > 65) CriticalRiskAlerts.Add("Riesgo por edad avanzada");
                    if (Patient.Antecedents != null)
                    {
                        foreach(var ant in Patient.Antecedents)
                        {
                            if (ant.Description.Contains("Hipertensión", StringComparison.OrdinalIgnoreCase) || 
                                ant.Description.Contains("Diabetes", StringComparison.OrdinalIgnoreCase))
                            {
                                CriticalRiskAlerts.Add(ant.Description);
                            }
                        }
                    }

                    // Setup Chart Data
                    if (Patient.BloodPressureSystolic != null && Patient.BloodPressureDiastolic != null)
                    {
                        VitalsSeries.Clear();
                        VitalsSeries.Add(new ChartSeries { Name = "Sistólica", Data = Patient.BloodPressureSystolic });
                        VitalsSeries.Add(new ChartSeries { Name = "Diastólica", Data = Patient.BloodPressureDiastolic });
                        VitalsLabels = Patient.VitalsLabels ?? Array.Empty<string>();
                    }
                }
            }
        }
        catch { }
        finally 
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void VolverAlDirectorio()
    {
        NavManager.NavigateTo("/patients/directory");
    }

    protected void NuevaConsulta()
    {
        NavManager.NavigateTo($"/care/consultation/new/{PatientId}");
    }

    private async Task AbrirModalAntecedente()
    {
        if (long.TryParse(PatientId, out long pId))
        {
            var parameters = new DialogParameters { ["PatientId"] = pId };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = DialogService.Show<AddAntecedentDialog>("Nuevo Antecedente", parameters, options);
            var result = await dialog.Result;

            if (!result!.Canceled)
            {
                await LoadData(); // Reload data after adding
            }
        }
    }

    protected void EditarPaciente()
    {
        Snackbar.Add("El módulo de edición de expediente se encuentra en desarrollo por el equipo de ingeniería.", Severity.Info);
    }

    protected void VerConsulta(long consultationId)
    {
        NavManager.NavigateTo($"/care/consultation/ver/{consultationId}");
    }

    protected async Task ImprimirExpediente()
    {
        await JSRuntime.InvokeVoidAsync("window.print");
    }
}
