using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Response;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Queries.ConsultarPersona
{
    public record ConsultarPersonaQuery(long PersonId, long OrganizationId) : IRequest<BaseResponse<PersonResponseDTO>>;

    public class ConsultarPersonaQueryHandler : IRequestHandler<ConsultarPersonaQuery, BaseResponse<PersonResponseDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public ConsultarPersonaQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<PersonResponseDTO>> Handle(ConsultarPersonaQuery request, CancellationToken cancellationToken)
        {
            var requestDto = new ConsultarPersonaRequestDTO
            {
                OrganizationId = request.OrganizationId,
                PersonId = request.PersonId
            };
            
            return await _apiClient.PostAsync<ConsultarPersonaRequestDTO, PersonResponseDTO>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.ConsultarPersona}", requestDto);
        }
    }
}
