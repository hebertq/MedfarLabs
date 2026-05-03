using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class AntecedentModal : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public long PatientId { get; set; }

    public class FormModel 
    {
        public long? TypeId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public FormModel Model { get; set; } = new FormModel();

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void Submit()
    {
        if (Model.TypeId.HasValue && Model.TypeId > 0 && !string.IsNullOrWhiteSpace(Model.Description))
        {
            var payload = new AntecedentRequestDTO(PatientId, Model.TypeId.Value, Model.Description);
            MudDialog.Close(DialogResult.Ok(payload));
        }
    }
}

