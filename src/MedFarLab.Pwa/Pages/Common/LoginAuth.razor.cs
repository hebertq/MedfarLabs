using Microsoft.AspNetCore.Components;
using MedFarLab.Pwa.State;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using MediatR;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Common
{
    public partial class LoginAuth : ComponentBase
    {
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private AppState AppState { get; set; } = default!;
        [Inject] private ISender Mediator { get; set; } = default!;
        [Inject] private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private Microsoft.Extensions.Configuration.IConfiguration Config { get; set; } = default!;

        protected int CurrentStep { get; set; } = 1;
        protected bool IsAuthenticating { get; set; } = false;
        protected string SelectedTenant { get; set; } = "clinica";
        protected string InstitutionName { get; set; } = string.Empty;
        protected string Username { get; set; } = "masteradmin";
        protected string Password { get; set; } = "root765*";

        protected void GoNextStep()
        {
            CurrentStep = 2;
        }
        
        protected void GoPrevStep()
        {
            CurrentStep = 1;
        }

        protected void OnDemoUserChanged(string demoUser)
        {
            Username = demoUser;
            Password = "root765*";
            
            if (demoUser == "labadmin") SelectedTenant = "laboratorio";
            else if (demoUser == "pharmacyadmin") SelectedTenant = "farmacia";
            else if (demoUser == "clinicadmin" || demoUser == "fulladmin" || demoUser == "masteradmin") SelectedTenant = "clinica";
            
            StateHasChanged();
        }

        protected async Task HandleLogin()
        {
            IsAuthenticating = true;
            StateHasChanged();

            try
            {
                var result = await Mediator.Send(new MedFarLab.Application.Features.Identity.Commands.Authenticate.AuthenticateCommand(Username, Password));

                if (result != null)
                {
                    AppState.UserId = result.UserId;
                    AppState.OrganizationId = result.OrganizationId;
                    AppState.BranchId = result.BranchId;
                    AppState.FullName = result.Username;
                    AppState.SessionToken = result.Token;
                    AppState.IsDoctor = result.IsDoctor;
                    
                    AppState.IsMasterAdmin = result.IsMasterAdmin;

                    AppState.UserRoles = result.Roles ?? new List<string>();
                    AppState.PlanModules = result.Modules ?? new List<string>();

                    AppState.OrganizationInfo.Name = result.OrganizationName ?? "";
                    AppState.OrganizationInfo.Address = result.OrganizationAddress ?? "";
                    AppState.OrganizationInfo.Phone = result.OrganizationPhone ?? "";
                    AppState.OrganizationInfo.Email = result.OrganizationEmail ?? "";
                    AppState.OrganizationInfo.LogoUrl = result.OrganizationLogoUrl ?? "";

                    // Persist to local storage
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_token", AppState.SessionToken);
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_userId", AppState.UserId.ToString());
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_branchId", AppState.BranchId.ToString());
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_isDoctor", AppState.IsDoctor.ToString());
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_isMaster", AppState.IsMasterAdmin.ToString());
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_roles", System.Text.Json.JsonSerializer.Serialize(AppState.UserRoles));
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_modules", System.Text.Json.JsonSerializer.Serialize(AppState.PlanModules));
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_orginfo", System.Text.Json.JsonSerializer.Serialize(AppState.OrganizationInfo));
                    
                    if (AppState.IsMasterAdmin)
                    {
                        AppState.CurrentTenantRoute = "admin";
                        NavManager.NavigateTo("/admin/dashboard");
                    }
                    else
                    {
                        // Redirigir al dashboard específico basado en el Módulo principal
                        AppState.CurrentTenantRoute = SelectedTenant switch
                        {
                            "laboratorio" => "laboratory",
                            "farmacia" => "pharmacy",
                            _ => "clinical"
                        };
                        
                        // Set the OrganizationType ID mapping roughly based on tenant for the dynamic menu
                        int orgTypeId = SelectedTenant switch
                        {
                            "laboratorio" => 15, // LAB catalog id exactly from medfarlab_db
                            "farmacia" => 125, // PHA
                            _ => 124 // CLI
                        };
                        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "medfarlab_orgtype", orgTypeId.ToString());
                        
                        NavManager.NavigateTo("/home");
                    }
                }
                else
                {
                    // Handle login failure
                    Snackbar.Add("Credenciales incorrectas o sesión inválida.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error de conexión con el servidor: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsAuthenticating = false;
                StateHasChanged();
            }
        }
    }
}
