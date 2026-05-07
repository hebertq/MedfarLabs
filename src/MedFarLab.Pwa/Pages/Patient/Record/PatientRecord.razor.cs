using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class PatientRecord : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;

    [Parameter]
    public string PatientId { get; set; } = string.Empty;

    protected bool IsLoading { get; set; } = true;

    // TODO: Usar el verdadero PatientDTO de la aplicaciÃ³n
    protected PatientMockDTO? Patient { get; set; }
    
    // Alertas clínicas conectadas al StickyPatientAlerts
    protected List<string> PatientAllergies { get; set; } = new();
    protected List<string> CriticalRiskAlerts { get; set; } = new();

    protected List<ConsultaHistorialDTO> Consultas { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        // Simulando carga desde la base de datos
        await Task.Delay(500);

        Patient = new PatientMockDTO
        {
            Id = PatientId,
            FullName = "Paciente de Ejemplo",
            DocumentId = "123456789",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        PatientAllergies = new List<string> { "Penicilina", "Ibuprofeno" };
        CriticalRiskAlerts = new List<string> { "HipertensiÃ³n Severa" };

        Consultas = new List<ConsultaHistorialDTO>
        {
            new ConsultaHistorialDTO { Fecha = DateTime.Now.AddDays(-30), Motivo = "Control de presiÃ³n", Medico = "Dr. Admin" }
        };

        IsLoading = false;
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

    protected void ImprimirExpediente()
    {
        // TODO: Imprimir
    }
}

public class PatientMockDTO
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class ConsultaHistorialDTO
{
    public DateTime Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Medico { get; set; } = string.Empty;
}
