using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;
using System.Collections.Generic;

namespace MedFarLab.Application.Features.System.Queries.GetSettingsMenu
{
    public class GetSettingsMenuQuery : IRequest<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        public int OrganizationTypeId { get; set; }
        public string? UserRole { get; set; }

        public GetSettingsMenuQuery(int organizationTypeId, string? userRole)
        {
            OrganizationTypeId = organizationTypeId;
            UserRole = userRole;
        }
    }
}
