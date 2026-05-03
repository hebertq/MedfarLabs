using MediatR;
using MedFarLab.Application.Features.Billing.DTOs;
using MedfarLabs.Core.Domain.Entities.Billing;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Billing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public class CreateInvoiceCommand : IRequest<MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<long>>
    {
        public MedfarLabs.Core.Application.Features.Billing.Dtos.Request.InvoiceRequestDTO InvoiceRequest { get; set; } = new();
    }

    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<long>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public CreateInvoiceCommandHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<long>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.PostAsync<MedfarLabs.Core.Application.Features.Billing.Dtos.Request.InvoiceRequestDTO, long>(
                $"api/Billing/{MedfarLabs.Core.Domain.Const.AppAction.Billling.GenerarFactura}", 
                request.InvoiceRequest);
                
            return response ?? MedfarLabs.Core.Domain.Common.Responses.Generic.BaseResponse<long>.Failure("Sin respuesta del API externa");
        }
    }
}

