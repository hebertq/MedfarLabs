using System.Net.Http.Json;
using System.Text.Json;
using MedfarLabs.Core.Domain.Entities.Common;
using MedfarLabs.Core.Domain.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;

using MedFarLab.Application.Features.Laboratory.Models;

namespace MedFarLab.Pwa.Pages.Laboratory.Config
{
    public partial class LabSampleConfigDialog : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;

        [Parameter] public ServiceSampleConfigDTO? InitialData { get; set; }

        public ServiceSampleConfigDTO Model { get; set; } = new();

        private MudForm form = default!;
        private bool success;

        private List<CatalogDetail> _sampleTypes = new();
        private List<CatalogDetail> _containerTypes = new();

        protected override async Task OnInitializedAsync()
        {
            if (InitialData != null)
            {
                Model.ServiceId = InitialData.ServiceId;
                Model.SampleType = InitialData.SampleType;
                Model.ContainerType = InitialData.ContainerType;
                Model.RequiredVolume = InitialData.RequiredVolume;
                Model.SpecialInstructions = InitialData.SpecialInstructions;
                Model.IsActive = InitialData.IsActive;
            }

            await LoadCatalogs();
        }

        private async Task LoadCatalogs()
        {
            try
            {
                // Fetch Sample Types
                var sampleResponse = await Mediator.Send(new MedFarLab.Application.Features.Common.Queries.GetCatalogDetails.GetCatalogDetailsQuery(26));
                if (sampleResponse != null)
                {
                    _sampleTypes = sampleResponse;
                }

                // Fetch Container Types
                var containerResponse = await Mediator.Send(new MedFarLab.Application.Features.Common.Queries.GetCatalogDetails.GetCatalogDetailsQuery(27));
                if (containerResponse != null)
                {
                    _containerTypes = containerResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar catálogos: " + ex.Message);
            }
        }

        private async Task<IEnumerable<string>> SearchSampleType(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
                return _sampleTypes.Select(x => x.Name);
            
            return _sampleTypes.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                               .Select(x => x.Name);
        }

        private async Task<IEnumerable<string>> SearchContainerType(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
                return _containerTypes.Select(x => x.Name);
            
            return _containerTypes.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                                  .Select(x => x.Name);
        }

        private async Task AddSampleType()
        {
            if (!string.IsNullOrWhiteSpace(Model.SampleType))
            {
                await SaveToCatalog(26, Model.SampleType);
                await LoadCatalogs();
                Snackbar.Add("Tipo de Muestra agregado al catálogo", Severity.Success);
            }
        }

        private async Task AddContainerType()
        {
            if (!string.IsNullOrWhiteSpace(Model.ContainerType))
            {
                await SaveToCatalog(27, Model.ContainerType);
                await LoadCatalogs();
                Snackbar.Add("Tipo de Contenedor agregado al catálogo", Severity.Success);
            }
        }

        private async Task SaveToCatalog(int catalogId, string name)
        {
            try
            {
                await Mediator.Send(new MedFarLab.Application.Features.Common.Commands.CreateCatalogDetail.CreateCatalogDetailCommand(catalogId, name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando catálogo {catalogId}: {ex.Message}");
            }
        }

        private void Submit()
        {
            form.Validate();
            if (success)
            {
                MudDialog.Close(DialogResult.Ok(Model));
            }
        }

        private void Cancel() => MudDialog.Cancel();
    }
}

