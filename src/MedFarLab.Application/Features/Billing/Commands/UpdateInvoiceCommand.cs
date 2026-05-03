using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.Billing.Interfaces;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public class UpdateInvoiceCommand : IRequest<BaseResponse<long>>
    {
        public UpdateInvoiceRequestDTO InvoiceRequest { get; set; } = null!;
    }

    public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, BaseResponse<long>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public UpdateInvoiceCommandHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.PostAsync<MedfarLabs.Core.Application.Features.Billing.Dtos.Request.UpdateInvoiceRequestDTO, long>(
                $"api/Billing/{MedfarLabs.Core.Domain.Const.AppAction.Billling.ActualizarFactura}", 
                request.InvoiceRequest);
                
            return response ?? BaseResponse<long>.Failure("Sin respuesta del API externa");
        }
    }
}
