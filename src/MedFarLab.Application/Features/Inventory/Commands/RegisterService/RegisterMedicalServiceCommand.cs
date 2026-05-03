using MediatR;
using MedfarLabs.Core.Application.Features.Inventory.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Inventory.Commands.RegisterService
{
    public record RegisterMedicalServiceCommand(MedicalServiceRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterMedicalServiceCommandHandler : IRequestHandler<RegisterMedicalServiceCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterMedicalServiceCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterMedicalServiceCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<MedicalServiceRequestDTO, long>($"api/Inventory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Inventory.RegistrarServicio}", request.Payload);
        }
    }
}
