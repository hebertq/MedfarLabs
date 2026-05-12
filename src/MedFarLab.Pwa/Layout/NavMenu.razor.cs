using Microsoft.AspNetCore.Components;
using System;

namespace MedFarLab.Pwa.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Parameter] public bool IsMobile { get; set; } = false;
        
        [Inject]
        public MedFarLab.Pwa.Services.MedFarMenuService MenuService { get; set; } = default!;

        protected override void OnInitialized()
        {
            AppState.OnChange += StateHasChanged;
        }

        private string GetMudIcon(string? iconName)
        {
            return MedFarLab.Pwa.Services.MedFarIconService.Resolve(iconName);
        }

        public void Dispose()
        {
            AppState.OnChange -= StateHasChanged;
        }
    }
}
