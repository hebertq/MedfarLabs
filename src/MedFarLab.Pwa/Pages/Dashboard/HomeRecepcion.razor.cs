using Microsoft.AspNetCore.Components;
using MedfarLabs.Core.Application.Common.Interfaces;

namespace MedFarLab.Pwa.Pages.Dashboard
{
    public partial class HomeRecepcion : ComponentBase
    {
        [Inject] private IApplicationDispatcher Dispatcher { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        protected string FechaHoy { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        protected decimal TotalEfectivo { get; set; } = 0;
        protected decimal TotalTarjeta { get; set; } = 0;
        protected int FacturasPendientes { get; set; } = 0;
        protected bool Cargando { get; set; } = true;
        protected List<DailyClosingRow> FilasCierre { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Cargando = true;
            await Task.Delay(500); // Mock API call
            Cargando = false;
        }

        protected void IrAFacturacion() => Nav.NavigateTo("/facturacion");
        protected void ImprimirCierre() 
        {
            // Print action
        }
    }

    public class DailyClosingRow
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
