using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MedFarLab.Pwa.Shared.Components.Forms
{
    public partial class MedFarForm<TModel> : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;

        private bool _isSubmitting = false;
        private string _serverError = string.Empty;
        private string _traceId = string.Empty;

        [Parameter] public TModel Model { get; set; } = default!;
        [Parameter] public RenderFragment? FormContent { get; set; }
        [Parameter] public RenderFragment? SecondaryActions { get; set; }
        [Parameter] public EventCallback<TModel> OnSubmit { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public string SubmitText { get; set; } = "Guardar";
        [Parameter] public string SubmitIcon { get; set; } = MudBlazor.Icons.Material.Filled.Save;
        [Parameter] public string CancelText { get; set; } = "Cancelar";
        [Parameter] public bool ShowCancel { get; set; } = true;
        [Parameter] public bool IsCompact { get; set; } = false;
        [Parameter] public bool ActionsSticky { get; set; } = false;

        /// <summary>
        /// Llamar desde el Code-Behind del padre cuando el API retorna IsSuccess=false
        /// </summary>
        public void SetServerError(string message, string? traceId = null)
        {
            _serverError = message;
            _traceId     = traceId ?? string.Empty;
            StateHasChanged();
        }

        public void ClearServerError()
        {
            _serverError = string.Empty;
            _traceId     = string.Empty;
        }

        private async Task HandleValidSubmit()
        {
            _isSubmitting = true;
            ClearServerError();
            try { await OnSubmit.InvokeAsync(Model); }
            finally { _isSubmitting = false; }
        }

        private async Task OnCancelClick() => await OnCancel.InvokeAsync();

        private async Task CopyTraceId()
        {
            if (!string.IsNullOrEmpty(_traceId))
            {
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", _traceId);
            }
        }
    }
}
