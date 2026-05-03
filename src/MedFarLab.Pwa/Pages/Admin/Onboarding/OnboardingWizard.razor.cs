using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Net.Http.Json;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;

namespace MedFarLab.Pwa.Pages.Admin.Onboarding;

public partial class OnboardingWizard : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected MediatR.ISender Mediator { get; set; } = default!;
    
    protected int _index;
    protected bool IsSubmitting;
    protected bool IsSuccess;
    protected string ErrorMessage = "";
    protected MudForm form1 = default!;
    protected MudForm form2 = default!;

    public class OnboardingFormModel
    {
        public string Name { get; set; } = "";
        public string TaxId { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string BranchAddress { get; set; } = "";
        public long PlanId { get; set; } = 1;
        public string LogoBase64 { get; set; } = "";
    }

    protected OnboardingFormModel FormModel = new();

    protected async Task UploadLogo(IBrowserFile file)
    {
        try
        {
            if (file != null)
            {
                var maxAllowedSize = 512000; // 500 KB limit for Base64 injection
                if (file.Size > maxAllowedSize)
                {
                    Snackbar.Add("El logo debe pesar máximo 500KB.", Severity.Warning);
                    return;
                }

                using var stream = file.OpenReadStream(maxAllowedSize);
                var buffer = new byte[file.Size];
                await stream.ReadAsync(buffer);
                FormModel.LogoBase64 = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al procesar logo: {ex.Message}", Severity.Error);
        }
    }

    protected async Task NextStep()
    {
        bool isValid = true;
        if (_index == 0)
        {
            await form1.Validate();
            isValid = form1.IsValid;
        }
        else if (_index == 1)
        {
            await form2.Validate();
            isValid = form2.IsValid;
        }

        if (isValid)
            _index++;
    }

    protected void PreviousStep()
    {
        _index--;
    }

    protected async Task SubmitForm()
    {
        IsSubmitting = true;
        ErrorMessage = "";
        StateHasChanged();

        try
        {
            var dto = new OrganizationRequestDTO(
                Name: FormModel.Name,
                TaxId: FormModel.TaxId,
                AuditNotes: "Auto-generado desde Workflow PWA Premium",
                IsActive: true,
                BranchName: FormModel.BranchName,
                BranchAddress: FormModel.BranchAddress,
                PlanId: FormModel.PlanId,
                LogoBase64: FormModel.LogoBase64
            );

            // API Dispatcher ActionCode 2005 = Identity.RegistrarOrganizacion
            var response = await Mediator.Send(new MedFarLab.Application.Features.Identity.Commands.RegisterOrganization.RegisterOrganizationCommand(dto));
            if (response != null && response.IsSuccess)
            {
                IsSuccess = true;
                StateHasChanged();

                await Task.Delay(3500);
                Navigation.NavigateTo("/admin/organizations");
            }
            else
            {
                ErrorMessage = $"El Motor Backend devolvió un error: {response?.Message}. Verifique duplicados o disponibilidad del SaaS API.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Fallo de conexión persistente con MedFarLabs.Core: " + ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
