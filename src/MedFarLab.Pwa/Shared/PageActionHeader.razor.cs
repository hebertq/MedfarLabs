using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Reflection;

namespace MedFarLab.Pwa.Shared;

public partial class PageActionHeader : ComponentBase
{
    [Parameter] public RenderFragment? ActionContent { get; set; }
    [Parameter] public RenderFragment? DropdownActions { get; set; }
    [Parameter] public RenderFragment? StatusContent { get; set; }
    [Parameter] public bool Sticky { get; set; } = false;
    [Parameter] public bool ShowBackButton { get; set; } = false;
    [Parameter] public string? BackUrl { get; set; }
    [Parameter] public EventCallback OnBackClick { get; set; }

    /// <summary>
    /// Lista de objetos a exportar. Si se proporciona, el botón de Excel aparecerá automáticamente.
    /// </summary>
    [Parameter] public System.Collections.IEnumerable? ExportData { get; set; }
    
    [Parameter] public string ExportFileName { get; set; } = "Exportacion";

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected async Task HandleBackClick()
    {
        if (OnBackClick.HasDelegate)
        {
            await OnBackClick.InvokeAsync();
        }
        else if (!string.IsNullOrEmpty(BackUrl))
        {
            NavigationManager.NavigateTo(BackUrl);
        }
        else
        {
            await JSRuntime.InvokeVoidAsync("history.back");
        }
    }

    protected async Task HandleExportExcel()
    {
        if (ExportData == null) return;

        var csv = new System.Text.StringBuilder();
        bool isHeaderWritten = false;
        PropertyInfo[]? props = null;

        foreach (var item in ExportData)
        {
            if (item == null) continue;

            if (!isHeaderWritten)
            {
                props = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                csv.AppendLine(string.Join(",", props.Select(p => "\"" + p.Name.Replace("\"", "\"\"") + "\"")));
                isHeaderWritten = true;
            }

            var line = string.Join(",", props!.Select(p => 
            {
                var val = p.GetValue(item)?.ToString() ?? "";
                return "\"" + val.Replace("\"", "\"\"") + "\"";
            }));
            csv.AppendLine(line);
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var base64 = Convert.ToBase64String(bytes);

        // Disparar descarga usando JS
        var jsCode = "var link = document.createElement('a'); link.href = 'data:text/csv;base64,' + '" + base64 + "'; link.download = '" + ExportFileName + ".csv'; document.body.appendChild(link); link.click(); document.body.removeChild(link);";
        await JSRuntime.InvokeVoidAsync("eval", jsCode);
    }
}
