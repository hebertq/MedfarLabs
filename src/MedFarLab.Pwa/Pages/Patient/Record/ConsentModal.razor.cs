using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;

namespace MedFarLab.Pwa.Pages.Patient.Record;

public partial class ConsentModal : ComponentBase
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    [CascadingParameter] protected MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public long PatientId { get; set; }
    
    [Parameter]
    public string PatientPhone { get; set; } = string.Empty;

    public class ConsentFormModel 
    {
        public int? ConsentTypeId { get; set; }
        public string DigitalFormUrl { get; set; } = string.Empty;
    }

    public ConsentFormModel Model { get; set; } = new ConsentFormModel();

    protected void OnTypeChanged(int? typeId)
    {
        Model.ConsentTypeId = typeId;
        
        // Simulación de plantillas pre-configuradas de JotForm para la Clínica
        if (typeId > 0 && string.IsNullOrWhiteSpace(Model.DigitalFormUrl) || Model.DigitalFormUrl.Contains("form.jotform.com"))
        {
            string formId = typeId switch
            {
                1 => "230000000000010", // Tratamiento
                2 => "230000000000020", // Cirugía
                3 => "230000000000030", // Datos Personales
                _ => "230000000000040"  // Laboratorios
            };
            
            // Construimos la rúbrica autocompletando el ID del paciente si el Form lo permite por querystring
            Model.DigitalFormUrl = $"https://form.jotform.com/{formId}?patientId={PatientId}";
        }
    }

    protected async Task ShareViaWhatsApp()
    {
        if (string.IsNullOrWhiteSpace(Model.DigitalFormUrl)) return;
        
        string phone = string.IsNullOrWhiteSpace(PatientPhone) || PatientPhone == "N/A" ? "" : PatientPhone;
        string msg = Uri.EscapeDataString($"Hola, por favor ayúdenos completando el siguiente Consentimiento Informado para su expediente médico: {Model.DigitalFormUrl}");
        
        string waUrl = string.IsNullOrWhiteSpace(phone) ? $"https://wa.me/?text={msg}" : $"https://wa.me/{phone}?text={msg}";
        await JSRuntime.InvokeVoidAsync("window.open", waUrl, "_blank");
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void Submit()
    {
        if (Model.ConsentTypeId.HasValue && Model.ConsentTypeId > 0 && !string.IsNullOrWhiteSpace(Model.DigitalFormUrl))
        {
            var payload = new ConsentRequestDTO(PatientId, Model.ConsentTypeId.Value, Model.DigitalFormUrl);
            MudDialog.Close(DialogResult.Ok(payload));
        }
    }
}

