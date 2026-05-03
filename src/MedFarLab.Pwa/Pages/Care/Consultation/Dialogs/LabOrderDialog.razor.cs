using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MediatR;
using MedFarLab.Application.Features.Inventory.Queries.GetServiceCatalogQuery;
using MedFarLab.Application.Features.Inventory.Models;

namespace MedFarLab.Pwa.Pages.Care.Consultation.Dialogs
{
    public partial class LabOrderDialog : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;

        [Parameter] public LabOrderDTO? InitialData { get; set; }
        
        public ModelData Model { get; set; } = new();
        protected List<ServiceItemVM> GlobalCatalog { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (InitialData != null)
            {
                Model.TestName = InitialData.TestName;
                Model.Notes = InitialData.Notes;
            }

            var response = await Mediator.Send(new GetServiceCatalogQuery("laboratory"));
            if (response.IsSuccess && response.Data != null)
            {
                GlobalCatalog = response.Data.Where(x => x.Category == "Laboratorio").ToList();
            }
        }

        private Task<IEnumerable<string>> SearchExams(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Task.FromResult(GlobalCatalog.Select(x => x.Name).AsEnumerable());
            }

            return Task.FromResult(GlobalCatalog
                .Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || 
                            x.Code.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Name).AsEnumerable());
        }

        protected bool IsFormValid => !string.IsNullOrWhiteSpace(Model.TestName);

        protected void Submit()
        {
            if (IsFormValid)
            {
                MudDialog.Close(DialogResult.Ok(new LabOrderDTO(Model.TestName, Model.Notes)));
            }
        }

        protected void Cancel() => MudDialog.Cancel();

        public class ModelData
        {
            public string TestName { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
        }
    }
}

