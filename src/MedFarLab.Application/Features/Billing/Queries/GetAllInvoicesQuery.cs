using MediatR;
using MedFarLab.Application.Features.Billing.DTOs;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetAllInvoicesQuery : IRequest<List<InvoiceDto>> 
    { 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetAllInvoicesQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
        {
            var payload = new MedfarLabs.Core.Application.Features.Billing.Dtos.Request.GetAllInvoicesRequestDTO 
            { 
                Page = request.Page,
                PageSize = request.PageSize 
            };
            var jsonPayload = global::System.Text.Json.JsonSerializer.Serialize(payload);
            var response = await _apiClient.GetAsync<List<InvoiceDto>>($"api/Billing/{AppAction.Billling.GetAllInvoices}?payload={jsonPayload}");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return new List<InvoiceDto>();
        }
    }
}
