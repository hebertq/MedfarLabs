using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.System.Dtos;
using Microsoft.Extensions.Configuration;

namespace MedFarLab.Application.Features.System.Queries.GetMenus
{
    public class GetSystemMenusQuery : IRequest<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        public int OrganizationTypeId { get; set; }
    }

    public class GetSystemMenusQueryHandler : IRequestHandler<GetSystemMenusQuery, BaseResponse<IEnumerable<NavigationMenuResponseDTO>>>
    {
        private readonly IConfiguration _config;

        public GetSystemMenusQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<BaseResponse<IEnumerable<NavigationMenuResponseDTO>>> Handle(GetSystemMenusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
                var sql = "SELECT id as Id, organization_type_id as OrganizationTypeId, title as Title, route as Route, icon as Icon, order_index as OrderIndex FROM system.mst_navigation_menu WHERE organization_type_id = @OrgId AND is_active = true ORDER BY order_index ASC";
                var menus = await Dapper.SqlMapper.QueryAsync<NavigationMenuResponseDTO>(conn, sql, new { OrgId = request.OrganizationTypeId });
                
                var response = new BaseResponse<IEnumerable<NavigationMenuResponseDTO>>();
                response.IsSuccess = true;
                response.Data = menus;
                response.Message = "OK";
                return response;
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
