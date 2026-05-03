using MediatR;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Commands.RegisterLabOrder
{
    public record RegisterLabOrderCommand(LabOrderRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterLabOrderCommandHandler : IRequestHandler<RegisterLabOrderCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterLabOrderCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterLabOrderCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<LabOrderRequestDTO, object>($"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.RegistrarOrden}", request.Payload);
        }
    }
}
