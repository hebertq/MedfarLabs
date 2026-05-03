using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;

using MedFarLab.Application.Features.Billing.Commands;

namespace MedFarLab.Pwa.Pages.Admin.Plans;

public partial class PlanEditorDialog : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;
    
    [Parameter] public UpdateSaasPlanCommand PlanToEdit { get; set; } = new();
    [Parameter] public bool IsNew { get; set; }

    protected MudForm form = default!;
    
    protected bool RoleClinico { get; set; }
    protected bool RoleAgenda { get; set; }
    protected bool RoleLaboratorio { get; set; }
    protected bool RoleFarmacia { get; set; }

    protected override void OnInitialized()
    {
    }

    protected async Task Save()
    {
        await form.Validate();
        if (form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(PlanToEdit));
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}

