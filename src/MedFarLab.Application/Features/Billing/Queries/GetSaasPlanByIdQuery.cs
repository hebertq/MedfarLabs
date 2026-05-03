using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetSaasPlanByIdQuery : IRequest<SaasPlanResponseDTO?>
    {
        public int PlanId { get; set; }
    }

    public class GetSaasPlanByIdQueryHandler : IRequestHandler<GetSaasPlanByIdQuery, SaasPlanResponseDTO?>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetSaasPlanByIdQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<SaasPlanResponseDTO?> Handle(GetSaasPlanByIdQuery request, CancellationToken cancellationToken)
        {
            var payload = new GetSaasPlanByIdRequestDTO
            {
                PlanId = request.PlanId
            };

            var payloadStr = global::System.Text.Json.JsonSerializer.Serialize(payload);
            var encodedPayload = global::System.Net.WebUtility.UrlEncode(payloadStr);
            var response = await _apiClient.GetAsync<SaasPlanResponseDTO>($"api/Billing/{AppAction.Billling.GetSaasPlanById}?payload={encodedPayload}");
            
            if (response != null && response.IsSuccess)
            {
                return response.Data;
            }
            
            return null;
        }
    }
}
