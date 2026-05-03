using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterMedicalRecord
{
    public record RegisterMedicalRecordCommand(MedicalRecordRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterMedicalRecordCommandHandler : IRequestHandler<RegisterMedicalRecordCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterMedicalRecordCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<MedicalRecordRequestDTO, object>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarExpediente}", request.Payload);
        }
    }
}
