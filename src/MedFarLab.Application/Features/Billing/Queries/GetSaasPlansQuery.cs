using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetSaasPlansQuery : IRequest<List<SaasPlanResponseDTO>>
    {
        public int? OrganizationTypeId { get; set; }
    }

    public class GetSaasPlansQueryHandler : IRequestHandler<GetSaasPlansQuery, List<SaasPlanResponseDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetSaasPlansQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<SaasPlanResponseDTO>> Handle(GetSaasPlansQuery request, CancellationToken cancellationToken)
        {
            var payload = new GetSaasPlansRequestDTO
            {
                OrganizationTypeId = request.OrganizationTypeId
            };

            var payloadStr = global::System.Text.Json.JsonSerializer.Serialize(payload);
            var encodedPayload = global::System.Net.WebUtility.UrlEncode(payloadStr);
            var response = await _apiClient.GetAsync<List<SaasPlanResponseDTO>>($"api/Billing/{AppAction.Billling.GetSaasPlans}?payload={encodedPayload}");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return new List<SaasPlanResponseDTO>();
        }
    }
}
