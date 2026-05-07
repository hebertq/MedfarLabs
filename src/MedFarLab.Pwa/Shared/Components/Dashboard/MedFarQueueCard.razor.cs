using Microsoft.AspNetCore.Components;

namespace MedFarLab.Pwa.Shared.Components.Dashboard
{
    public partial class MedFarQueueCard<TItem> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string? Subtitle { get; set; }
        [Parameter] public string Icon { get; set; } = MudBlazor.Icons.Material.Filled.List;
        [Parameter] public IEnumerable<TItem>? Items { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; } = null!;
        [Parameter] public string EmptyTitle { get; set; } = "Todo en orden";
        [Parameter] public string EmptyMessage { get; set; } = "No hay elementos pendientes.";
    }
}
