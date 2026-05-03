using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Care.Consultation.Dialogs
{
    public partial class AppliedProductDialog : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

        // We use the same class defined in ConsultationWorkspace for now, 
        // to avoid rewriting logic. Let's just create a local wrapper.
        [Parameter] public ProductModel Model { get; set; } = new();

        protected bool IsFormValid => !string.IsNullOrWhiteSpace(Model.ProductName) && Model.Quantity > 0;

        protected void Submit()
        {
            if (IsFormValid)
            {
                MudDialog.Close(DialogResult.Ok(Model));
            }
        }

        protected void Cancel() => MudDialog.Cancel();
        
        public class ProductModel
        {
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; } = 1;
        }
    }
}

