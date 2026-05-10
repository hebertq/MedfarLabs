using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using Microsoft.JSInterop;
using MedfarLabs.Core.Domain.Models.Reporting;
using MedFarLab.Application.Features.Reporting.DTOs;
using MedFarLab.Application.Features.Reporting.Queries.GetCareReport;
using System.Net.Http.Json;
namespace MedFarLab.Pwa.Pages.Care.Consultation
{
    public partial class ConsultationWorkspace : ComponentBase
    {

        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private MediatR.ISender Mediator { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private Microsoft.JSInterop.IJSRuntime JS { get; set; } = default!;   

        [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

        [Parameter] public long? AppointmentId { get; set; }
        [Parameter] public long? ConsultationId { get; set; }
        [SupplyParameterFromQuery] public bool ReadOnly { get; set; }
        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        public bool IsLoading { get; set; } = true;

        protected bool IsSubmitting { get; set; }
        
        // Mock UI state for patient banner
        protected string PatientName { get; set; } = "Cargando...";
        protected string PatientInitials { get; set; } = "Px";
        protected long MedicalRecordId { get; set; }
        protected long DoctorUserId { get; set; }

        // Native SOAP UI Binding Fields (to avoid mutability issues with Core DTO Records)
        protected string SubjectiveInput { get; set; } = string.Empty;
        protected string ObjectiveInput { get; set; } = string.Empty;
        protected string AnalysisInput { get; set; } = string.Empty;
        protected string PlanInput { get; set; } = string.Empty;
        protected string? CurrentDiagnosisCode { get; set; }

        public class DiagnosisCategory
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public List<DiagnosisCategory> DiagnosisCategories = new() {
            new DiagnosisCategory { Id = 77, Name = "Capítulo I: Enfermedades infecciosas y parasitarias" },
            new DiagnosisCategory { Id = 78, Name = "Capítulo II: Neoplasias (Cáncer)" },
            new DiagnosisCategory { Id = 79, Name = "Capítulo IV: Enfermedades endocrinas y metabólicas" },
            new DiagnosisCategory { Id = 168, Name = "Capítulo V: Trastornos mentales y del comportamiento" },
            new DiagnosisCategory { Id = 169, Name = "Capítulo VI: Enfermedades del sistema nervioso" },
            new DiagnosisCategory { Id = 80, Name = "Capítulo IX: Enfermedades del sistema circulatorio" },
            new DiagnosisCategory { Id = 81, Name = "Capítulo X: Enfermedades del sistema respiratorio" },
            new DiagnosisCategory { Id = 170, Name = "Capítulo XI: Enfermedades del aparato digestivo" },
            new DiagnosisCategory { Id = 171, Name = "Capítulo XIII: Enfermedades del sistema osteomuscular y del tejido conectivo" },
            new DiagnosisCategory { Id = 172, Name = "Capítulo XIV: Enfermedades del aparato genitourinario" }
        };

        protected List<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO> AvailableDiagnoses { get; set; } = new();

        public List<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO> Diagnoses { get; set; } = new();
        public List<PrescriptionItemDTO> Prescriptions { get; set; } = new();
        public List<LabOrderDTO> LabOrders { get; set; } = new();
        public DateTime? ScheduleNextAppointment { get; set; }
        public TimeSpan? NextAppointmentTime { get; set; }

        protected bool ShowScheduleModal { get; set; }
        protected DateTime? TempNextAppointmentDate { get; set; }
        protected TimeSpan? TempNextAppointmentTime { get; set; }

        
        // Medicine Form state (removed inline binding fields)
        // Lab Order Form state (removed inline binding fields)

        // Applied Products (Insumos aplicados en consulta)
        public List<AppliedProductItem> AppliedProducts { get; set; } = new();

        // Vitals Section
        protected VitalsState CurrentVitals { get; set; } = new();
        protected bool IsSavingVitals { get; set; }
        protected long PatientId { get; set; }

        // Chart Data (Heart Rate historic)
        public List<ChartSeries> VitalsSeries = new List<ChartSeries>();
        public string[] VitalsLabels = { "Dia 1", "Dia 5", "Dia 12", "Dia 18", "Dia 25", "Hoy" };

        protected void GoBack()
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                NavManager.NavigateTo(ReturnUrl);
            }
            else
            {
                NavManager.NavigateTo("/care/appointments");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (ConsultationId.HasValue)
                {
                    var response = await Mediator.Send(new MedFarLab.Application.Features.Care.Queries.GetConsultationDetails.GetConsultationDetailsQuery(ConsultationId.Value));
                    if (response.IsSuccess && response.Data != null)
                    {
                        MedicalRecordId = response.Data.MedicalRecordId;
                        PatientId = response.Data.PatientId;
                        DoctorUserId = response.Data.DoctorUserId;
                        PatientName = response.Data.PatientName;
                        PatientInitials = response.Data.PatientInitials;
                        
                        SubjectiveInput = response.Data.Subjective;
                        ObjectiveInput = response.Data.Objective;
                        AnalysisInput = response.Data.Analysis;
                        PlanInput = response.Data.Plan;

                        CurrentVitals.Systolic = response.Data.Systolic?.ToString() ?? "";
                        CurrentVitals.Diastolic = response.Data.Diastolic?.ToString() ?? "";
                        CurrentVitals.HeartRate = response.Data.HeartRate?.ToString() ?? "";
                        CurrentVitals.Temperature = response.Data.Temperature?.ToString() ?? "";
                        CurrentVitals.Weight = response.Data.Weight?.ToString() ?? "";

                        Prescriptions = response.Data.Prescriptions.ToList();
                        LabOrders = response.Data.LabOrders.ToList();
                        Diagnoses = response.Data.Diagnoses.ToList();

                        VitalsSeries.Clear();
                        VitalsSeries.Add(new ChartSeries() 
                        { 
                            Name = "Frec. Cardíaca", 
                            Data = new double[] { response.Data.HeartRate ?? 0 }
                        });
                    }
                    else
                    {
                        Snackbar.Add(response.Message ?? "Error al cargar la consulta", Severity.Error);
                        NavManager.NavigateTo($"/error/500?Message={Uri.EscapeDataString(response.Message ?? "Error interno al cargar la consulta.")}");
                    }
                }
                else if (AppointmentId.HasValue)
                {
                    var response = await Mediator.Send(new MedFarLab.Application.Features.Care.Queries.GetConsultationContext.GetConsultationContextQuery(AppointmentId.Value));
                    if (response.IsSuccess && response.Data != null)
                    {
                        PatientName = response.Data.PatientName;
                        PatientInitials = response.Data.PatientInitials;
                        MedicalRecordId = response.Data.MedicalRecordId;
                        PatientId = response.Data.PatientId;
                        DoctorUserId = response.Data.DoctorUserId;
                        
                        if (!string.IsNullOrWhiteSpace(response.Data.ReasonNotes))
                        {
                            SubjectiveInput = response.Data.ReasonNotes;
                        }

                        VitalsSeries.Clear();
                        VitalsSeries.Add(new ChartSeries() 
                        { 
                            Name = "Frec. Cardíaca", 
                            Data = response.Data.HistoricalVitalsHeartRate.ToArray() 
                        });
                    }
                    else
                    {
                        Snackbar.Add(response.Message ?? "Error al cargar contexto", Severity.Error);
                        NavManager.NavigateTo($"/error/500?Message={Uri.EscapeDataString(response.Message ?? "Error interno al inicializar el contexto de consulta.")}");
                    }
                }
            }
            catch (Exception ex)
            {
                var traceId = Guid.NewGuid().ToString().Substring(0,8);
                Snackbar.Add($"Excepción cargando datos [TraceId: {traceId}]: " + ex.Message, Severity.Error);
                NavManager.NavigateTo($"/error/500?Message={Uri.EscapeDataString(ex.Message)}");
            }
            IsLoading = false;
        }

