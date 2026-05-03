using Microsoft.AspNetCore.Components;
using MedFarLab.Pwa.Services;

namespace MedFarLab.Pwa.Shared;

public partial class ToastContainer : ComponentBase, IDisposable
{
    [Inject] protected NotificationService NotificationService { get; set; } = default!;

    protected override void OnInitialized()
    {
        NotificationService.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        NotificationService.OnChange -= StateHasChanged;
    }
    
    protected string GetColorClass(string type)
    {
        return type switch
        {
            "success" => "success",
            "error" => "error",
            _ => "info"
        };
    }
}
