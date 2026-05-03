using MediatR;
using Microsoft.AspNetCore.Mvc;
using MedfarLabs.Core.Application.Features.System.Commands.CreateMenu;
using MedfarLabs.Core.Application.Features.System.Commands.UpdateMenu;
using MedfarLabs.Core.Application.Features.System.Queries.GetMenusByOrganizationType;
using Microsoft.AspNetCore.Authorization;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] - Commented out for dev but typically would use policies here
    public class MenuController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("type/{organizationTypeId}")]
        public async Task<IActionResult> GetMenusByOrgType(int organizationTypeId)
        {
            var query = new GetMenusByOrganizationTypeQuery(organizationTypeId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMenu([FromBody] UpdateMenuCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
