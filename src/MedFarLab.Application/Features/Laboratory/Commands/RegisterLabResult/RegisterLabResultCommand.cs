using MediatR;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Commands.RegisterLabResult
{
    public record RegisterLabResultCommand(LabResultRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterLabResultCommandHandler : IRequestHandler<RegisterLabResultCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterLabResultCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterLabResultCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<LabResultRequestDTO, long>($"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.RegistrarResultado}", request.Payload);
        }
    }
}
