using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetSubscriptionStatusQuery : IRequest<SubscriptionStatusResponseDTO?>
    {
    }

    public class GetSubscriptionStatusQueryHandler : IRequestHandler<GetSubscriptionStatusQuery, SubscriptionStatusResponseDTO?>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetSubscriptionStatusQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<SubscriptionStatusResponseDTO?> Handle(GetSubscriptionStatusQuery request, CancellationToken cancellationToken)
        {
            var payload = new GetSubscriptionStatusRequestDTO();
            var payloadStr = global::System.Text.Json.JsonSerializer.Serialize(payload);
            var encodedPayload = global::System.Net.WebUtility.UrlEncode(payloadStr);
            var response = await _apiClient.GetAsync<SubscriptionStatusResponseDTO>($"api/Billing/{AppAction.Billling.GetSubscriptionStatus}?payload={encodedPayload}");
            
            if (response != null && response.IsSuccess)
            {
                return response.Data;
            }
            
            return null;
        }
    }
}
