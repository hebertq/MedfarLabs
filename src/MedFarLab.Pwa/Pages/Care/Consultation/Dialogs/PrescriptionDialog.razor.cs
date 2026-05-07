using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MediatR;
using MedFarLab.Application.Features.Pharmacy.Queries.GetMedicationCatalog;

namespace MedFarLab.Pwa.Pages.Care.Consultation.Dialogs
{
    public partial class PrescriptionDialog : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;

        [Parameter] public PrescriptionItemDTO? InitialData { get; set; }
        [Parameter] public List<string> PatientAllergies { get; set; } = new();
        
        protected MedFarLab.Pwa.Shared.Clinical.AllergyWarningModal AllergyModal { get; set; } = default!;

        public ModelData Model { get; set; } = new();
        protected List<MedicationItemDTO> GlobalMedications { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (InitialData != null)
            {
                Model.MedicationName = InitialData.MedicationName;
                Model.Dosage = InitialData.Dosage;
                Model.Frequency = InitialData.Frequency;
                Model.Duration = InitialData.Duration;
                Model.Instructions = InitialData.Instructions;
            }

            var response = await Mediator.Send(new GetMedicationCatalogQuery());
            if (response.IsSuccess && response.Data != null)
            {
                GlobalMedications = response.Data.ToList();
            }
        }

        private Task<IEnumerable<string>> SearchMedications(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Task.FromResult(GlobalMedications.Select(x => x.Name).AsEnumerable());
            }

            return Task.FromResult(GlobalMedications
                .Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || 
                            x.GenericComponent.Contains(value, StringComparison.InvariantCultureIgnoreCase) ||
                            x.Brand.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Name).AsEnumerable());
        }

        protected bool IsFormValid => 
            !string.IsNullOrWhiteSpace(Model.MedicationName) &&
            !string.IsNullOrWhiteSpace(Model.Dosage) &&
            !string.IsNullOrWhiteSpace(Model.Frequency) &&
            !string.IsNullOrWhiteSpace(Model.Duration);

        protected async Task Submit()
        {
            if (IsFormValid)
            {
                if (PatientAllergies.Any())
                {
                    bool isSafe = await AllergyModal.CheckAndConfirmAsync(Model.MedicationName, PatientAllergies);
                    if (!isSafe)
                    {
                        return;
                    }
                }

                MudDialog.Close(DialogResult.Ok(new PrescriptionItemDTO(Model.MedicationName, Model.Dosage, Model.Frequency, Model.Duration, Model.Instructions)));
            }
        }

        protected void Cancel() => MudDialog.Cancel();

        public class ModelData
        {
            public string MedicationName { get; set; } = string.Empty;
            public string Dosage { get; set; } = string.Empty;
            public string Frequency { get; set; } = string.Empty;
            public string Duration { get; set; } = string.Empty;
            public string Instructions { get; set; } = string.Empty;
        }
    }
}

