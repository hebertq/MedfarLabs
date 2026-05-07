using Microsoft.AspNetCore.Components;

namespace MedFarLab.Pwa.Shared.Components.Forms
{
    public partial class MedFarField : ComponentBase
    {
        [Parameter] public RenderFragment? FieldContent { get; set; }
        [Parameter] public string? ServerError { get; set; }
    }
}
