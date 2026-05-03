using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Clinical.Models;

namespace MedFarLab.Application.Features.Clinical.Commands.SetDoctorConfiguration
{
    public record SetDoctorConfigurationCommand(DoctorConfigurationVM Payload) : IRequest<BaseResponse<long>>;

    public class SetDoctorConfigurationCommandHandler : IRequestHandler<SetDoctorConfigurationCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SetDoctorConfigurationCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(SetDoctorConfigurationCommand request, CancellationToken cancellationToken)
        {
            var vm = request.Payload;
            var requestDto = new DoctorConfigurationRequestDTO
            {
                DoctorUserId = vm.DoctorUserId,
                AvailableHours = vm.AvailableHours,
                MinConsultationTimeMins = vm.MinConsultationTimeMins
            };

            // Endpoint definido en ClinicalController
            return await _apiClient.PostAsync<DoctorConfigurationRequestDTO, long>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.ConfigurarMedico}", requestDto);
        }
    }
}
