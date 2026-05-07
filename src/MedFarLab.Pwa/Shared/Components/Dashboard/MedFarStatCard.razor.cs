using Microsoft.AspNetCore.Components;

namespace MedFarLab.Pwa.Shared.Components.Dashboard
{
    public partial class MedFarStatCard : ComponentBase
    {
        [Parameter] public string Label { get; set; } = string.Empty;
        [Parameter] public string Value { get; set; } = "0";
        [Parameter] public string? Subtitle { get; set; }
        [Parameter] public string Icon { get; set; } = MudBlazor.Icons.Material.Filled.Analytics;
        [Parameter] public string IconColor { get; set; } = "#10B981";
        [Parameter] public string IconBg { get; set; } = "rgba(16,185,129,0.12)";
        [Parameter] public bool IsLoading { get; set; } = false;
        [Parameter] public bool ShowTrend { get; set; } = false;
        [Parameter] public bool TrendUp { get; set; } = true;
        [Parameter] public string TrendText { get; set; } = string.Empty;
        [Parameter] public string? CssClass { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }
    }
}
