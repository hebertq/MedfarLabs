using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterBranch
{
    public record RegisterBranchCommand(BranchRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterBranchCommandHandler : IRequestHandler<RegisterBranchCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterBranchCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterBranchCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<BranchRequestDTO, object>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.RegistrarSucursal}", request.Payload);
        }
    }
}
