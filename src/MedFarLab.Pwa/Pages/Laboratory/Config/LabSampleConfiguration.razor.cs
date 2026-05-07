using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Domain.Const;
using MedFarLab.Application.Features.Laboratory.Models;
using MedFarLab.Pwa.Services;
using MedFarLab.Pwa.State;
using MediatR;
namespace MedFarLab.Pwa.Pages.Laboratory.Config
{
    public partial class LabSampleConfiguration : ComponentBase
    {
        [Parameter] public long ServiceId { get; set; }
        
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;
        [Inject] private AppState AppState { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.Services.MedFarSnackbarService Snackbar { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected List<ServiceSampleConfigDTO> Configs { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadConfigsAsync();
        }

        private async Task LoadConfigsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Queries.GetServiceSampleConfigs.GetServiceSampleConfigsQuery(ServiceId));
                if (response != null)
                {
                    Configs = response;
                }
            }
            catch (Exception ex)
            {
                Snackbar.ShowError("Error de conexión", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [Inject] private IDialogService DialogService { get; set; } = default!;

        protected async Task OpenAddDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<LabSampleConfigDialog>("Agregar Muestra", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is ServiceSampleConfigDTO newConfig)
            {
                Configs.Add(new ServiceSampleConfigDTO
                {
                    ServiceId = ServiceId,
                    SampleType = newConfig.SampleType,
                    ContainerType = newConfig.ContainerType,
                    RequiredVolume = newConfig.RequiredVolume,
                    SpecialInstructions = newConfig.SpecialInstructions,
                    IsActive = newConfig.IsActive
                });
            }
        }

        protected async Task OpenEditDialog(ServiceSampleConfigDTO item)
        {
            var parameters = new DialogParameters<LabSampleConfigDialog>
            {
                { x => x.InitialData, new ServiceSampleConfigDTO
                    {
                        SampleType = item.SampleType,
                        ContainerType = item.ContainerType,
                        RequiredVolume = item.RequiredVolume,
                        SpecialInstructions = item.SpecialInstructions,
                        IsActive = item.IsActive
                    } 
                }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<LabSampleConfigDialog>("Editar Muestra", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is ServiceSampleConfigDTO updatedConfig)
            {
                item.SampleType = updatedConfig.SampleType;
                item.ContainerType = updatedConfig.ContainerType;
                item.RequiredVolume = updatedConfig.RequiredVolume;
                item.SpecialInstructions = updatedConfig.SpecialInstructions;
                item.IsActive = updatedConfig.IsActive;
            }
        }

        protected void RemoveRow(ServiceSampleConfigDTO item)
        {
            Configs.Remove(item);
        }

        protected async Task SaveConfigsAsync()
        {
            if (Configs.Any(c => string.IsNullOrWhiteSpace(c.SampleType)))
            {
                Snackbar.ShowWarning("El 'Tipo de Muestra' es obligatorio para todas las filas.");
                return;
            }

            var response = await Mediator.Send(new MedFarLab.Application.Features.Laboratory.Commands.SaveServiceSampleConfigs.SaveServiceSampleConfigsCommand(
                AppState.OrganizationId,
                ServiceId,
                Configs
            ));

            if (response != null && response.IsSuccess)
            {
                Snackbar.ShowSuccess("Configuraciones guardadas exitosamente");
                GoBack();
            }
            else
            {
                Snackbar.ShowError(response?.Message ?? "Error al guardar");
            }
        }

        protected void GoBack()
        {
            NavManager.NavigateTo("/inventory/catalog");
        }
    }
}
