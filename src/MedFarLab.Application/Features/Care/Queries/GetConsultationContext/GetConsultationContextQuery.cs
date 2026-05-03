using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Care.Dtos.Response;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;

namespace MedFarLab.Application.Features.Care.Queries.GetConsultationContext
{
    public record GetConsultationContextQuery(long AppointmentId) : IRequest<BaseResponse<ConsultationContextResponseDTO>>;

    public class GetConsultationContextQueryHandler : IRequestHandler<GetConsultationContextQuery, BaseResponse<ConsultationContextResponseDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetConsultationContextQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<ConsultationContextResponseDTO>> Handle(GetConsultationContextQuery request, CancellationToken cancellationToken)
        {
            var payload = new ConsultationContextRequestDTO { AppointmentId = request.AppointmentId };
            return await _apiClient.PostAsync<ConsultationContextRequestDTO, ConsultationContextResponseDTO>($"api/Care/{AppAction.Care.GetConsultationContext}", payload);
        }
    }
}
