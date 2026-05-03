using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterConsent
{
    public record RegisterConsentCommand(ConsentRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterConsentCommandHandler : IRequestHandler<RegisterConsentCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterConsentCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterConsentCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<ConsentRequestDTO, object>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarConsentimiento}", request.Payload);
        }
    }
}
