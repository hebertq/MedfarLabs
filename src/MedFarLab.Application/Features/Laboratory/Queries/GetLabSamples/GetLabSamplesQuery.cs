using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;

using System.Collections.Generic;

namespace MedFarLab.Application.Features.Laboratory.Models
{
    public class SampleRecord
    {
        public long Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string SampleType { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendiente";
    }
}

namespace MedFarLab.Application.Features.Laboratory.Queries.GetLabSamples
{
    public class GetLabSamplesQuery : IRequest<List<Models.SampleRecord>>
    {
        public GetLabSamplesQuery() { }
    }

    public class GetLabSamplesQueryHandler : IRequestHandler<GetLabSamplesQuery, List<Models.SampleRecord>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetLabSamplesQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<Models.SampleRecord>> Handle(GetLabSamplesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.GetAsync<List<Models.SampleRecord>>("api/Laboratory/8002");
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return new List<Models.SampleRecord>();
        }
    }
}
