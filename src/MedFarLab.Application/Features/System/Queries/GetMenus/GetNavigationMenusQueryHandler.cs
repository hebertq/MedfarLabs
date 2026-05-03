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
                var response = await _apiClient.GetAsync<IEnumerable<NavigationMenuResponseDTO>>($"api/Menu/type/{request.OrganizationTypeId}");
                if (response != null)
                {
                    return response;
                }
                return BaseResponse<IEnumerable<NavigationMenuResponseDTO>>.Failure("No se pudo obtener el menú");
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<NavigationMenuResponseDTO>>.Failure(ex.Message);
            }
        }
    }
}
