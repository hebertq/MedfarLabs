using MediatR;
using MedFarLab.Application.Features.Care.Models;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Care;

namespace MedFarLab.Application.Features.Care.Queries
{
    public class GetDailyAppointmentsQuery : IRequest<List<AppointmentModel>>
    {
        public DateTime Date { get; set; }
        public long? DoctorUserId { get; set; }
    }

    public class GetDailyAppointmentsQueryHandler : IRequestHandler<GetDailyAppointmentsQuery, List<AppointmentModel>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetDailyAppointmentsQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<AppointmentModel>> Handle(GetDailyAppointmentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string url = $"api/Care/Appointments?date={request.Date:yyyy-MM-dd}";
                if (request.DoctorUserId.HasValue)
                {
                    url += $"&doctorUserId={request.DoctorUserId.Value}";
                }

                var response = await _apiClient.GetAsync<List<AppointmentModel>>(url);

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch { }
            return new List<AppointmentModel>();
        }
    }
}
