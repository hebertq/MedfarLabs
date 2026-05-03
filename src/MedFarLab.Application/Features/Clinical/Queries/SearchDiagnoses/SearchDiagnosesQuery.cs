using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Response;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;

namespace MedFarLab.Application.Features.Clinical.Queries.SearchDiagnoses
{
    public record SearchDiagnosesQuery(string Query, int? CategoryId = null) : IRequest<BaseResponse<IEnumerable<DiagnosisCodeDTO>>>;

    public class SearchDiagnosesQueryHandler : IRequestHandler<SearchDiagnosesQuery, BaseResponse<IEnumerable<DiagnosisCodeDTO>>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SearchDiagnosesQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<IEnumerable<DiagnosisCodeDTO>>> Handle(SearchDiagnosesQuery request, CancellationToken cancellationToken)
        {
            var payload = new SearchDiagnosesRequestDTO(request.Query, request.CategoryId);
            return await _apiClient.PostAsync<SearchDiagnosesRequestDTO, IEnumerable<DiagnosisCodeDTO>>($"api/Clinical/{AppAction.Clinical.SearchDiagnoses}", payload);
        }
    }
}
