using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Laboratory.Models;
using System.Text.Json;

namespace MedFarLab.Application.Features.Laboratory.Queries.GetServiceSampleConfigs
{
    public class GetServiceSampleConfigsQuery : IRequest<List<ServiceSampleConfigDTO>>
    {
        public long ServiceId { get; set; }

        public GetServiceSampleConfigsQuery(long serviceId)
        {
            ServiceId = serviceId;
        }
    }

    public class GetServiceSampleConfigsQueryHandler : IRequestHandler<GetServiceSampleConfigsQuery, List<ServiceSampleConfigDTO>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetServiceSampleConfigsQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<ServiceSampleConfigDTO>> Handle(GetServiceSampleConfigsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { ServiceId = request.ServiceId };
                var response = await _apiClient.GetAsync<List<ServiceSampleConfigDTO>>($"api/Laboratory/8005?payload={JsonSerializer.Serialize(payload)}");

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return new List<ServiceSampleConfigDTO>();
        }
    }
}
