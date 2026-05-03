using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterAppUser
{
    public record RegisterAppUserCommand(CreateAppUserRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterAppUserCommandHandler : IRequestHandler<RegisterAppUserCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterAppUserCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterAppUserCommand request, CancellationToken cancellationToken)
        {
            // We dispatch a raw custom POST because IdentityController should expose this endpoint
            return await _apiClient.PostAsync<CreateAppUserRequestDTO, long>($"api/Identity/Users/AppUser", request.Payload);
        }
    }
}
