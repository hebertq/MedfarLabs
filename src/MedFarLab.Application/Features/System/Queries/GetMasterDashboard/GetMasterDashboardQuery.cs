using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.System.Models;

namespace MedFarLab.Application.Features.System.Models
{
    public class MasterDashboardResponseDTO
    {
        public int PendingInvoicesCount { get; set; }
        public int PendingSubscriptionsCount { get; set; }
        public int PendingOnboardings { get; set; }
        public int ActiveOrganizationsCount { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
    }
}

namespace MedFarLab.Application.Features.System.Queries.GetMasterDashboard
{
    public class GetMasterDashboardQuery : IRequest<MasterDashboardResponseDTO?>
    {
    }

    public class GetMasterDashboardQueryHandler : IRequestHandler<GetMasterDashboardQuery, MasterDashboardResponseDTO?>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetMasterDashboardQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<MasterDashboardResponseDTO?> Handle(GetMasterDashboardQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.GetAsync<MasterDashboardResponseDTO>("/api/Dashboard/master");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return null;
        }
    }
}
