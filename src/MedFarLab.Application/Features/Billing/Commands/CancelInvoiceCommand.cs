using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public class CancelInvoiceCommand : IRequest<BaseResponse<bool>>
    {
        public long InvoiceId { get; set; }
    }

    public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public CancelInvoiceCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            var payload = new CancelInvoiceRequestDTO
            {
                InvoiceId = request.InvoiceId
            };

            var response = await _apiClient.PostAsync<CancelInvoiceRequestDTO, bool>($"api/Billing/{AppAction.Billling.AnularFactura}", payload);

            return response;
        }
    }
}
