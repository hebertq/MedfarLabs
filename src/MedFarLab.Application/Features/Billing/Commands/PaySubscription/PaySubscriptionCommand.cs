using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Billing.Commands.PaySubscription
{
    public class PaySubscriptionPayload
    {
        public long OrganizationId { get; set; }
        public long InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public record PaySubscriptionCommand(PaySubscriptionPayload Payload) : IRequest<BaseResponse<long>>;

    public class PaySubscriptionCommandHandler : IRequestHandler<PaySubscriptionCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public PaySubscriptionCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(PaySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var reqDto = new PaySubscriptionRequestDTO
            {
                OrganizationId = request.Payload.OrganizationId,
                InvoiceId = request.Payload.InvoiceId,
                AmountPaid = request.Payload.AmountPaid,
                PaymentMethod = request.Payload.PaymentMethod
            };
            return await _apiClient.PostAsync<PaySubscriptionRequestDTO, long>($"api/Billing/{(int)MedfarLabs.Core.Domain.Const.AppAction.Billling.PagarSuscripcion}", reqDto);
        }
    }
}
