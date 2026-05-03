using MedfarLabs.Core.Application.Features.Care.Dtos.Response;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using MedFarLab.Application.Features.Clinical.Models;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Queries.GetClinicalDashboard
{
    public class GetClinicalDashboardQuery : IRequest<BaseResponse<ClinicalDashboardVM>>
    {
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
    }

    public class GetClinicalDashboardQueryHandler : IRequestHandler<GetClinicalDashboardQuery, BaseResponse<ClinicalDashboardVM>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetClinicalDashboardQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<ClinicalDashboardVM>> Handle(GetClinicalDashboardQuery request, CancellationToken cancellationToken)
        {
            var apiResponse = await _apiClient.GetAsync<ClinicalDashboardVM>($"api/Care/Dashboard/Clinical?branchId={request.BranchId}&date={request.Date:yyyy-MM-dd}");

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return BaseResponse<ClinicalDashboardVM>.Success(apiResponse.Data, "Dashboard cargado correctamente.");
            }

            return BaseResponse<ClinicalDashboardVM>.Failure("No se pudo cargar el dashboard.");
        }
    }
}
