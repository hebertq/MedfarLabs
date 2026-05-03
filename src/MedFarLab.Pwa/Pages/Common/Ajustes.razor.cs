using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Common
{
    public partial class Ajustes : ComponentBase
    {
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;

        protected bool IsSending { get; set; }

        protected async Task CreateBackupAsync()
        {
            IsSending = true;
            StateHasChanged();

            // Simulate Backup API request
            await Task.Delay(2000); 

            IsSending = false;
            Snackbar.Add("Respaldo físico programado. Recibirás un enlace de descarga segura.", Severity.Success);
            StateHasChanged();
        }

        protected void OpenSettingsModal(string type)
        {
            Snackbar.Add($"Módulo {type} se habilitará en la siguiente fase de desarrollo.", Severity.Info);
        }

        protected async Task OpenTemplatesModalAsync()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Common.TemplatesSettingsModal>("Diseñador de Documentos", options);
        }
    }
}
