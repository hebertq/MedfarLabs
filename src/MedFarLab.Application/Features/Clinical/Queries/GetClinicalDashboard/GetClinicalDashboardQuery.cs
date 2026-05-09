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
            var payloadObj = new { BranchId = request.BranchId, Date = request.Date.ToString("yyyy-MM-ddTHH:mm:ssZ") };
            var encodedPayload = global::System.Net.WebUtility.UrlEncode(global::System.Text.Json.JsonSerializer.Serialize(payloadObj));
            var apiResponse = await _apiClient.GetAsync<ClinicalDashboardVM>($"api/Care/5008?payload={encodedPayload}");

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return BaseResponse<ClinicalDashboardVM>.Success(apiResponse.Data, "Dashboard cargado correctamente.");
            }

            return BaseResponse<ClinicalDashboardVM>.Failure("No se pudo cargar el dashboard.");
        }
    }
}
