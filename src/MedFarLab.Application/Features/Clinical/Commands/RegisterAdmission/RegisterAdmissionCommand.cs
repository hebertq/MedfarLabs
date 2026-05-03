using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterAdmission
{
    public record RegisterAdmissionCommand(DirectAdmissionRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterAdmissionCommandHandler : IRequestHandler<RegisterAdmissionCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterAdmissionCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterAdmissionCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<DirectAdmissionRequestDTO, object>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarConsultaDirecta}", request.Payload);
        }
    }
}
