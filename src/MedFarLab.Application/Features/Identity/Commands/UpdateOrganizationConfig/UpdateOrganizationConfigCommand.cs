using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Identity.Commands.UpdateOrganizationConfig
{
    public class UpdateOrganizationConfigCommand : IRequest<BaseResponse<bool>>
    {
        public MedfarLabs.Core.Application.Features.Identity.Dtos.Request.UpdateOrganizationConfigDTO ConfigRequest { get; set; } = default!;
    }

    public class UpdateOrganizationConfigCommandHandler : IRequestHandler<UpdateOrganizationConfigCommand, BaseResponse<bool>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public UpdateOrganizationConfigCommandHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateOrganizationConfigCommand request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.PostAsync<MedfarLabs.Core.Application.Features.Identity.Dtos.Request.UpdateOrganizationConfigDTO, bool>(
                $"api/Identity/{MedfarLabs.Core.Domain.Const.AppAction.Identity.ActualizarConfiguracionOrganizacion}", 
                request.ConfigRequest);
                
            return response ?? BaseResponse<bool>.Failure("Sin respuesta de la API externa");
        }
    }
}
