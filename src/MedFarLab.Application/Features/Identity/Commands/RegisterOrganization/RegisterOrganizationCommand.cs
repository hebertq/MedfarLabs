using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterOrganization
{
    public record RegisterOrganizationCommand(OrganizationRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterOrganizationCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterOrganizationCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<OrganizationRequestDTO, long>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.RegistrarOrganizacion}", request.Payload);
        }
    }
}
