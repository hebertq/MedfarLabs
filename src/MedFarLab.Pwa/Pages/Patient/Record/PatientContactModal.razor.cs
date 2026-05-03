using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace MedFarLab.Pwa.Pages.Patient.Record
{
    public partial class PatientContactModal
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public ContactFormModel Model { get; set; } = new ContactFormModel();
        
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
            
            MudDialog.Close(DialogResult.Ok(Model));
        }

        public class ContactFormModel
        {
            public long Id { get; set; } // En caso de edición
            public string FullName { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public int RelationshipId { get; set; } = 1;
            public bool IsPrimary { get; set; }
        }
    }
}
