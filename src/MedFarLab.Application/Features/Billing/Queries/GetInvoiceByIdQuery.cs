using MediatR;
using MedFarLab.Application.Features.Billing.DTOs;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetInvoiceByIdQuery : IRequest<InvoiceDto>
    {
        public long InvoiceId { get; set; }
    }

    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetInvoiceByIdQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.GetAsync<InvoiceDto>($"api/Billing/{AppAction.Billling.GetInvoiceById}?payload={{\"InvoiceId\":{request.InvoiceId}}}");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return null;
        }
    }
}
