using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;

namespace MedFarLab.Application.Features.System.Queries.GetMenus
{
    public class GetNavigationMenusQuery : IRequest<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        public int OrganizationTypeId { get; set; }

        public GetNavigationMenusQuery(int organizationTypeId)
        {
            OrganizationTypeId = organizationTypeId;
        }
    }
}
