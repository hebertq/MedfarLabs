using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Response;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using MedFarLab.Application.Features.Laboratory.Models;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Queries.GetLabDashboard
{
    public class GetLabDashboardQuery : IRequest<BaseResponse<LabDashboardVM>>
    {
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
    }

    public class GetLabDashboardQueryHandler : IRequestHandler<GetLabDashboardQuery, BaseResponse<LabDashboardVM>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetLabDashboardQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<LabDashboardVM>> Handle(GetLabDashboardQuery request, CancellationToken cancellationToken)
        {
            // Todo: 9992 is a placeholder integer Action Code pending formal Dapper aggregation integration in Core.
            return await _apiClient.GetAsync<LabDashboardVM>($"api/Laboratory/9992?payload={{\"BranchId\":{request.BranchId},\"Date\":\"{request.Date:yyyy-MM-dd}\"}}");
        }
    }
}
