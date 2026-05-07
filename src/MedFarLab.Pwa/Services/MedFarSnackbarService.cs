using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Services
{
    public class MedFarSnackbarService
    {
        private readonly ISnackbar _snackbar;

        public MedFarSnackbarService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void ShowSuccess(string message)
        {
            _snackbar.Add(message, Severity.Success);
        }

        public void ShowWarning(string message)
        {
            _snackbar.Add(message, Severity.Warning);
        }

        public void ShowError(string message, string? traceId = null)
        {
            if (string.IsNullOrEmpty(traceId))
            {
                _snackbar.Add(message, Severity.Error);
            }
            else
            {
                // Incluye el TraceId para que el usuario pueda reportarlo a soporte
                var content = $"<div>{message}</div><div style='font-size: 0.7rem; margin-top: 4px; opacity: 0.8;'>Trace ID: {traceId}</div>";
                _snackbar.Add((MarkupString)content, Severity.Error);
            }
        }
    }
}
