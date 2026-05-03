using MediatR;
using MedFarLab.Application.Features.Clinical.Models;

namespace MedFarLab.Application.Features.Patient.Queries.GetPatientDirectory
{
    public class GetPatientDirectoryQuery : IRequest<List<PatientDirectoryVM>>
    {
        public long OrganizationId { get; set; }
        public GetPatientDirectoryQuery(long orgId) => OrganizationId = orgId;
    }

    public class GetPatientDirectoryQueryHandler : IRequestHandler<GetPatientDirectoryQuery, List<PatientDirectoryVM>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetPatientDirectoryQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<PatientDirectoryVM>> Handle(GetPatientDirectoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { organization_id = request.OrganizationId };
                var response = await _apiClient.PostAsync<object, IEnumerable<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientDirectoryResponseDTO>>(
                    "api/Clinical/4010", payload
                );

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data.Select(dto => new PatientDirectoryVM
                    {
                        Id = dto.Id,
                        FullName = dto.FullName,
                        DocumentId = dto.DocumentId,
                        Status = dto.Status,
                        LastVisit = dto.LastVisit,
                        MainDiagnosis = dto.MainDiagnosis
                    }).ToList();
                }
            }
            catch { }
            return new List<PatientDirectoryVM>();
        }
    }
}
