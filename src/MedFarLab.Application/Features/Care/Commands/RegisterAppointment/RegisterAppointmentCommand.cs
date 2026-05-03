using MediatR;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Care.Models;

namespace MedFarLab.Application.Features.Care.Commands.RegisterAppointment
{
    public record RegisterAppointmentCommand(AppointmentVM Payload) : IRequest<BaseResponse<object>>;

    public class RegisterAppointmentCommandHandler : IRequestHandler<RegisterAppointmentCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterAppointmentCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterAppointmentCommand request, CancellationToken cancellationToken)
        {
            var vm = request.Payload;
            var requestDto = new AppointmentRequestDTO(
                BranchId: 1, // Defaulting or fetching from context
                PatientId: vm.PatientId,
                DoctorUserId: vm.DoctorUserId,
                FacilityRoomId: null,
                ScheduledDate: vm.Date ?? DateTime.Today,
                StartTime: vm.SelectedTime ?? TimeSpan.Zero,
                EndTime: (vm.SelectedTime ?? TimeSpan.Zero).Add(TimeSpan.FromMinutes(30)),
                StatusId: (int)vm.Status,
                ReasonNotes: vm.Reason
            );

            return await _apiClient.PostAsync<AppointmentRequestDTO, object>($"api/Care/{(int)MedfarLabs.Core.Domain.Const.AppAction.Care.GestionarCita}", requestDto);
        }
    }
}
