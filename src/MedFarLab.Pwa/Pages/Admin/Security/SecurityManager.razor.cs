using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Pwa.Services;

namespace MedFarLab.Pwa.Pages.Admin.Security;

public partial class SecurityManager : ComponentBase
{
    [Inject] protected NavigationManager NavManager { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected MedFarSnackbarService SnackbarService { get; set; } = default!;
    
    protected bool IsLoading { get; set; } = false;

    protected List<MockGroup> mockGroups = new() {
        new MockGroup { Id = 1, Name = "Director Médico", Alias = "GRP_DIR_MEDICO" },
        new MockGroup { Id = 2, Name = "Médico General", Alias = "GRP_DOCTOR" },
        new MockGroup { Id = 3, Name = "Químico LIMS", Alias = "GRP_QFB_LIMS" },
    };

    protected List<MockRole> mockRoles = new() {
        new MockRole { Id = 10, Name = "Leer Expediente" },
        new MockRole { Id = 11, Name = "Escribir Receta" },
        new MockRole { Id = 12, Name = "Aprobar Orden Lab" },
    };

    protected List<MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem> mockActions = new() {
        new MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem { Id = 100, ModuleId = 5, Name = "AppAction.Care.CrearReceta", Description = "Acción interna para Dispatcher de Recetas" },
        new MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem { Id = 101, ModuleId = 4, Name = "AppAction.Clinical.ActualizarDirectorio", Description = "Grabar pacientes en PG" },
    };

    public class MockGroup { public int Id { get; set; } public string Name { get; set; } = ""; public string Alias { get; set; } = ""; }
    public class MockRole { public int Id { get; set; } public string Name { get; set; } = ""; }

    protected string GetModuleName(int id)
    {
        return id switch {
            1 => "Security", 2 => "Identity", 3 => "Billing",
            4 => "Clinical", 5 => "Care", 6 => "Common",
            7 => "Inventory", 8 => "Laboratory", _ => "General"
        };
    }

    protected bool FilterGroup(MockGroup item, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        if (item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (item.Alias.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    protected bool FilterRole(MockRole item, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        if (item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    protected bool FilterAction(MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem item, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        if (item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (item.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    // --- GRUPOS ---
    protected async Task OpenAddGroupModal()
    {
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.RoleGroupDialog>("Crear Grupo");
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            mockGroups.Add(new MockGroup { Id = mockGroups.Max(x => x.Id) + 1, Name = data.Name, Alias = data.Alias });
            SnackbarService.ShowSuccess("Grupo creado");
            StateHasChanged();
        }
    }

    protected async Task OpenEditGroupModal(MockGroup grp)
    {
        var parameters = new DialogParameters { ["Name"] = grp.Name, ["Alias"] = grp.Alias };
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.RoleGroupDialog>("Editar Grupo", parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            grp.Name = data.Name;
            grp.Alias = data.Alias;
            SnackbarService.ShowSuccess("Grupo actualizado");
            StateHasChanged();
        }
    }

    protected async Task DeleteGroup(MockGroup grp)
    {
        bool? result = await DialogService.ShowMessageBox("Confirmar", $"Borrar grupo {grp.Name}?", yesText: "Borrar", cancelText: "Cancelar");
        if (result == true)
        {
            mockGroups.Remove(grp);
            SnackbarService.ShowError("Grupo removido");
            StateHasChanged();
        }
    }

    // --- ROLES ---
    protected async Task OpenAddRoleModal()
    {
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.RoleDialog>("Crear Rol");
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            mockRoles.Add(new MockRole { Id = mockRoles.Max(x => x.Id) + 1, Name = data.Name });
            SnackbarService.ShowSuccess("Rol creado");
            StateHasChanged();
        }
    }

    protected async Task OpenEditRoleModal(MockRole rol)
    {
        var parameters = new DialogParameters { ["Name"] = rol.Name };
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.RoleDialog>("Editar Rol", parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            rol.Name = data.Name;
            SnackbarService.ShowSuccess("Rol actualizado");
            StateHasChanged();
        }
    }

    protected async Task DeleteRole(MockRole rol)
    {
        bool? result = await DialogService.ShowMessageBox("Confirmar", $"Borrar rol {rol.Name}?", yesText: "Borrar", cancelText: "Cancelar");
        if (result == true)
        {
            mockRoles.Remove(rol);
            SnackbarService.ShowError("Rol removido");
            StateHasChanged();
        }
    }

    protected async Task OpenAssignActionsModal(MockRole rol)
    {
        // En una app real, traeríamos los Permisos actuales del rol (SelectedActionIds) desde API
        var currentRoleActionIds = new List<int> { 100 }; // mock data: rol tiene permisos en Care.CrearReceta
        var parameters = new DialogParameters { ["RoleId"] = rol.Id, ["RoleName"] = rol.Name, ["MockActions"] = mockActions, ["SelectedActionIds"] = currentRoleActionIds };
        
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog>("Configurador de Transacciones", parameters, new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            // El resultado es List<int> con la nueva selección de ActionIds
            SnackbarService.ShowSuccess($"Matriz de seguridad actualizada para el Rol: {rol.Name}");
        }
    }

    // --- ACCIONES (PERMISSIONS) ---
    protected async Task OpenAddActionModal()
    {
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.ActionPermissionDialog>("Crear Transacción Segura");
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            mockActions.Add(new MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem { Id = mockActions.Max(x => x.Id) + 1, Name = data.Name, ModuleId = data.ModuleId, Description = data.Description });
            SnackbarService.ShowSuccess("Acción creada");
            StateHasChanged();
        }
    }

    protected async Task OpenEditActionModal(MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem act)
    {
        var parameters = new DialogParameters { ["ModuleId"] = act.ModuleId, ["Name"] = act.Name, ["Description"] = act.Description };
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.ActionPermissionDialog>("Editar Acción", parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            dynamic data = result.Data;
            act.Name = data.Name;
            act.ModuleId = data.ModuleId;
            act.Description = data.Description;
            SnackbarService.ShowSuccess("Acción actualizada");
            StateHasChanged();
        }
    }

    protected async Task DeleteAction(MedFarLab.Pwa.Pages.Admin.Components.RoleActionMapDialog.ActionItem act)
    {
        bool? result = await DialogService.ShowMessageBox("Cuidado Crítico", $"¿Estás seguro de eliminar el rastreo de la acción {act.Name}? Los usuarios con este claim lo perderán.", yesText: "Sí, desvincular", cancelText: "Cancelar");
        if (result == true)
        {
            mockActions.Remove(act);
            SnackbarService.ShowError("Acción eliminada");
            StateHasChanged();
        }
    }
}
