using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public class PayInvoiceCommand : IRequest<BaseResponse<long>>
    {
        public long InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class PayInvoiceCommandHandler : IRequestHandler<PayInvoiceCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public PayInvoiceCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(PayInvoiceCommand request, CancellationToken cancellationToken)
        {
            var payload = new PaymentRequestDTO
            {
                InvoiceId = request.InvoiceId,
                AmountPaid = request.AmountPaid
            };

            var response = await _apiClient.PostAsync<PaymentRequestDTO, long>($"api/Billing/{AppAction.Billling.RegistrarPago}", payload);

            return response;
        }
    }
}
