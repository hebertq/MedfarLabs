using MediatR;
using MedfarLabs.Core.Application.Features.Security.Dtos;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Security.Commands.RegisterRoleGroup
{
    public record RegisterRoleGroupCommand(RoleGroupRequestDto Payload) : IRequest<BaseResponse<object>>;

    public class RegisterRoleGroupCommandHandler : IRequestHandler<RegisterRoleGroupCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterRoleGroupCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterRoleGroupCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<RoleGroupRequestDto, object>("api/Security/CreateRoleGroup", request.Payload);
        }
    }
}
