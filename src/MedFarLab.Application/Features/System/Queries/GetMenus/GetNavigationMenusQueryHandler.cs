using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;

namespace MedFarLab.Application.Features.System.Queries.GetMenus
{
    public class GetNavigationMenusQueryHandler : IRequestHandler<GetNavigationMenusQuery, BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetNavigationMenusQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>> Handle(GetNavigationMenusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = global::System.Text.Json.JsonSerializer.Serialize(new { OrganizationTypeId = request.OrganizationTypeId });
                var encodedPayload = global::System.Net.WebUtility.UrlEncode(payload);
                var response = await _apiClient.GetAsync<IEnumerable<NavigationMenuResponseDTO>>($"api/System/12005?payload={encodedPayload}");
                if (response != null)
                {
                    return response;
                }
                var failResponse = new BaseResponse<IEnumerable<NavigationMenuResponseDTO>>();
                failResponse.IsSuccess = false;
                failResponse.Message = "No se pudo obtener el menú";
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
