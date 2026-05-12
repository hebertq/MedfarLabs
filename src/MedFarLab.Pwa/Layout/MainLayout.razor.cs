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

        [Inject] private MediatR.IMediator Mediator { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.Services.IUserContextService UserCtx { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.Services.MedFarMenuService MenuService { get; set; } = default!;

        private bool _isMenusLoaded = false;

        protected override async Task OnInitializedAsync()
        {
            await UserCtx.InitializeAsync();
            NavManager.LocationChanged += OnLocationChanged;

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

            if (AppState.UserId > 0)
            {
                await LoadDynamicMenusAsync();
                _isMenusLoaded = true;
                CheckRoute(NavManager.Uri);
            }
        }

        private async Task LoadDynamicMenusAsync()
        {
            if (AppState.IsMasterAdmin) return;

            try
            {
                var storedOrgType = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "medfarlab_orgtype");
                int orgTypeId = string.IsNullOrEmpty(storedOrgType) ? 1 : int.Parse(storedOrgType);

                await MenuService.LoadAsync(orgTypeId, UserCtx.PrimaryRole);

                if (MenuService.NavItems.Any())
                {
                    AppState.DynamicMenus = MenuService.NavItems.ToList();
                    AppState.NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading dynamic menus: " + ex.ToString());
                Snackbar.Add($"Exception: {ex.Message}", Severity.Error, config => { config.RequireInteraction = true; });
            }
        }

        private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            if (!_isMenusLoaded && AppState.UserId > 0) return;
            CheckRoute(e.Location);
        }

        private void CheckRoute(string url)
        {
            var relativeUri = NavManager.ToBaseRelativePath(url).Split('?')[0].Split('#')[0];
            var route = "/" + relativeUri;

            if (AppState.UserId == 0 && route != "/" && !route.StartsWith("/user/login"))
            {
                NavManager.NavigateTo("/", forceLoad: true);
                return;
            }

            if (AppState.UserId > 0 && !AppState.IsRouteAllowed(route))
            {
                Snackbar.Add($"Acceso denegado a la ruta solicitada: {route}", Severity.Warning);
                
                // Si no hay menús cargados (falla en API), no redirigir a /home para evitar loop infinito
                if (AppState.DynamicMenus == null || !AppState.DynamicMenus.Any())
                {
                    // Mantener en una vista vacía o de error
                    return;
                }
                
                NavManager.NavigateTo("/home");
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
