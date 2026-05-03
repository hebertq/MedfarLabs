using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.System.Models
{
    public class TenantDashboardResponseDTO
    {
        public decimal TotalRevenueThisMonth { get; set; }
        public int TotalAppointmentsToday { get; set; }
        public int PatientsWaiting { get; set; }
        public int DoctorsOnline { get; set; }
        public List<PatientShortInfo> LatestPatients { get; set; } = new();
        public List<decimal> RevenueTrend { get; set; } = new();
    }

    public class PatientShortInfo
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public string Time { get; set; } = "";
    }
}

namespace MedFarLab.Application.Features.System.Queries.GetTenantDashboard
{
    public class GetTenantDashboardQuery : IRequest<Models.TenantDashboardResponseDTO?>
    {
        public long OrganizationId { get; set; }

        public GetTenantDashboardQuery(long organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class GetTenantDashboardQueryHandler : IRequestHandler<GetTenantDashboardQuery, Models.TenantDashboardResponseDTO?>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetTenantDashboardQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Models.TenantDashboardResponseDTO?> Handle(GetTenantDashboardQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.GetAsync<Models.TenantDashboardResponseDTO>($"/api/Dashboard/tenant/{request.OrganizationId}");
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
