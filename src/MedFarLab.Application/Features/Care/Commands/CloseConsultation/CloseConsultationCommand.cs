using MedFarLab.Application.Common.Resilience;
using MedFarLab.Application.Features.Care.Commands.RegisterAppointment;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MediatR;

namespace MedFarLab.Application.Features.Care.Commands.CloseConsultation
{
    public record CloseConsultationCommand(CloseConsultationRequestDTO Payload) : IRequest<BaseResponse<object>>;

    public class CloseConsultationCommandHandler : IRequestHandler<CloseConsultationCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;
        private readonly OfflineCommandHandler<CloseConsultationCommand> _offlineHandler;
        public CloseConsultationCommandHandler(IExternalServiceClient apiClient,
        OfflineCommandHandler<CloseConsultationCommand> offlineHandler)
        {
            _apiClient = apiClient;
            _offlineHandler = offlineHandler;
        }

        public async Task<BaseResponse<object>> Handle(CloseConsultationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _apiClient.PostAsync<CloseConsultationRequestDTO, object>($"api/Care/{(int)MedfarLabs.Core.Domain.Const.AppAction.Care.CerrarConsulta}", request.Payload);
            }
            catch
            {
                return await _offlineHandler.ProcessOffline(request);
            }
        }
    }
}
