using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Care.Dtos.Response;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Application.Common.Interfaces;

namespace MedFarLab.Pwa.Pages.Dashboard
{
    public partial class HomeMedico : ComponentBase
    {

        [Inject] private NavigationManager Nav { get; set; } = null!;

        protected ClinicalDashboardStatsDTO? Stats { get; set; }
        protected string NombreMedico { get; set; } = string.Empty;
        protected int ConsultasMes { get; set; } = 0;
        protected bool Cargando { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            Cargando = true;
            await Task.Delay(500); // Simulated network delay
            /*
            var result = await Dispatcher.DispatchAsync<ClinicalDashboardStatsDTO>(
                new GetDashboardStatsRequestDTO());

            if (result.IsSuccess)
            {
                Stats = result.Data;
            }
            */
            Stats = new ClinicalDashboardStatsDTO { PatientQueue = new() };
            Cargando = false;
        }

        protected Color GetStatusColor(int statusId) => statusId switch
        {
            1 => Color.Warning,   // En espera
            2 => Color.Success,   // En consulta
            3 => Color.Default,   // Completado
            _ => Color.Default
        };

        protected void IrACitas()       => Nav.NavigateTo("/citas");
        protected void IrAListaEspera() => Nav.NavigateTo("/citas?estado=espera");
        protected void IrAResultados()  => Nav.NavigateTo("/laboratorio/resultados?listos=true");
        protected void BuscarPaciente() => Nav.NavigateTo("/pacientes");
        protected void NuevaCita()      => Nav.NavigateTo("/citas/nueva");
        protected void NuevaReceta()    => Nav.NavigateTo("/recetas/nueva");
        protected void NuevaOrdenLab()  => Nav.NavigateTo("/laboratorio/ordenes/nueva");

        protected void IniciarConsulta(long patientId, long appointmentId) =>
            Nav.NavigateTo($"/consultas/nueva?paciente={patientId}&cita={appointmentId}");
    }
}
