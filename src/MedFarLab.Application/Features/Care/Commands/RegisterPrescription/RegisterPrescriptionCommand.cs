using MediatR;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Care.Commands.RegisterPrescription
{
    public record RegisterPrescriptionCommand(PrescriptionRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class RegisterPrescriptionCommandHandler : IRequestHandler<RegisterPrescriptionCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterPrescriptionCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterPrescriptionCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<PrescriptionRequestDTO, object>($"api/Care/{(int)MedfarLabs.Core.Domain.Const.AppAction.Care.EmitirReceta}", request.Payload);
        }
    }
}
