using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Billing.Commands.SearchInvoice
{
    public record SearchInvoiceQuery(SearchInvoiceRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class SearchInvoiceQueryHandler : IRequestHandler<SearchInvoiceQuery, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SearchInvoiceQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(SearchInvoiceQuery request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<SearchInvoiceRequestDTO, object>($"api/Billing/{(int)MedfarLabs.Core.Domain.Const.AppAction.Billling.BuscarFactura}", request.Payload);
        }
    }
}
