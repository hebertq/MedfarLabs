using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;

namespace MedFarLab.Pwa.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        protected MudTheme ModernTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#1A56DB", // Corporate Trust Blue
                Secondary = "#059669", // Emerald Fresh
                Tertiary = "#8B5CF6", // Purple accent
                Background = "#F3F4F6", // Light modern gray
                AppbarBackground = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                Surface = "#FFFFFF",
                Warning = "#F59E0B",
                Error = "#E11D48",
                Success = "#10B981"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "12px",
            },
            Typography = new Typography()
            {
                Default = new Default() { FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" } },
                Button = new Button() { TextTransform = "none" }
            }
        };
        [Inject] private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (AppState.UserId == 0)
            {
                var storedUserId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "medfarlab_userId");
                if (!string.IsNullOrEmpty(storedUserId) && long.TryParse(storedUserId, out var uid))
                {
                    AppState.UserId = uid;
                    AppState.SessionToken = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "medfarlab_token");
                    
                    var orgInfoJson = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "medfarlab_orginfo");
                    if (!string.IsNullOrEmpty(orgInfoJson))
                    {
                        try
                        {
                            var orgInfo = System.Text.Json.JsonSerializer.Deserialize<MedfarLabs.Core.Domain.Models.Reporting.OrganizationInfoModel>(orgInfoJson);
                            if (orgInfo != null) AppState.OrganizationInfo = orgInfo;
                        }
                        catch { }
                    }
                }
            }
        }

        private async Task Logout()
        {
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "medfarlab_token");
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "medfarlab_userId");
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "medfarlab_isMaster");
            AppState.UserId = 0;
            AppState.SessionToken = string.Empty;
            AppState.OrganizationId = 0;
            AppState.IsMasterAdmin = false;
            NavManager.NavigateTo("/", forceLoad: true);
        }
    }
}
