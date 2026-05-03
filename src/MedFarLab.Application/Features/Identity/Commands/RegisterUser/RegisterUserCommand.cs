using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterUser
{
    public record RegisterUserCommand(UsuarioRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterUserCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<UsuarioRequestDTO, object>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.RegistrarUsuario}", request.Payload);
        }
    }
}
