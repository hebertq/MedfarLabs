using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace MedFarLab.Application.Features.System.Queries.GetSettingsMenu
{
    public class GetSettingsMenuQueryHandler : IRequestHandler<GetSettingsMenuQuery, BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetSettingsMenuQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>> Handle(GetSettingsMenuQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = global::System.Text.Json.JsonSerializer.Serialize(new { OrganizationTypeId = request.OrganizationTypeId, UserRole = request.UserRole });
                var encodedPayload = global::System.Net.WebUtility.UrlEncode(payload);
                var response = await _apiClient.GetAsync<IEnumerable<NavigationMenuResponseDTO>>($"api/System/12005?payload={encodedPayload}");
                if (response != null)
                {
                    return response;
                }
                var failResponse = new BaseResponse<IEnumerable<NavigationMenuResponseDTO>>();
                failResponse.IsSuccess = false;
                failResponse.Message = "No se pudo obtener el menú de configuración";
                return failResponse;
            }
            catch (Exception ex)
            {
                var failResponse = new BaseResponse<IEnumerable<NavigationMenuResponseDTO>>();
                failResponse.IsSuccess = false;
                failResponse.Message = ex.Message;
                return failResponse;
            }
        }
    }
}
