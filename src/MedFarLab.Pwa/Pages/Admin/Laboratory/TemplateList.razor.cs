using Microsoft.AspNetCore.Components;
using MediatR;
using MudBlazor;
using MedFarLab.Application.Features.Laboratory.Queries.GetLabExamTemplates;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Response;

namespace MedFarLab.Pwa.Pages.Admin.Laboratory
{
    public partial class TemplateList : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected List<LabExamTemplateResponseDTO> Templates { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            IsLoading = true;
            StateHasChanged();
            
            try
            {
                var response = await Mediator.Send(new GetLabExamTemplatesQuery());
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    Templates = response.Data.ToList();
                }
                else
                {
                    Snackbar.Add($"Error al cargar plantillas: {response?.Message}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Excepción de conexión: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OpenNewTemplateModal()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
            var dialog = await DialogService.ShowAsync<TemplateFormDialog>("Nueva Plantilla de Laboratorio", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
            }
        }

        protected async Task OpenEditTemplateModal(LabExamTemplateResponseDTO template)
        {
            var parameters = new DialogParameters { ["TemplateModel"] = template };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
            var dialog = await DialogService.ShowAsync<TemplateFormDialog>("Editar Plantilla", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadData();
            }
        }

        protected async Task OpenConfigureItemsModal(LabExamTemplateResponseDTO template)
        {
            var parameters = new DialogParameters { ["TemplateId"] = template.Id, ["TemplateName"] = template.Name };
            var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseButton = true };
            var dialog = await DialogService.ShowAsync<TemplateItemsDialog>("Configurar Ítems de la Plantilla", parameters, options);
            await dialog.Result;
            
            // Reload to update item counts
            await LoadData();
        }
    }
}
