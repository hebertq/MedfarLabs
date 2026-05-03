using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;


namespace MedFarLab.Application.Features.Identity.Models
{
    public class DoctorListResponse
    {
        public long DoctorUserId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
    }
}

namespace MedFarLab.Application.Features.Identity.Queries.GetDoctors
{
    public class GetDoctorsQuery : IRequest<List<Models.DoctorListResponse>>
    {
        public long OrganizationId { get; set; }

        public GetDoctorsQuery(long organizationId)
        {
            OrganizationId = organizationId;
        }
    }

    public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, List<Models.DoctorListResponse>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetDoctorsQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<Models.DoctorListResponse>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.GetAsync<List<Models.DoctorListResponse>>($"api/Auth/Doctors?organizationId={request.OrganizationId}");

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return new List<Models.DoctorListResponse>();
        }
    }
}
