using MediatR;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Clinical.Models;

namespace MedFarLab.Application.Features.Clinical.Commands.RegisterPatient
{
    public record RegisterPatientCommand(PatientVM Payload) : IRequest<BaseResponse<long?>>;

    public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, BaseResponse<long?>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterPatientCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long?>> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {
            var vm = request.Payload;
            var requestDto = new PatientRequestDTO
            {
                PersonId = vm.PersonId,
                OrganizationId = vm.OrganizationId,
                InternalCode = vm.InternalCode,
                AuditNotes = vm.AuditNotes
            };

            return await _apiClient.PostAsync<PatientRequestDTO, long?>($"api/Clinical/{(int)MedfarLabs.Core.Domain.Const.AppAction.Clinical.RegistrarPaciente}", requestDto);
        }
    }
}