        private int? _selectedCategoryId;
        protected int? SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (_selectedCategoryId != value)
                {
                    _selectedCategoryId = value;
                    CurrentDiagnosisCode = null;
                    AvailableDiagnoses.Clear();

                    if (value.HasValue)
                    {
                        _ = FetchDiagnosesForCategoryAsync(value.Value);
                    }
                }
            }
        }

        private async Task FetchDiagnosesForCategoryAsync(int categoryId)
        {
            try
            {
                var response = await Mediator.Send(new MedFarLab.Application.Features.Clinical.Queries.SearchDiagnoses.SearchDiagnosesQuery("", categoryId));
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    AvailableDiagnoses = response.Data.ToList();
                    // Snackbar.Add($"Se cargaron {AvailableDiagnoses.Count} diagnósticos para la categoría.", Severity.Info);
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    Snackbar.Add($"Error al cargar: {response?.Message ?? "Respuesta vacía o fallida"}", Severity.Warning);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error crítico en carga: {ex.Message}", Severity.Error);
            }
        }

        protected async Task AddDiagnosis()
        {
            if (SelectedCategoryId == null)
            {
                Snackbar.Add("Debe seleccionar una Categoría de Patología.", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(CurrentDiagnosisCode))
            {
                Snackbar.Add("Debe seleccionar una Patología Específica.", Severity.Warning);
                return;
            }

            var diagnosisObj = AvailableDiagnoses.FirstOrDefault(x => x.Code == CurrentDiagnosisCode);
            if (diagnosisObj == null) return;

            var parameters = new DialogParameters { ["Diagnosis"] = diagnosisObj };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<Dialogs.DiagnosisDialog>("Añadir Diagnóstico", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO diagnosisEntry)
            {
                if (!Diagnoses.Any(d => d.Id == diagnosisEntry.Id))
                    Diagnoses.Add(diagnosisEntry);
                    
                CurrentDiagnosisCode = null;
                SelectedCategoryId = null;
                AvailableDiagnoses.Clear();
            }
        }

        protected async Task<IEnumerable<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO>> SearchDiagnosesApi(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
                return Array.Empty<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO>();

            var response = await Mediator.Send(new MedFarLab.Application.Features.Clinical.Queries.SearchDiagnoses.SearchDiagnosesQuery(value ?? string.Empty), token);
            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            return Array.Empty<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO>();
        }

        protected async Task OpenPrescriptionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<Dialogs.PrescriptionDialog>("Añadir Medicamento", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is PrescriptionItemDTO data)
            {
                // Poka-Yoke: Cross validation for allergies
                bool hasAllergyMatch = PatientAllergies.Any(a => data.MedicationName.Contains(a, StringComparison.OrdinalIgnoreCase));
                if (hasAllergyMatch)
                {
                    bool? confirmResult = await DialogService.ShowMessageBox(
                        "⚠️ Alerta de Alergia Cruzada", 
                        $"El paciente tiene una alergia registrada que coincide con el medicamento '{data.MedicationName}'. ¿Está seguro de que desea recetarlo bajo su responsabilidad clínica?",
                        yesText: "Sí, recetar bajo mi riesgo", cancelText: "Cancelar"
                    );

                    if (confirmResult != true)
                    {
                        return; // The doctor cancelled
                    }
                }

                // To display it in the grid with a brand new instace:
                Prescriptions.Add(new PrescriptionItemDTO(
                    data.MedicationName, data.Dosage, data.Frequency, data.Duration, data.Instructions
                ));
            }
        }

        protected async Task EditPrescription(PrescriptionItemDTO item)
        {
            var parameters = new DialogParameters { ["Model"] = item with { } }; // Clone it conceptually, but we can mutate it or pass a copy.
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            
            var dialog = await DialogService.ShowAsync<Dialogs.PrescriptionDialog>("Editar Medicamento", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is PrescriptionItemDTO updated)
            {
                var index = Prescriptions.IndexOf(item);
                if (index != -1)
                {
                    Prescriptions[index] = updated;
                }
            }
        }

        protected async Task DeletePrescription(PrescriptionItemDTO item)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirmar", 
                "¿Está seguro que desea eliminar este medicamento de la receta?", 
                yesText: "Sí", cancelText: "No");

            if (result == true)
            {
                Prescriptions.Remove(item);
            }
        }

        protected async Task OpenLabOrderDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<Dialogs.LabOrderDialog>("Añadir Orden Clínico", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is LabOrderDTO data)
            {
                LabOrders.Add(new LabOrderDTO(data.TestName, data.Notes));
            }
        }

        protected async Task EditLabOrder(LabOrderDTO item)
        {
            var parameters = new DialogParameters { ["Model"] = item with { } }; 
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            
            var dialog = await DialogService.ShowAsync<Dialogs.LabOrderDialog>("Editar Orden Clínico", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is LabOrderDTO updated)
            {
                var index = LabOrders.IndexOf(item);
                if (index != -1)
                {
                    LabOrders[index] = updated;
                }
            }
        }

        protected async Task DeleteLabOrder(LabOrderDTO item)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirmar", 
                "¿Está seguro que desea eliminar este examen de la orden?", 
                yesText: "Sí", cancelText: "No");

            if (result == true)
            {
                LabOrders.Remove(item);
            }
        }

        protected async Task OpenAppliedProductDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<Dialogs.AppliedProductDialog>("Añadir Insumo Aplicado", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is Dialogs.AppliedProductDialog.ProductModel data)
            {
                AppliedProducts.Add(new AppliedProductItem(data.ProductName, data.Quantity));
            }
        }

        protected async Task EditAppliedProduct(AppliedProductItem item)
        {
            var clone = new Dialogs.AppliedProductDialog.ProductModel { ProductName = item.ProductName, Quantity = item.Quantity };
            var parameters = new DialogParameters { ["Model"] = clone };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            
            var dialog = await DialogService.ShowAsync<Dialogs.AppliedProductDialog>("Editar Insumo", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is Dialogs.AppliedProductDialog.ProductModel updated)
            {
                var index = AppliedProducts.IndexOf(item);
                if (index != -1)
                {
                    AppliedProducts[index] = new AppliedProductItem(updated.ProductName, updated.Quantity);
                }
            }
        }

        protected async Task DeleteAppliedProduct(AppliedProductItem item)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirmar", 
                "¿Está seguro que desea eliminar este insumo aplicado?", 
                yesText: "Sí", cancelText: "No");

            if (result == true)
            {
                AppliedProducts.Remove(item);
            }
        }

        protected void OpenScheduleModal()
        {
            TempNextAppointmentDate = ScheduleNextAppointment ?? DateTime.Today.AddDays(7);
            TempNextAppointmentTime = NextAppointmentTime ?? new TimeSpan(10, 0, 0);
            ShowScheduleModal = true;
        }

        protected void CancelScheduleModal()
        {
            ShowScheduleModal = false;
        }

        protected void ConfirmScheduleModal()
        {
            ScheduleNextAppointment = TempNextAppointmentDate;
            NextAppointmentTime = TempNextAppointmentTime;
            ShowScheduleModal = false;
        }

        protected async Task CancelConsultation()
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                NavManager.NavigateTo(ReturnUrl);
            }
            else
            {
                await JS.InvokeVoidAsync("history.back");
            }
        }

        protected async Task SubmitConsultation(bool isDraft = false)
        {
            // Auto-agregar diagnóstico que se quedó seleccionado pero no le dieron al botón Agregar
            if (!string.IsNullOrEmpty(CurrentDiagnosisCode))
            {
                var uncommittedDiagnosis = AvailableDiagnoses.FirstOrDefault(x => x.Code == CurrentDiagnosisCode);
                if (uncommittedDiagnosis != null && !Diagnoses.Any(x => x.Id == uncommittedDiagnosis.Id))
                {
                    Diagnoses.Add(uncommittedDiagnosis);
                }
            }

            if (!isDraft && (Diagnoses == null || !Diagnoses.Any()))
            {
                Snackbar.Add("Debe agregar al menos un diagnóstico (Categoría y Patología) para poder finalizar la consulta.", Severity.Warning);
                return;
            }

            IsSubmitting = true;
            try
            {
                // Concat UI Features into the strings for the Core DTO (keeping them plain string just in case they're forced required by rules)
                var finalObjective = ObjectiveInput;
                var finalAnalysis = AnalysisInput;
                var finalPlan = PlanInput;
                
                // Parse Vitals
                decimal.TryParse(CurrentVitals.Systolic, out var sys);
                decimal.TryParse(CurrentVitals.Diastolic, out var dia);
                int.TryParse(CurrentVitals.HeartRate, out var hr);
                decimal.TryParse(CurrentVitals.Temperature, out var temp);
                decimal.TryParse(CurrentVitals.Weight, out var weight);

                var vitalsDto = new VitalSignsDTO(
                    SystolicPressure: sys > 0 ? sys : null,
                    DiastolicPressure: dia > 0 ? dia : null,
                    HeartRate: hr > 0 ? hr : null,
                    Temperature: temp > 0 ? temp : null,
                    WeightKg: weight > 0 ? weight : null
                );

                // 100% Core DTO Re-Usability (Immutable Record from MedfarLabs.Core)
                var dto = new ConsultationRequestDTO(
                    ConsultationId: this.ConsultationId, // Send ConsultationId if it exists to trigger UPSERT logic
                    MedicalRecordId: MedicalRecordId,
                    DoctorUserId: DoctorUserId,
                    Subjective: SubjectiveInput,
                    Objective: finalObjective,
                    Analysis: finalAnalysis,
                    Plan: finalPlan,
                    Vitals: vitalsDto,
                    Diagnoses: Diagnoses,
                    Prescriptions: Prescriptions,
                    LabOrders: LabOrders
                );

                var apiResponse = await Mediator.Send(new MedFarLab.Application.Features.Care.Commands.RegisterConsultation.RegisterConsultationCommand(dto));
                
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    long consultationId = apiResponse.Data;
                    
                    // Always capture the returning consultationId so further "Guadar Borrador" clicks UPDATE instead of duplicating!
                    this.ConsultationId = consultationId;

                    if (isDraft)
                    {
                        Snackbar.Add("Borrador guardado exitosamente.", Severity.Success, config => { config.ActionColor = Color.Primary; config.Icon = Icons.Material.Filled.Save; });
                        IsSubmitting = false;
                        return; // Prevent redirecting on draft
                    }
                    else
                    {
                        // Si no es borrador, enviamos el comando para cerrar la consulta (CerrarConsulta)
                        var closeDto = new MedfarLabs.Core.Application.Features.Care.Dtos.Request.CloseConsultationRequestDTO(consultationId);
                        var closeResponse = await Mediator.Send(new MedFarLab.Application.Features.Care.Commands.CloseConsultation.CloseConsultationCommand(closeDto));
                        
                        if (closeResponse != null && closeResponse.IsSuccess)
                        {
                            Snackbar.Add("Consulta finalizada y cerrada exitosamente.", Severity.Success, config => { config.ActionColor = Color.Primary; config.Icon = Icons.Material.Filled.CheckCircle; });
                        }
                        else
                        {
                            // A pesar de registrarla, falló al cerrarla
                            Snackbar.Add("Consulta guardada pero falló al cerrarla: " + (closeResponse?.Message ?? "Error desconocido"), Severity.Warning);
                        }
                    }
                }
                else
                {
                    if (apiResponse != null && apiResponse.Errors != null && apiResponse.Errors.Any())
                    {
                        foreach (var error in apiResponse.Errors)
                        {
                            Snackbar.Add(error, Severity.Warning);
                        }
                    }
                    else
                    {
                        Snackbar.Add(apiResponse?.Message ?? "Fallo al conectar con el servidor", Severity.Error);
                    }
                    return;
                }
                
                if (LabOrders.Any())
                {
                    Snackbar.Add("Órdenes de Laboratorio auto-escaladas exitosamente.", Severity.Info);
                }
                if (ScheduleNextAppointment.HasValue)
                {
                    Snackbar.Add($"Cita automática programada para: {ScheduleNextAppointment.Value.ToString("dd/MM/yyyy")}", Severity.Info);
                }
                
                // Exit to Dashboard
                await Task.Delay(2000);
                NavManager.NavigateTo("/clinical/dashboard");
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        protected async Task SaveVitals()
        {
            IsSavingVitals = true;
            StateHasChanged();

            try 
            {
                // Parse the string values locally
                _ = decimal.TryParse(CurrentVitals.Systolic, out decimal sys);
                _ = decimal.TryParse(CurrentVitals.Diastolic, out decimal dia);
                _ = decimal.TryParse(CurrentVitals.HeartRate, out decimal hr);
                _ = decimal.TryParse(CurrentVitals.Temperature, out decimal temp);
                _ = decimal.TryParse(CurrentVitals.Weight, out decimal weight);

                var payload = new MedfarLabs.Core.Application.Features.Clinical.Dtos.Request.VitalSignsRequestDTO
                {
                    PatientId = this.PatientId,
                    Systolic = (int)sys,
                    Diastolic = (int)dia,
                    HeartRate = (int)hr,
                    Temperature = temp,
                    Weight = weight,
                    Height = 0m
                };

                var command = new MedFarLab.Application.Features.Clinical.Commands.RegisterVitals.RegisterVitalsCommand(payload);
                var response = await Mediator.Send(command);

                if (response != null && response.IsSuccess)
                {
                    Snackbar.Add("Signos vitales guardados exitosamente.", Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Error: {response?.Message}", Severity.Error);
                }
            }
            catch(Exception ex)
            {
                Snackbar.Add($"Excepción guardando signos: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsSavingVitals = false;
                StateHasChanged();
            }
        }

        public class VitalsState
        {
            public string Systolic { get; set; } = string.Empty;
            public string Diastolic { get; set; } = string.Empty;
            public string HeartRate { get; set; } = string.Empty;
            public string Temperature { get; set; } = string.Empty;
            public string Weight { get; set; } = string.Empty;
        }

        public record AppliedProductItem(string ProductName, int Quantity);

        protected bool IsPrinting { get; set; }
        protected string PrescriptionPrintFormat { get; set; } = "A4";
        protected string LabOrderPrintFormat { get; set; } = "A4";

        // Poka-Yoke / UI Mock Data for Clinical Safety Phase
        public List<string> PatientAllergies { get; set; } = new() { "Penicilina" };
        public List<string> PatientRisks { get; set; } = new() { "Hipertensión Severa" };

        protected async Task PrintPrescriptionPDF()
        {
            if (!Prescriptions.Any())
            {
                Snackbar.Add("No hay medicamentos para imprimir.", Severity.Warning);
                return;
            }

            try
            {
                IsPrinting = true;
                StateHasChanged();

                var model = new PrescriptionReportModel
                {
                    PatientName = this.PatientName,
                    DoctorName = "Dr. MedFarLab",
                    MedicalRecordId = this.MedicalRecordId,
                    Date = DateTime.Now,
                    Format = PrescriptionPrintFormat,
                    OrganizationInfo = AppState.OrganizationInfo,
                    Items = Prescriptions.Select(p => new PrescriptionReportItem
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        Duration = p.Duration,
                        Instructions = p.Instructions
                    }).ToList()
                };

                // Generación de PDF delegada al Mediator
                var response = await Mediator.Send(new MedFarLab.Application.Features.Reporting.Commands.GeneratePrescriptionPDF.GeneratePrescriptionPDFCommand(model));

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    await TriggerFileDownload(response.Data.FileName, response.Data.MimeType, response.Data.Base64Data);
                }
                else
                {
                    Snackbar.Add("Error generando reporte: " + response?.Message, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Error en la solicitud: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsPrinting = false;
                StateHasChanged();
            }
        }

        protected async Task PrintLabOrderPDF()
        {
            if (!LabOrders.Any())
            {
                Snackbar.Add("No hay órdenes de laboratorio para imprimir.", Severity.Warning);
                return;
            }

            try
            {
                IsPrinting = true;
                StateHasChanged();

                var model = new LabOrderReportModel
                {
                    PatientName = this.PatientName,
                    DoctorName = "Dr. MedFarLab",
                    MedicalRecordId = this.MedicalRecordId,
                    Date = DateTime.Now,
                    Format = LabOrderPrintFormat,
                    OrganizationInfo = AppState.OrganizationInfo,
                    Orders = LabOrders.Select(l => new LabOrderReportItem
                    {
                        TestName = l.TestName,
                        Notes = l.Notes
                    }).ToList()
                };

                // Generación de PDF delegada al Mediator
                var response = await Mediator.Send(new MedFarLab.Application.Features.Reporting.Commands.GenerateLabOrderPDF.GenerateLabOrderPDFCommand(model));

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    await TriggerFileDownload(response.Data.FileName, response.Data.MimeType, response.Data.Base64Data);
                }
                else
                {
                    Snackbar.Add("Error generando reporte: " + response?.Message, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Error en la solicitud: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsPrinting = false;
                StateHasChanged();
            }
        }

        private async Task TriggerFileDownload(string fileName, string contentType, string base64Data)
        {
            // JS Interop logic to download base64 encoded file
            var jsCode = $@"
                function downloadFromByteArray(fileName, contentType, byteBase64) {{
                    const link = document.createElement('a');
                    link.download = fileName;
                    link.href = 'data:' + contentType + ';base64,' + byteBase64;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                }}
            ";
            
            // Execute eval directly for simplicity here (or better to place in index.html)
            await JS.InvokeVoidAsync("eval", jsCode);
            await JS.InvokeVoidAsync("downloadFromByteArray", fileName, contentType, base64Data);
        }
    }
}

