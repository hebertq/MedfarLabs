using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterAntecedent
{
    public record RegisterAntecedentCommand(AntecedentRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterAntecedentCommandHandler : IRequestHandler<RegisterAntecedentCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterAntecedentCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterAntecedentCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<AntecedentRequestDTO, object>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarAntecedente}", request.Payload);
        }
    }
}
