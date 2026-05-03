using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Application.Features.Inventory.Models;

namespace MedFarLab.Pwa.Pages.Billing;

public partial class InvoiceItemModal : ComponentBase
{
    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public InvoiceItemVM Model { get; set; } = new InvoiceItemVM();

    [Parameter]
    public List<ServiceItemVM> GlobalCatalog { get; set; } = new();

    [Parameter]
    public bool AllowManualPriceEdit { get; set; } = true;

    protected override void OnInitialized()
    {
        // Clone if initialized, but we pass clean modal or cloned modal model.
    }

    protected Task<IEnumerable<ServiceItemVM>> SearchServices(string value, CancellationToken token)
    {
        if (string.IsNullOrEmpty(value))
            return Task.FromResult(GlobalCatalog.AsEnumerable());
            
        return Task.FromResult(GlobalCatalog.Where(x => 
            x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || 
            x.Code.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
    }

    protected void OnServiceSelected(ServiceItemVM selected)
    {
        Model.SelectedService = selected;
        if (selected != null)
        {
            Model.Description = selected.Name;
            Model.UnitPrice = selected.UnitPrice;
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void Submit()
    {
        if (!string.IsNullOrWhiteSpace(Model.Description) && Model.Quantity > 0 && Model.UnitPrice >= 0)
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
    }
}

