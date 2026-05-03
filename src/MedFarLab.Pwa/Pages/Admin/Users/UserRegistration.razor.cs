using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;

namespace MedFarLab.Pwa.Pages.Admin.Users;

public partial class UserRegistration : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    
    protected MudForm form = default!;
    protected bool isFormValid;
    protected bool isSaving;
    protected DateTime? selectedDate = DateTime.Today.AddYears(-25);

    public CreateAppUserRequestDTO Model { get; set; } = new CreateAppUserRequestDTO();

    protected async Task SaveUserAsync()
    {
        await form.Validate();
        if (!form.IsValid) return;

        isSaving = true;
        StateHasChanged();

        if (selectedDate.HasValue) Model.BirthDate = selectedDate.Value;
        
        // Simular consumo de API POST /api/Identity/Users/AppUser
        await Task.Delay(1500);

        isSaving = false;
        Snackbar.Add("¡El usuario ha sido registrado y puede iniciar sesión ahora!", Severity.Success);
        
        // Reset form
        Model = new CreateAppUserRequestDTO();
        selectedDate = DateTime.Today.AddYears(-25);
        await form.ResetAsync();
    }
}
