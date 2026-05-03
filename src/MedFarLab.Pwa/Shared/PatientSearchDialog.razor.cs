using MedFarLab.Application.Features.Identity.Queries.SearchPersons;
using MedFarLab.Pwa.State;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Shared
{
    public partial class PatientSearchDialog : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private AppState AppState { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected string SearchQuery { get; set; } = string.Empty;
        protected bool IsSearching { get; set; } = false;

        protected List<PatientSearchResultVM> Results { get; set; } = new();

        protected async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            IsSearching = true;

            var result = await Mediator.Send(new SearchPersonsCommand(SearchQuery, AppState.OrganizationId));
            if (result.IsSuccess && result.Data != null)
            {
                Results = result.Data.Select(x => new PatientSearchResultVM
                {
                    PersonId = x.PersonId,
                    PatientId = x.PatientId,
                    RecordId = x.RecordId ?? "",
                    FullName = x.FullName,
                    AgeAndSex = x.AgeAndSex,
                    Phone = x.Phone,
                    IsPatient = x.IsPatient
                }).ToList();
            }
            else
            {
                Snackbar.Add("Error al buscar identidades", Severity.Error);
            }

            IsSearching = false;
        }

        protected void SelectPatient(PatientSearchResultVM patient)
        {
            MudDialog.Close(DialogResult.Ok(patient));
        }

        protected void ConvertToPatient(PatientSearchResultVM patient)
        {
            MudDialog.Close();
            NavManager.NavigateTo($"/patients/new?personId={patient.PersonId}");
        }

        protected void Cancel()
        {
            MudDialog.Cancel();
        }

        public class PatientSearchResultVM
        {
            public long PersonId { get; set; }
            public long? PatientId { get; set; }
            public string RecordId { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string AgeAndSex { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public bool IsPatient { get; set; }
        }
    }
}

