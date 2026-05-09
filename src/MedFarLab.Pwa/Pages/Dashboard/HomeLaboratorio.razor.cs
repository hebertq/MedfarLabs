using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Common.Interfaces;

namespace MedFarLab.Pwa.Pages.Dashboard
{
    public partial class HomeLaboratorio : ComponentBase
    {

        [Inject] private NavigationManager Nav { get; set; } = null!;

        protected LabDashboardStatsDTO? Stats { get; set; }
        protected int EnProceso { get; set; } = 0;
        protected bool Cargando { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            Cargando = true;
            // Simulated call
            // var result = await Dispatcher.DispatchAsync<LabDashboardStatsDTO>(new GetLabDashboardStatsRequestDTO());
            // if (result.IsSuccess) { Stats = result.Data; }
            await Task.Delay(500); // Mock delay
            Stats = new LabDashboardStatsDTO { PendingQueue = new() }; 
            Cargando = false;
        }

        protected Color GetPriorityColor(string priority) => priority?.ToLower() switch
        {
            "alta" => Color.Error,
            "media" => Color.Warning,
            "baja" => Color.Success,
            _ => Color.Default
        };

        protected void RegistrarResultado(long orderId) => Nav.NavigateTo($"/laboratorio/resultados/nuevo/{orderId}");
        protected void VerCriticos() => Nav.NavigateTo("/laboratorio/resultados?criticos=true");
    }

    public class LabDashboardStatsDTO
    {
        public int PendingExamsToday { get; set; }
        public int CompletedExamsToday { get; set; }
        public int CriticalResults { get; set; }
        public List<LabPendingQueueDTO> PendingQueue { get; set; } = new();
    }

    public class LabPendingQueueDTO
    {
        public long OrderId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
    }
}
