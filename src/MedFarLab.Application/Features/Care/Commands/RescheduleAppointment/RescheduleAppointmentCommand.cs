using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Care.Commands.RescheduleAppointment
{
    public record RescheduleAppointmentCommand(long AppointmentId, TimeSpan NewStartTime) : IRequest<BaseResponse<object>>;

    public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RescheduleAppointmentCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PutAsync<object, object>($"api/Care/Appointments/{request.AppointmentId}/Reschedule?newTime={request.NewStartTime}", null!);
        }
    }
}
