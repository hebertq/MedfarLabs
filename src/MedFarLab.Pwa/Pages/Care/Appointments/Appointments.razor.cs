using MedFarLab.Application.Common.Services;
using MedFarLab.Application.Features.Care.Commands.RegisterAppointment;
using MedFarLab.Application.Features.Care.Models;
using MedFarLab.Pwa.Pages.Care.Appointments.Components;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;using System.Net.Http.Json;

namespace MedFarLab.Pwa.Pages.Care;

public class AppointmentsBase : ComponentBase
{
    [Inject] protected ISender Mediator { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected SyncManager SyncManager { get; set; } = default!;
    [Inject] protected MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected AppointmentVM Model = new(); 
    protected List<AppointmentModel> DailyAppointments = new();
    protected Dictionary<long, string> _patientNameCache = new(); 
    protected bool IsLoading;
    protected List<TimeSpan> TimeSlots = new();

    protected long? SelectedDoctorId { get; set; }
    protected List<MedFarLab.Application.Features.Identity.Models.DoctorListResponse> Doctors = new();
    
    protected override async Task OnInitializedAsync()
    {
        if (AppState.IsDoctor)
        {
            SelectedDoctorId = AppState.UserId;
        }
        else
        {
            await LoadDoctorsAsync();
            if (Doctors.Any())
            {
                SelectedDoctorId = Doctors.First().DoctorUserId;
            }
        }

        await LoadDailyAppointments();
    }

    protected async Task LoadDoctorsAsync()
    {
        try
        {
            var response = await Mediator.Send(new MedFarLab.Application.Features.Identity.Queries.GetDoctors.GetDoctorsQuery(AppState.OrganizationId));
            if (response != null)
            {
                Doctors = response;
            }
        }
        catch { }
    }

    protected async Task OnDoctorChanged(long? newDoctorId)
    {
        SelectedDoctorId = newDoctorId;
        await LoadDailyAppointments();
    }

    protected async Task OpenRegisterDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var parameters = new DialogParameters<RegisterAppointmentDialog>
        {
            { x => x.Model, new AppointmentVM { 
                Date = Model.Date, 
                SelectedTime = Model.SelectedTime,
                DoctorUserId = SelectedDoctorId ?? AppState.UserId 
            } }
        };

        var dialog = await DialogService.ShowAsync<RegisterAppointmentDialog>("Nueva Cita", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            var resultModel = result.Data as AppointmentVM;
            if (resultModel != null)
            {
                Model = resultModel;
                await OnValidSubmit(); 
            }
        }
    }

    protected async Task OnValidSubmit()
    {
        IsLoading = true;
        try
        {
            var response = await Mediator.Send(new RegisterAppointmentCommand(Model));

            if (response.IsSuccess)
            {
                var severity = response.Message!.Contains("OFFLINE") ? Severity.Warning : Severity.Success;
                Snackbar.Add(response.Message, severity);
                Model = new AppointmentVM(); 
                await LoadDailyAppointments();
            }
            else
            {
                Snackbar.Add(response.Message!, Severity.Error);
            }
        }
        finally
        {
            IsLoading = false;
            _ = SyncManager.ProcessPendingSync();
        }
    }

    protected async Task LoadDailyAppointments()
    {
        IsLoading = true;
        StateHasChanged();
        await Task.Delay(400);

        try
        {
            var requestDate = Model.Date ?? DateTime.Today;
            var response = await Mediator.Send(new MedFarLab.Application.Features.Care.Queries.GetDailyAppointmentsQuery 
            { 
                Date = requestDate, 
                DoctorUserId = SelectedDoctorId 
            });
            
            if (response != null)
            {
                DailyAppointments = response;
            }
            else 
            {
                DailyAppointments = new List<AppointmentModel>();
            }
        }
        catch 
        {
            DailyAppointments = new List<AppointmentModel>();
        }

        GenerateTimeSlots();
        IsLoading = false;
        StateHasChanged();
    }

    protected void ChangeDay(int days)
    {
        Model.Date = Model.Date?.AddDays(days);
        _ = LoadDailyAppointments();
    }

    protected void OnPatientSelected(long id) => Model.PatientId = id;

     protected Color GetStatusColor(AppointmentStatus status) => status switch
     {
         AppointmentStatus.Programada => Color.Info,
         AppointmentStatus.Completada => Color.Success,
         AppointmentStatus.Cancelada => Color.Error,
         AppointmentStatus.NoAsistio => Color.Warning,
         _ => Color.Default
     };

     protected async Task<IEnumerable<long>> SearchPatients(string searchText, CancellationToken cancellationToken)
     {
         await Task.Delay(100, cancellationToken); 
         return _patientNameCache.Keys
             .Where(id => _patientNameCache[id].Contains(searchText, StringComparison.OrdinalIgnoreCase))
             .Take(10);
     }

    private void GenerateTimeSlots()
    {
        TimeSlots.Clear();
        var start = new TimeSpan(8, 0, 0);
        var end = new TimeSpan(18, 0, 0);
        var nowToTimeSpan = DateTime.Now.TimeOfDay;

        while (start <= end)
        {
            var isPast = Model.Date == DateTime.Today && start < nowToTimeSpan.Subtract(TimeSpan.FromMinutes(30));
            var hasAppointment = DailyAppointments.Any(a => a.StartTime == start);
            
            // Only add slot if it's NOT in the past (unless it already holds an appointment)
            if (!(isPast && !hasAppointment) && Model.Date >= DateTime.Today)
            {
                TimeSlots.Add(start);
            }
            start = start.Add(TimeSpan.FromMinutes(30));
        }
    }

    protected async Task OnTimeSlotClick(TimeSpan slot, AppointmentModel? appointment)
    {
        if (appointment != null)
        {
            Snackbar.Add("Ya existe una cita en este horario", Severity.Info);
            return;
        }

        Model.SelectedTime = slot;
        await OpenRegisterDialog();
    }

    protected async Task OnAppointmentDropped(MudItemDropInfo<AppointmentModel> dropInfo)
    {
        if (dropInfo.Item == null) return;
        
        if (TimeSpan.TryParse(dropInfo.DropzoneIdentifier, out var targetTime))
        {
            if (DailyAppointments.Any(a => a.StartTime == targetTime && a.Id != dropInfo.Item.Id))
            {
                Snackbar.Add("Este horario ya está ocupado por otro paciente. Operación cancelada.", Severity.Warning);
                return;
            }

            var previousTime = dropInfo.Item.StartTime;
            dropInfo.Item.StartTime = targetTime;
            dropInfo.Item.Status = AppointmentStatus.Programada;
            StateHasChanged();
            
            var response = await Mediator.Send(new MedFarLab.Application.Features.Care.Commands.RescheduleAppointment.RescheduleAppointmentCommand(dropInfo.Item.Id, targetTime));
            if (response.IsSuccess)
            {
                Snackbar.Add($"Cita de {dropInfo.Item.PatientName} movida a las {targetTime.ToString(@"hh\:mm")}.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Error al guardar la reprogramación. Revirtiendo...", Severity.Error);
                dropInfo.Item.StartTime = previousTime;
                StateHasChanged();
            }
        }
    }
}