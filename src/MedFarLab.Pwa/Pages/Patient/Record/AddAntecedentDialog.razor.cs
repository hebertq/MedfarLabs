using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using MediatR;
using MedFarLab.Application.Features.Patient.Models;
using MedFarLab.Pwa.State;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class AddAntecedentDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public long PatientId { get; set; }

    [Inject] private MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient ApiClient { get; set; } = default!;
    [Inject] private AppState AppState { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    protected AntecedentRequestModel Model { get; set; } = new AntecedentRequestModel { TypeId = 1 };

    protected void Cancel() => MudDialog.Cancel();

    protected async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(Model.Description))
        {
            Snackbar.Add("La descripción es obligatoria", Severity.Warning);
            return;
        }

        try
        {
            var payload = new
            {
                patient_id = PatientId,
                type_id = Model.TypeId,
                description = Model.Description,
                user_id = AppState.UserId
            };

            var response = await ApiClient.PostAsync<object, object>("api/Clinical/4004", payload); // 4004 = RegistrarAntecedente

            if (response != null && response.IsSuccess)
            {
                Snackbar.Add("Antecedente agregado exitosamente", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add(response?.Message ?? "Error al guardar el antecedente", Severity.Error);
            }
        }
        catch (System.Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    public class AntecedentRequestModel
    {
        public int TypeId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
