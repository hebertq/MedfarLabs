namespace MedFarLab.Pwa.State;

public class AppState
{
    // Mantiene en memoria el tipo de empresa que inició sesión.
    // Posibles: "clinical", "laboratory", "pharmacy"
    public string CurrentTenantRoute { get; set; } = "clinical"; 

    // Sesión
    public bool IsAuthenticated => !string.IsNullOrEmpty(SessionToken);
    public long UserId { get; set; } = 0;
    public long OrganizationId { get; set; } = 0;
    public long BranchId { get; set; } = 0;
    public string SessionToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsDoctor { get; set; } = false;
        
    public bool IsMasterAdmin { get; set; } = false;
    public List<string> UserRoles { get; set; } = new();
    public List<string> PlanModules { get; set; } = new();

    public MedfarLabs.Core.Domain.Models.Reporting.OrganizationInfoModel OrganizationInfo { get; set; } = new();

    // Cache dynamic menus
    public IEnumerable<MedfarLabs.Core.Application.Features.System.Dtos.NavigationMenuResponseDTO>? DynamicMenus { get; set; }

    public bool HasRole(string role) => UserRoles.Contains("All") || UserRoles.Contains(role);
    public bool HasModule(string module) => PlanModules.Contains("All") || PlanModules.Contains(module);

    public bool IsRouteAllowed(string route)
    {
        if (IsMasterAdmin) return true;
        
        var normalizedRoute = route.ToLower().TrimEnd('/');
        
        // Whitelisted routes
        if (string.IsNullOrEmpty(normalizedRoute) || 
            normalizedRoute == "/" || 
            normalizedRoute == "/home" || 
            normalizedRoute.StartsWith("/user/profile") || 
            normalizedRoute.StartsWith("/settings/"))
            return true;

        if (DynamicMenus == null || !DynamicMenus.Any()) return false;

        foreach (var menu in DynamicMenus)
        {
            if (!string.IsNullOrEmpty(menu.Route) && menu.Route.ToLower().TrimEnd('/') == normalizedRoute)
                return true;
                
            if (menu.SubMenus != null)
            {
                if (menu.SubMenus.Any(sub => !string.IsNullOrEmpty(sub.Route) && sub.Route.ToLower().TrimEnd('/') == normalizedRoute))
                    return true;
            }
        }

        return false;
    }

    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();
}
