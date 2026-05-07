using Microsoft.AspNetCore.Components;
using MediatR;
using MedFarLab.Application.Features.Laboratory.Queries.GetLabExamTemplates;
using MedFarLab.Application.Features.Laboratory.Queries.GetLabExamTemplateItems;
using MedFarLab.Application.Features.Laboratory.Commands.CloneLabTemplate;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Response;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Laboratory.Config
{
    public partial class LabTemplates : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.Services.MedFarSnackbarService Snackbar { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected bool IsLoadingItems { get; set; }
        protected bool IsCloning { get; set; }

        protected List<LabExamTemplateResponseDTO> Templates { get; set; } = new();
        protected LabExamTemplateResponseDTO? SelectedTemplate { get; set; }
        protected List<LabExamTemplateItemResponseDTO> TemplateItems { get; set; } = new();

        protected long SelectedTemplateId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTemplates();
        }

        protected void GoBack()
        {
            NavManager.NavigateTo("/laboratory/dashboard");
        }

        private async Task LoadTemplates()
        {
            IsLoading = true;
            StateHasChanged();

            try
            {
                var response = await Mediator.Send(new GetLabExamTemplatesQuery());
                if (response.IsSuccess && response.Data != null)
                {
                    Templates = response.Data.ToList();
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.ShowError("Error cargando el catálogo de exámenes.", ex.Message);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OnTemplateSelected(long templateId)
        {
            SelectedTemplateId = templateId;
            SelectedTemplate = Templates.FirstOrDefault(t => t.Id == templateId);
            
            if (SelectedTemplate != null)
            {
                await LoadTemplateItems(templateId);
            }
        }

        private async Task LoadTemplateItems(long templateId)
        {
            IsLoadingItems = true;
            StateHasChanged();

            try
            {
                var response = await Mediator.Send(new GetLabExamTemplateItemsQuery(templateId));
                if (response.IsSuccess && response.Data != null)
                {
                    TemplateItems = response.Data.ToList();
                }
                else
                {
                    TemplateItems.Clear();
                    Snackbar.ShowError("Error al cargar los analitos.");
                }
            }
            catch (System.Exception ex)
            {
                TemplateItems.Clear();
                Snackbar.ShowError("Error cargando los analitos.", ex.Message);
            }
            finally
            {
                IsLoadingItems = false;
                StateHasChanged();
            }
        }

        protected async Task CloneTemplate()
        {
            if (SelectedTemplate == null) return;

            IsCloning = true;
            StateHasChanged();

            // In real app, organizationId comes from Auth Context. Hardcoding to 1 for this module
            long organizationId = 1;
            
            try
            {
                var command = new CloneLabTemplateCommand(organizationId, SelectedTemplate.Id);
                var response = await Mediator.Send(command);

                if (response != null && response.IsSuccess)
                {
                    Snackbar.ShowSuccess($"Examen {SelectedTemplate.Name} clonado exitosamente.");
                }
                else
                {
                    Snackbar.ShowError(response?.Message ?? "Error al clonar plantilla.");
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.ShowError("Fallo en la comunicación al clonar.", ex.Message);
            }
            finally
            {
                IsCloning = false;
                StateHasChanged();
            }
        }
    }
}
