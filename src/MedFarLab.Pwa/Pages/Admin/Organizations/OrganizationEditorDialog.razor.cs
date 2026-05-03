using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MedFarLab.Pwa.Pages.Admin.Organizations.OrganizationManager;

namespace MedFarLab.Pwa.Pages.Admin.Organizations;

public partial class OrganizationEditorDialog : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;
    
    [Parameter] public OrganizationDto OrgToEdit { get; set; } = new();

    protected MudForm form = default!;

    protected async Task Save()
    {
        await form.Validate();
        if (form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(OrgToEdit));
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}

