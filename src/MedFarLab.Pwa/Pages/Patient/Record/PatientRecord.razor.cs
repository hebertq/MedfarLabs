using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;
using MedFarLab.Application.Features.Patient.Queries.GetPatientRecord;
using MedFarLab.Application.Features.Patient.Models;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class PatientRecord : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISender Mediator { get; set; } = default!;
    [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

    [Parameter]
    public string PatientId { get; set; } = string.Empty;

    protected bool IsLoading { get; set; } = true;

    protected PatientRecordVM? Patient { get; set; }
    
    // Alertas clínicas conectadas al StickyPatientAlerts
    protected List<string> PatientAllergies => Patient?.Allergies ?? new List<string>();
    protected List<string> CriticalRiskAlerts { get; set; } = new();

    protected List<ClinicalHistoryItemVM> Consultas => Patient?.Consultations ?? new List<ClinicalHistoryItemVM>();

    protected override async Task OnInitializedAsync()
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
                }
            }
        }
        catch { }
        finally 
        {
            IsLoading = false;
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

    protected void EditarPaciente()
    {
        NavManager.NavigateTo($"/patients/edit/{PatientId}");
    }

    protected void VerConsulta(long consultationId)
    {
        NavManager.NavigateTo($"/care/consultation/ver/{consultationId}");
    }

    protected void ImprimirExpediente()
    {
        // TODO: Imprimir
    }
}
