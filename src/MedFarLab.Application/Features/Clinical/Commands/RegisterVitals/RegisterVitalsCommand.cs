using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterVitals
{
    public record RegisterVitalsCommand(VitalSignsRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterVitalsCommandHandler : IRequestHandler<RegisterVitalsCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterVitalsCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterVitalsCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<VitalSignsRequestDTO, long>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarSignosVitales}", request.Payload);
        }
    }
}
