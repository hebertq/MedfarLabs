using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using MedFarLab.Application.Features.Patient.Models;
using MedFarLab.Application.Features.Patient.Queries.GetPatientRecord;
using MudBlazor;
using MedFarLab.Application.Features.Clinical.Commands.RegisterAntecedent;
using MedFarLab.Application.Features.Clinical.Commands.RegisterConsent;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedFarLab.Application.Features.Care.Models;
using MedFarLab.Application.Features.Care.Commands.RegisterAppointment;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class PatientRecord : ComponentBase
{
    [Inject] 
    private NavigationManager NavManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IMediator Mediator { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

    [Parameter]
    public string Id { get; set; } = string.Empty;

    protected bool IsLoading { get; set; } = true;
    protected PatientRecordVM Model { get; set; } = new();

    public List<MudBlazor.ChartSeries> Series = new();

    // Observability Data
    protected List<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientAlertResponseDTO> Alerts = new();
    protected List<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientContactResponseDTO> Contacts = new();
    protected List<MedfarLabs.Core.Application.Features.Security.Dtos.Response.AccessLogEntryDTO> AccessLogs = new();

    [Inject]
    private MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient ExternalClient { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        Series.Clear();

        long patientId = long.TryParse(Id, out var parsed) ? parsed : 1;
        var response = await Mediator.Send(new GetPatientRecordQuery { PatientId = patientId, OrganizationId = AppState.OrganizationId });

        if (response != null && response.IsSuccess && response.Data != null)
        {
            Model = response.Data;
            Series.Add(new MudBlazor.ChartSeries { Name = "Sistólica", Data = Model.BloodPressureSystolic });
            Series.Add(new MudBlazor.ChartSeries { Name = "Diastólica", Data = Model.BloodPressureDiastolic });

            // Load Observability Data
            await LoadObservabilityData(patientId);

            // Log Access
            _ = Task.Run(async () =>
            {
                var accessLog = new MedfarLabs.Core.Application.Features.Security.Dtos.Request.LogPatientAccessRequestDTO(
                    patientId, "READ", "MEDICAL_RECORD", null, "Visión 360"
                ) { OrganizationId = AppState.OrganizationId };
                await ExternalClient.PostAsync<object, object>("api/Security/4126", accessLog);
            });
        }

        IsLoading = false;
        StateHasChanged();
    }

    private async Task LoadObservabilityData(long patientId)
    {
        var alertReq = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.GetPatientAlertsRequestDTO(patientId) { OrganizationId = AppState.OrganizationId };
        var alertsRes = await ExternalClient.PostAsync<object, IEnumerable<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientAlertResponseDTO>>("api/Clinical/4114", alertReq);
        if (alertsRes?.Data != null) Alerts = alertsRes.Data.ToList();

        var contactReq = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.GetPatientContactsRequestDTO(patientId) { OrganizationId = AppState.OrganizationId };
        var contactsRes = await ExternalClient.PostAsync<object, IEnumerable<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientContactResponseDTO>>("api/Clinical/4120", contactReq);
        if (contactsRes?.Data != null) Contacts = contactsRes.Data.ToList();

        var logReq = new MedfarLabs.Core.Application.Features.Security.Dtos.Request.GetAccessHistoryRequestDTO(patientId, 50, 0) { OrganizationId = AppState.OrganizationId };
        var logsRes = await ExternalClient.PostAsync<object, MedfarLabs.Core.Application.Features.Security.Dtos.Response.AccessHistoryResponseDTO>("api/Security/4125", logReq);
        if (logsRes?.Data?.Items != null) AccessLogs = logsRes.Data.Items.ToList();
    }

    protected async Task GoBack()
    {
        await JSRuntime.InvokeVoidAsync("history.back");
    }

    protected bool IsCreatingConsultation { get; set; } = false;

    protected async Task CreateDirectConsultation()
    {
        if (IsCreatingConsultation) return;
        IsCreatingConsultation = true;
        StateHasChanged();

        try
        {
            var vm = new AppointmentVM
            {
                PatientId = Model.PatientId,
                Date = DateTime.Today,
                SelectedTime = DateTime.Now.TimeOfDay,
                Status = AppointmentStatus.Programada,
                Reason = "Consulta express generada desde expediente médico"
            };

            var response = await Mediator.Send(new RegisterAppointmentCommand(vm));

            if (response != null && response.IsSuccess)
            {
                long newAppointmentId = 0;
                
                if (response.Data is System.Text.Json.JsonElement element && element.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    newAppointmentId = element.GetInt64();
                }
                else if (response.Data != null)
                {
                    long.TryParse(response.Data.ToString(), out newAppointmentId);
                }

                if (newAppointmentId > 0)
                {
                    NavManager.NavigateTo($"/care/consultation/realizar/{newAppointmentId}");
                }
                else
                {
                    Snackbar.Add("Consulta generada en la agenda, pero no se pudo obtener el ID para redirigir.", Severity.Warning);
                    NavManager.NavigateTo("/care/appointments");
                }
            }
            else
            {
                Snackbar.Add(response?.Message ?? "Error al generar la consulta automática.", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Error inesperado al generar la consulta.", Severity.Error);
        }
        finally
        {
            IsCreatingConsultation = false;
            StateHasChanged();
        }
    }

    protected async Task OpenAntecedentModal()
    {
        var parameters = new DialogParameters<AntecedentModal>
        {
            { x => x.PatientId, Model.PatientId }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AntecedentModal>("Registrar Antecedente Clínico", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data != null)
        {
            var payload = result.Data as AntecedentRequestDTO;
            if (payload != null)
            {
                // Execute MediatR command
                var response = await Mediator.Send(new RegisterAntecedentCommand(payload));

                if (response != null && response.IsSuccess)
                {
                    Snackbar.Add("Antecedente registrado satisfactoriamente.", Severity.Success);

                    // Refresh data
                    await OnParametersSetAsync();
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add(response?.Message ?? "Error al registrar el antecedente clínico.", Severity.Error);
                }
            }
        }
    }

    protected async Task OpenConsentModal()
    {
        var parameters = new DialogParameters<ConsentModal>
        {
            { x => x.PatientId, Model.PatientId },
            { x => x.PatientPhone, "N/A" } // O puedes extraer un teléfono si se incluye en DemoGraphics a futuro
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ConsentModal>("Consentimiento Digital Externo", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data != null)
        {
            var payload = result.Data as ConsentRequestDTO;
            if (payload != null)
            {
                var response = await Mediator.Send(new RegisterConsentCommand(payload));
                if (response != null && response.IsSuccess)
                {
                    Snackbar.Add("Copia del enlace almacenada correctamente.", Severity.Success);
                    await OnParametersSetAsync();
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add(response?.Message ?? "Error al registrar el consentimiento.", Severity.Error);
                }
            }
        }
    }

    protected async Task OpenAlertModal()
    {
        var parameters = new DialogParameters<PatientAlertModal> { { x => x.PatientId, Model.PatientId } };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<PatientAlertModal>("Crear Alerta Médica", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is PatientAlertModal.AlertFormModel payload)
        {
            var req = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.CreatePatientAlertRequestDTO(
                Model.PatientId, payload.AlertTypeId, payload.SeverityId, payload.Message, 1, null
            ) { OrganizationId = AppState.OrganizationId };
            var res = await ExternalClient.PostAsync<object, MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientAlertResponseDTO>("api/Clinical/4115", req);
            
            if (res != null && res.IsSuccess)
            {
                Snackbar.Add("Alerta médica registrada correctamente.", Severity.Success);
                await LoadObservabilityData(Model.PatientId);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(res?.Message ?? "Error al crear la alerta.", Severity.Error);
            }
        }
    }

    protected async Task OpenContactModal()
    {
        var parameters = new DialogParameters<PatientContactModal>();
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<PatientContactModal>("Añadir Contacto", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is PatientContactModal.ContactFormModel payload)
        {
            var req = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.CreatePatientContactRequestDTO(
                Model.PatientId, 1, payload.FullName, payload.Phone, payload.Email, payload.RelationshipId, payload.IsPrimary
            ) { OrganizationId = AppState.OrganizationId };
            var res = await ExternalClient.PostAsync<object, MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientContactResponseDTO>("api/Clinical/4121", req);
            
            if (res != null && res.IsSuccess)
            {
                Snackbar.Add("Contacto registrado correctamente.", Severity.Success);
                await LoadObservabilityData(Model.PatientId);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(res?.Message ?? "Error al crear el contacto.", Severity.Error);
            }
        }
    }

    protected async Task AcknowledgeAlert(long alertId)
    {
        var req = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.AcknowledgeAlertRequestDTO(alertId) { OrganizationId = AppState.OrganizationId };
        var res = await ExternalClient.PostAsync<object, object>("api/Clinical/4116", req);
        
        if (res != null && res.IsSuccess)
        {
            Snackbar.Add("Alerta marcada como leída.", Severity.Success);
            await LoadObservabilityData(Model.PatientId);
            StateHasChanged();
        }
        else
        {
            Snackbar.Add(res?.Message ?? "Error al marcar alerta.", Severity.Error);
        }
    }
}

