using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Billing.Commands.GenerateInvoice
{
    public record GenerateInvoiceCommand(InvoiceRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GenerateInvoiceCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<InvoiceRequestDTO, long>($"api/Billing/{(int)MedfarLabs.Core.Domain.Const.AppAction.Billling.GenerarFactura}", request.Payload);
        }
    }
}
