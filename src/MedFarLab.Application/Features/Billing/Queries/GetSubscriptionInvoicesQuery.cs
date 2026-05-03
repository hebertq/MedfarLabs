using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetSubscriptionInvoicesQuery : IRequest<List<SubscriptionInvoiceResponseDTO>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetSubscriptionInvoicesQueryHandler : IRequestHandler<GetSubscriptionInvoicesQuery, List<SubscriptionInvoiceResponseDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetSubscriptionInvoicesQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<SubscriptionInvoiceResponseDTO>> Handle(GetSubscriptionInvoicesQuery request, CancellationToken cancellationToken)
        {
            var payload = new GetSubscriptionInvoicesRequestDTO
            {
                Page = request.Page,
                PageSize = request.PageSize
            };

            var payloadStr = global::System.Text.Json.JsonSerializer.Serialize(payload);
            var encodedPayload = global::System.Net.WebUtility.UrlEncode(payloadStr);
            var response = await _apiClient.GetAsync<List<SubscriptionInvoiceResponseDTO>>($"api/Billing/{AppAction.Billling.GetSubscriptionInvoices}?payload={encodedPayload}");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return new List<SubscriptionInvoiceResponseDTO>();
        }
    }
}
