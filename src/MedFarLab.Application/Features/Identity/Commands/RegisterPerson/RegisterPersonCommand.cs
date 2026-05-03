using MediatR;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Identity.Models;

namespace MedFarLab.Application.Features.Identity.Commands.RegisterPerson
{
    public record RegisterPersonCommand(PersonVM Payload) : IRequest<BaseResponse<object>>;

    public class RegisterPersonCommandHandler : IRequestHandler<RegisterPersonCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterPersonCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterPersonCommand request, CancellationToken cancellationToken)
        {
            var p = request.Payload;
            var requestDto = new PersonRequestDTO(
                FirstName: p.FirstName,
                MiddleName: p.MiddleName,
                LastName: p.LastName,
                SecondLastName: p.SecondLastName,
                BirthDate: p.BirthDate ?? DateTime.Today,
                GenderId: p.GenderId,
                BirthCountryId: p.BirthCountryId,
                Email: p.Email,
                MobilePhone: p.MobilePhone,
                Address: p.Address ?? string.Empty,
                GeolocationId: p.GeolocationId
            );
            return await _apiClient.PostAsync<PersonRequestDTO, object>($"api/Identity/{(int)MedfarLabs.Core.Domain.Const.AppAction.Identity.RegistrarPersona}", requestDto);
        }
    }
}
