using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace MedFarLab.Pwa.Pages.Patient.Record
{
    public partial class PatientAlertModal
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public long PatientId { get; set; }

        public AlertFormModel Model { get; set; } = new AlertFormModel();
        bool success;
        string[] errors = { };
        MudForm form = default!;
        bool isSubmitting = false;

        void Cancel() => MudDialog.Cancel();

        async Task Submit()
        {
            await form.Validate();
            if (!form.IsValid) return;

            isSubmitting = true;
            StateHasChanged();
            
            // Retornamos el modelo para que el componente padre ejecute el API
            MudDialog.Close(DialogResult.Ok(Model));
        }

        public class AlertFormModel
        {
            public int AlertTypeId { get; set; } = 1;
            public int SeverityId { get; set; } = 2; // HIGH
            public string Message { get; set; } = string.Empty;
        }
    }
}
