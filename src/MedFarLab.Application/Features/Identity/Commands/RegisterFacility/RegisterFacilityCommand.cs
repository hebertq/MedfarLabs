using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterFacility
{
    public record RegisterFacilityCommand(FacilityRoomRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterFacilityCommandHandler : IRequestHandler<RegisterFacilityCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterFacilityCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterFacilityCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<FacilityRoomRequestDTO, object>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.RegistrarConsultorio}", request.Payload);
        }
    }
}
