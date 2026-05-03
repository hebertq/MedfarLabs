using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using MedFarLab.Application.Features.Pharmacy.Models;

namespace MedFarLab.Application.Features.Pharmacy.Queries.GetPharmacyDashboard
{
    public class GetPharmacyDashboardQuery : IRequest<BaseResponse<PharmacyDashboardVM>>
    {
        public long BranchId { get; set; }
    }

    public class GetPharmacyDashboardQueryHandler : IRequestHandler<GetPharmacyDashboardQuery, BaseResponse<PharmacyDashboardVM>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetPharmacyDashboardQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<PharmacyDashboardVM>> Handle(GetPharmacyDashboardQuery request, CancellationToken cancellationToken)
        {
            // Todo: 9991 is a placeholder integer Action Code pending formal Dapper aggregation integration in Core.
            return await _apiClient.GetAsync<PharmacyDashboardVM>($"api/Pharmacy/9991?payload={{\"BranchId\":{request.BranchId}}}");
        }
    }
}
