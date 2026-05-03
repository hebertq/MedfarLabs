using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Entities.Common;
using System.Text.Json;

namespace MedFarLab.Application.Features.Common.Queries.GetCatalogDetails
{
    public class GetCatalogDetailsQuery : IRequest<List<CatalogDetail>>
    {
        public int CatalogId { get; set; }

        public GetCatalogDetailsQuery(int catalogId)
        {
            CatalogId = catalogId;
        }
    }

    public class GetCatalogDetailsQueryHandler : IRequestHandler<GetCatalogDetailsQuery, List<CatalogDetail>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetCatalogDetailsQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CatalogDetail>> Handle(GetCatalogDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { catalog = request.CatalogId };
                var payloadStr = Uri.EscapeDataString(JsonSerializer.Serialize(payload));
                var response = await _apiClient.GetAsync<List<CatalogDetail>>($"api/Common/6001?payload={payloadStr}");

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return new List<CatalogDetail>();
        }
    }
}
