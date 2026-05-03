using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Linq;

namespace MedFarLab.Pwa.Pages.Admin.Components;

public partial class RoleActionMapDialog : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public int RoleId { get; set; }
    [Parameter] public string RoleName { get; set; } = "";
    
    // Lista total de acciones disponibles en el sistema (pasada desde el padre para visualización)
    [Parameter] public List<ActionItem> MockActions { get; set; } = new();

    // Lista de las sub-acciones que este Rol ya tiene marcadas
    [Parameter] public List<int> SelectedActionIds { get; set; } = new();

    protected void OnActionToggled(int actionId, bool isChecked)
    {
        if(isChecked && !SelectedActionIds.Contains(actionId))
        {
            SelectedActionIds.Add(actionId);
        }
        else if(!isChecked && SelectedActionIds.Contains(actionId))
        {
            SelectedActionIds.Remove(actionId);
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void Submit()
    {
        MudDialog.Close(DialogResult.Ok(SelectedActionIds));
    }

    protected string GetModuleName(int id)
    {
        // Mini mapper manual temporal
        return id switch {
            1 => "Security", 2 => "Identity", 3 => "Billing",
            4 => "Clinical", 5 => "Care", 6 => "Common",
            7 => "Inventory", 8 => "Laboratory", _ => "General"
        };
    }

    // Estructura ligera de comunicación interna
    public class ActionItem 
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}

