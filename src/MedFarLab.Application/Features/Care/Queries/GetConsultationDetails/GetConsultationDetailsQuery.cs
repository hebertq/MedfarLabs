using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;

namespace MedFarLab.Application.Features.Care.Queries.GetConsultationDetails
{
    public class GetConsultationDetailsQuery : IRequest<BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>>
    {
        public long ConsultationId { get; set; }

        public GetConsultationDetailsQuery(long consultationId)
        {
            ConsultationId = consultationId;
        }
    }

    public class GetConsultationDetailsQueryHandler : IRequestHandler<GetConsultationDetailsQuery, BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetConsultationDetailsQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>> Handle(GetConsultationDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new MedfarLabs.Core.Application.Features.Care.Dtos.Request.GetConsultationDetailsRequestDTO(request.ConsultationId);
                var response = await _apiClient.PostAsync<MedfarLabs.Core.Application.Features.Care.Dtos.Request.GetConsultationDetailsRequestDTO, MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>(
                    $"api/Care/{MedfarLabs.Core.Domain.Const.AppAction.Care.GetConsultationDetails}", payload
                );

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>.Success(response.Data);
                }

                return BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>.Failure(response?.Message ?? "Error al obtener la consulta.");
            }
            catch (Exception ex)
            {
                return BaseResponse<MedfarLabs.Core.Application.Features.Care.Dtos.Response.ConsultationDetailsResponseDTO>.Failure(ex.Message);
            }
        }
    }
}
