using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Response;

namespace MedFarLab.Pwa.Pages.Care.Consultation.Dialogs
{
    public partial class DiagnosisDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public DiagnosisCodeDTO? Diagnosis { get; set; }

        protected string DiagnosisType { get; set; } = "Presuntivo";
        protected string Observations { get; set; } = string.Empty;

        void Submit()
        {
            if (Diagnosis == null) return;
            MudDialog.Close(DialogResult.Ok(Diagnosis));
        }

        void Cancel() => MudDialog.Cancel();
    }
}

