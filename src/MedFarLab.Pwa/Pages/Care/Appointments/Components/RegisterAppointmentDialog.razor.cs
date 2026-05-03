using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Application.Features.Care.Models;

namespace MedFarLab.Pwa.Pages.Care.Appointments.Components;

public partial class RegisterAppointmentDialog : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    
    [Parameter] public AppointmentVM Model { get; set; } = new();
    
    protected string PatientName { get; set; } = string.Empty;

    protected async Task OpenPatientSearch()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Shared.PatientSearchDialog>("Buscar", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var patient = result.Data as MedFarLab.Pwa.Shared.PatientSearchDialog.PatientSearchResultVM;
            if (patient != null)
            {
                Model.PatientId = patient.PatientId ?? 0;
                PatientName = $"{patient.FullName} ({patient.RecordId})";
                StateHasChanged();
            }
        }
    }

    protected void Submit() => MudDialog.Close(DialogResult.Ok(Model));
    protected void Cancel() => MudDialog.Cancel();
}

