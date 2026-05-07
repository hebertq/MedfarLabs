using Microsoft.AspNetCore.Components;

namespace MedFarLab.Pwa.Pages.Dashboard
{
    public partial class HomeFarmacia : ComponentBase
    {
        protected bool Cargando { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            Cargando = true;
            await Task.Delay(500); // Mock
            Cargando = false;
        }
    }
}
