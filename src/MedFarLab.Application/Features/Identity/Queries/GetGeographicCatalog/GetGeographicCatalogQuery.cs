using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Response;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;

namespace MedFarLab.Application.Features.Identity.Queries.GetGeographicCatalog
{
    public record GetGeographicCatalogQuery(int? CountryId) : IRequest<BaseResponse<GeoCatalogResponseDTO>>;

    public class GetGeographicCatalogQueryHandler : IRequestHandler<GetGeographicCatalogQuery, BaseResponse<GeoCatalogResponseDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetGeographicCatalogQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<GeoCatalogResponseDTO>> Handle(GetGeographicCatalogQuery request, CancellationToken cancellationToken)
        {
            var payload = new GetGeographicCatalogRequestDTO { CountryId = request.CountryId };
            return await _apiClient.PostAsync<GetGeographicCatalogRequestDTO, GeoCatalogResponseDTO>(
                $"api/Identity/{AppAction.Identity.GetGeographicCatalog}", payload);
        }
    }
}
