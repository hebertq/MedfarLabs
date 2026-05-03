using MediatR;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Care.Models;

namespace MedFarLab.Application.Features.Care.Commands.RegisterConsultation
{
    public record RegisterConsultationCommand(ConsultationRequestDTO Payload) : IRequest<BaseResponse<long>>;

    public class RegisterConsultationCommandHandler : IRequestHandler<RegisterConsultationCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterConsultationCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(RegisterConsultationCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<ConsultationRequestDTO, long>($"api/Care/{(int)MedfarLabs.Core.Domain.Const.AppAction.Care.RegistrarConsulta}", request.Payload);
        }
    }
}
