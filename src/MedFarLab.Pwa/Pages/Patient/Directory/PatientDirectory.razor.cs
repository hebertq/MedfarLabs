using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedFarLab.Application.Features.Clinical.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;
using MedFarLab.Application.Features.Patient.Queries.GetPatientDirectory;

namespace MedFarLab.Pwa.Pages.Patient.Directory;

public partial class PatientDirectory : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISender Mediator { get; set; } = default!;
    [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

    private string SearchTerm { get; set; } = string.Empty;

    private List<PatientDirectoryVM> PatientsList = new();

    private IEnumerable<PatientDirectoryVM> FilteredPatients =>
        string.IsNullOrWhiteSpace(SearchTerm) 
            ? PatientsList 
            : PatientsList.Where(p => 
                p.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                p.DocumentId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            var data = await Mediator.Send(new GetPatientDirectoryQuery(AppState.OrganizationId));
            if (data != null) PatientsList = data;
        }
        catch { }
        finally 
        {
            IsLoading = false;
        }
    }

    private void CheckInPatient()
    {
        NavManager.NavigateTo("/patients/new");
    }

    private void OpenRecord(string id)
    {
        NavManager.NavigateTo($"/patients/record/{id}");
    }

    private void EditarPaciente(string id)
    {
        NavManager.NavigateTo($"/patients/edit/{id}");
    }

    private Task ExportarCSV()
    {
        // TODO: Implement CSV export
        return Task.CompletedTask;
    }

    private bool FiltrarPaciente(PatientDirectoryVM p, string term) =>
        p.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
        p.DocumentId.Contains(term, StringComparison.OrdinalIgnoreCase);

    private async Task OpenGlobalSearch()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Shared.PatientSearchDialog>("Buscar", options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is MedFarLab.Pwa.Shared.PatientSearchDialog.PatientSearchResultVM patient)
        {
            NavManager.NavigateTo($"/patients/record/{patient.PatientId}");
        }
    }
}
