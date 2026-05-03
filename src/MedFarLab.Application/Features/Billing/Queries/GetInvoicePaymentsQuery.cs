using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetInvoicePaymentsQuery : IRequest<IEnumerable<object>>
    {
        public long InvoiceId { get; set; }
    }

    public class GetInvoicePaymentsQueryHandler : IRequestHandler<GetInvoicePaymentsQuery, IEnumerable<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetInvoicePaymentsQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<object>> Handle(GetInvoicePaymentsQuery request, CancellationToken cancellationToken)
        {
            var dto = new GetInvoicePaymentsRequestDTO { InvoiceId = request.InvoiceId };
            var payload = JsonSerializer.Serialize(dto);
            
            var response = await _apiClient.GetAsync<IEnumerable<object>>("api/Billing/$(AppAction.Billling.GetInvoicePayments)?payload=$(payload)");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return new List<object>();
        }
    }
}
