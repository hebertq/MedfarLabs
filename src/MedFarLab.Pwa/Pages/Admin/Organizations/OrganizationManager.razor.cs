using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Pwa.State;

namespace MedFarLab.Pwa.Pages.Admin.Organizations;

public partial class OrganizationManager : ComponentBase
{
    [Inject] protected AppState AppState { get; set; } = default!;
    [Inject] protected NavigationManager NavManager { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected List<OrganizationDto> mockOrgs = new();

    protected override void OnInitialized()
    {
        // Generación de Datos Mocks (Efecto Wow para el Directorio de Organizaciones)
        mockOrgs = new List<OrganizationDto>
        {
            new() { Id = 1, Name = "MedfarLabs Corporation", IdentifierCode = "ML-0000", PlanName = "Master", Modules = new List<string> { "All" }, IsActive = true },
            new() { Id = 2, Name = "Clínica San Lucas", IdentifierCode = "1792348123001", PlanName = "Premium Care", Modules = new List<string> { "Clinical", "Appointments", "Billing" }, IsActive = true },
            new() { Id = 3, Name = "Lab BioTest Express", IdentifierCode = "1799981223001", PlanName = "Basic LIS", Modules = new List<string> { "Laboratory", "Billing" }, IsActive = true },
            new() { Id = 4, Name = "Consultorio Dr. Mendoza", IdentifierCode = "1701122334001", PlanName = "Starter Med", Modules = new List<string> { "Clinical", "Appointments" }, IsActive = false }
        };
    }

    protected Color GetModuleColor(string module) => module switch
    {
        "Clinical" => Color.Primary,
        "Appointments" => Color.Warning,
        "Laboratory" => Color.Info,
        "Pharmacy" => Color.Success,
        "Billing" => Color.Error,
        "All" => Color.Dark,
        _ => Color.Default
    };

    protected void ToggleStatus(OrganizationDto org)
    {
        org.IsActive = !org.IsActive;
        Snackbar.Add($"Organización {org.Name} ahora está {(org.IsActive ? "Activa" : "Suspendida")}.", org.IsActive ? Severity.Success : Severity.Warning);
    }

    protected async Task OpenProfileDialog(OrganizationDto org)
    {
        var parameters = new DialogParameters<OrganizationEditorDialog>
        {
            { "OrgToEdit", new OrganizationDto 
                { 
                    Id = org.Id, Name = org.Name, IdentifierCode = org.IdentifierCode, 
                    PlanName = org.PlanName, IsActive = org.IsActive, Modules = org.Modules, LogoUrl = org.LogoUrl
                } 
            }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<OrganizationEditorDialog>("Editar Perfil Corporativo", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is OrganizationDto)
        {
            var updated = (OrganizationDto)result.Data;
            org.Name = updated.Name;
            org.IdentifierCode = updated.IdentifierCode;
            org.PlanName = updated.PlanName;
            org.IsActive = updated.IsActive;
            
            StateHasChanged();
            Snackbar.Add("Perfil Corporativo actualizado con éxito.", Severity.Success);
        }
    }

    protected async Task TogglePermissions(OrganizationDto org)
    {
        bool? result = await DialogService.ShowMessageBox(
            "Permisos Granulares Básicos",
            $"Se solicitará al API backend inyectar el módulo Pharmacy a {org.Name} temporalmente para pruebas.",
            yesText: "Inyectar", cancelText: "Cancelar");

        if (result == true)
        {
            if (!org.Modules.Contains("Pharmacy"))
            {
                org.Modules.Add("Pharmacy");
                Snackbar.Add("Feature Flag activado con éxito. (Módulo Farmacia)", Severity.Success);
                StateHasChanged();
            }
        }
    }

    public class OrganizationDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string IdentifierCode { get; set; } = "";
        public string PlanName { get; set; } = "";
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public List<string> Modules { get; set; } = new();
    }
}

