using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Response;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using System.Collections.Generic;

namespace MedFarLab.Application.Features.Identity.Queries.SearchPersons
{
    public record SearchPersonsCommand(string SearchTerm, long OrganizationId) : IRequest<BaseResponse<IEnumerable<SearchPersonResponseDTO>>>;

    public class SearchPersonsCommandHandler : IRequestHandler<SearchPersonsCommand, BaseResponse<IEnumerable<SearchPersonResponseDTO>>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SearchPersonsCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<IEnumerable<SearchPersonResponseDTO>>> Handle(SearchPersonsCommand request, CancellationToken cancellationToken)
        {
            var requestDto = new SearchPersonRequestDTO
            {
                OrganizationId = request.OrganizationId,
                SearchTerm = request.SearchTerm
            };
            
            return await _apiClient.PostAsync<SearchPersonRequestDTO, IEnumerable<SearchPersonResponseDTO>>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.ConsultarPersonaGlobal}", requestDto);
        }
    }
}
