using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;
using MedFarLab.Application.Features.Billing.Queries;
using MedFarLab.Application.Features.Billing.Commands;
using MediatR;

namespace MedFarLab.Pwa.Pages.Admin.Plans;

public partial class PlansAdmin : ComponentBase
{
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IMediator Mediator { get; set; } = default!;

    public List<SaasPlanResponseDTO> Plans { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadPlans();
    }

    private async Task LoadPlans()
    {
        var response = await Mediator.Send(new GetSaasPlansQuery());
        if (response != null)
        {
            Plans = response.OrderBy(x => x.SortOrder).ToList();
        }
    }

    protected void CreateNewPlan()
    {
        var newPlan = new UpdateSaasPlanCommand { Name = "Nuevo Plan", Features = new List<string>() };
        OpenPlanDialog(newPlan, isNew: true);
    }

    protected async Task OpenPlanDialog(UpdateSaasPlanCommand plan, bool isNew = false)
    {
        var parameters = new DialogParameters<PlanEditorDialog>
        {
            { x => x.PlanToEdit, plan },
            { x => x.IsNew, isNew }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<PlanEditorDialog>(isNew ? "Crear Nuevo Plan" : "Editar Plan SaaS", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var editedPlan = (UpdateSaasPlanCommand)result.Data;
            
            if (isNew)
            {
                var createCommand = new CreateSaasPlanCommand
                {
                    Name = editedPlan.Name,
                    Description = editedPlan.Description,
                    MonthlyPrice = editedPlan.MonthlyPrice,
                    AnnualPrice = editedPlan.AnnualPrice,
                    MaxBranches = editedPlan.MaxBranches,
                    MaxUsers = editedPlan.MaxUsers,
                    IncludedUsers = editedPlan.IncludedUsers,
                    IncludedBranches = editedPlan.IncludedBranches,
                    PricePerExtraUser = editedPlan.PricePerExtraUser,
                    PricePerBranch = editedPlan.PricePerBranch,
                    PricePerConsultation = editedPlan.PricePerConsultation,
                    OrganizationTypeId = editedPlan.OrganizationTypeId,
                    IsPayPerUse = editedPlan.IsPayPerUse,
                    GraceDays = editedPlan.GraceDays,
                    IsFeatured = editedPlan.IsFeatured,
                    SortOrder = editedPlan.SortOrder,
                    Features = editedPlan.Features
                };
                
                var response = await Mediator.Send(createCommand);
                if (response.IsSuccess)
                {
                    Snackbar.Add("Nuevo plan creado exitosamente.", Severity.Success);
                    await LoadPlans();
                }
                else
                {
                    Snackbar.Add($"Error: {response.Message}", Severity.Error);
                }
            }
            else
            {
                var response = await Mediator.Send(editedPlan);
                if (response.IsSuccess)
                {
                    Snackbar.Add("Plan actualizado exitosamente.", Severity.Success);
                    await LoadPlans();
                }
                else
                {
                    Snackbar.Add($"Error: {response.Message}", Severity.Error);
                }
            }
        }
    }

    protected void EditExistingPlan(SaasPlanResponseDTO plan)
    {
        var editCommand = new UpdateSaasPlanCommand
        {
            PlanId = (int)plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice,
            AnnualPrice = plan.AnnualPrice,
            MaxBranches = plan.MaxBranches,
            MaxUsers = plan.MaxUsers,
            IncludedUsers = plan.IncludedUsers,
            IncludedBranches = plan.IncludedBranches,
            PricePerExtraUser = plan.PricePerExtraUser,
            PricePerBranch = plan.PricePerBranch,
            PricePerConsultation = plan.PricePerConsultation,
            OrganizationTypeId = plan.OrganizationTypeId,
            IsPayPerUse = plan.IsPayPerUse,
            GraceDays = plan.GraceDays,
            IsFeatured = plan.IsFeatured,
            SortOrder = plan.SortOrder,
            Features = plan.Features,
            IsActive = true // or infer from response if there is an IsActive property in response
        };
        OpenPlanDialog(editCommand, isNew: false);
    }
}
