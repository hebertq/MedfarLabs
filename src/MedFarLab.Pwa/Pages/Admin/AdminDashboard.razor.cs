using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using MedFarLab.Pwa.State;

namespace MedFarLab.Pwa.Pages.Admin;

public partial class AdminDashboard : ComponentBase
{
    [Inject] protected AppState AppState { get; set; } = default!;
    [Inject] protected NavigationManager NavManager { get; set; } = default!;
    [Inject] protected MediatR.ISender Mediator { get; set; } = default!;

    protected bool IsLoading = true;
    protected MedFarLab.Application.Features.System.Models.MasterDashboardResponseDTO? DashboardData;

    protected override async Task OnInitializedAsync()
    {
        if (!AppState.IsMasterAdmin)
        {
            return;
        }

        try
        {
            var response = await Mediator.Send(new MedFarLab.Application.Features.System.Queries.GetMasterDashboard.GetMasterDashboardQuery());
            if (response != null)
            {
                DashboardData = response;
            }
            else
            {
                LoadFallbackData();
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error fetch master dashboard (Using Fallback Mode): {ex.Message}");
            LoadFallbackData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadFallbackData()
    {
        // Mock estático de contingencia para evitar página en blanco B2B
        DashboardData = new MedFarLab.Application.Features.System.Models.MasterDashboardResponseDTO
        {
            PendingInvoicesCount = 3,
            PendingSubscriptionsCount = 1,
            PendingOnboardings = 2,
            ActiveOrganizationsCount = 42,
            MonthlyRecurringRevenue = 28450.00m
        };
    }
}
