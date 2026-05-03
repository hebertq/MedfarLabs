using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Billing.Commands.RegisterPayment
{
    public record RegisterPaymentCommand(PaymentRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterPaymentCommandHandler : IRequestHandler<RegisterPaymentCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterPaymentCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterPaymentCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<PaymentRequestDTO, object>($"api/Billing/{(int)MedfarLabs.Core.Domain.Const.AppAction.Billling.RegistrarPago}", request.Payload);
        }
    }
}
