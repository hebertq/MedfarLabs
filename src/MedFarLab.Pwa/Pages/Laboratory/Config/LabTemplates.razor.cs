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
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

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

        private async Task LoadTemplates()
        {
            IsLoading = true;
            StateHasChanged();

            var response = await Mediator.Send(new GetLabExamTemplatesQuery());
            if (response.IsSuccess && response.Data != null)
            {
                Templates = response.Data.ToList();
            }
            else
            {
                Snackbar.Add("Error al cargar las plantillas globales.", Severity.Error);
            }

            IsLoading = false;
            StateHasChanged();
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

            var response = await Mediator.Send(new GetLabExamTemplateItemsQuery(templateId));
            if (response.IsSuccess && response.Data != null)
            {
                TemplateItems = response.Data.ToList();
            }
            else
            {
                TemplateItems.Clear();
                Snackbar.Add("Error al cargar los analitos.", Severity.Error);
            }

            IsLoadingItems = false;
            StateHasChanged();
        }

        protected async Task CloneTemplate()
        {
            if (SelectedTemplate == null) return;

            IsCloning = true;
            StateHasChanged();

            // In real app, organizationId comes from Auth Context. Hardcoding to 1 for this module
            long organizationId = 1;
            
            var command = new CloneLabTemplateCommand(organizationId, SelectedTemplate.Id);
            var response = await Mediator.Send(command);

            if (response.IsSuccess)
            {
                Snackbar.Add($"Plantilla '{SelectedTemplate.Name}' clonada exitosamente a tu laboratorio.", Severity.Success);
            }
            else
            {
                Snackbar.Add(response.Message ?? "Error al clonar la plantilla. Tal vez ya la tienes configurada.", Severity.Warning);
            }

            IsCloning = false;
            StateHasChanged();
        }
    }
}
