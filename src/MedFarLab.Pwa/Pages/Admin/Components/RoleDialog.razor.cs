using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Admin.Components;

public partial class RoleDialog : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string Name { get; set; } = "";

    protected bool success;
    protected MudForm form = default!;

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected async Task Submit()
    {
        await form.Validate();
        if (form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(new { Name }));
        }
    }
}

