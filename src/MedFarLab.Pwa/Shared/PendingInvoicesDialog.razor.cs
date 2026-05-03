using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Shared;

public partial class PendingInvoicesDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected List<PendingInvoiceVM> PendingInvoices { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(400); // Simulate API call to fetch pending
        
        PendingInvoices = new List<PendingInvoiceVM>
        {
            new PendingInvoiceVM { Id = 1001, PatientId = 55018, PatientName = "Mónica Castilleros", Concept = "Consulta General + Inyección", Subtotal = 65.00m },
            new PendingInvoiceVM { Id = 1002, PatientId = 12044, PatientName = "Carlos Martínez", Concept = "Consulta Cardiológica", Subtotal = 80.00m },
            new PendingInvoiceVM { Id = 1003, PatientId = 89001, PatientName = "Lucía Fernández", Concept = "Control Neurológico", Subtotal = 50.00m }
        };

        IsLoading = false;
    }

    protected void SelectInvoice(PendingInvoiceVM invoice)
    {
        MudDialog.Close(DialogResult.Ok(invoice));
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    public class PendingInvoiceVM
    {
        public long Id { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Concept { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
    }
}

