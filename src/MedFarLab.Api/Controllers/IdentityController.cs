using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MedfarLabs.Core.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Enums;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class IdentityController : ControllerBase
    {
        private readonly IActionDispatcher _dispatcher;
        private readonly MedfarLabs.Core.Domain.Interfaces.Security.IUserContext _userContext;

        public IdentityController(IActionDispatcher dispatcher, MedfarLabs.Core.Domain.Interfaces.Security.IUserContext userContext)
        {
            _dispatcher = dispatcher;
            _userContext = userContext;
        }

        [HttpGet("{actionCode}")]
        public async Task<IActionResult> Get(int actionCode, [FromQuery] string? payload = null)
        {
            JsonElement jsonPayload = JsonSerializer.Deserialize<JsonElement>("{}");
            if (!string.IsNullOrEmpty(payload))
            {
                try {
                    jsonPayload = JsonSerializer.Deserialize<JsonElement>(payload);
                } catch {
                    // Ignore or handle invalid JSON in query
                }
            }
            
            long branchId = Request.Headers.TryGetValue("X-Branch-Id", out var bStr) && long.TryParse(bStr, out var b) ? b : 0;
            var result = await _dispatcher.DispatchAsync(AppModule.Identity, actionCode, jsonPayload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }

        [HttpPost("{actionCode}")]
        public async Task<IActionResult> Post(int actionCode, [FromBody] JsonElement payload)
        {
            Console.WriteLine($"[IdentityController.Post] _userContext.UserId inside Controller is: {_userContext.UserId}");
            long branchId = Request.Headers.TryGetValue("X-Branch-Id", out var bStr) && long.TryParse(bStr, out var b) ? b : 0;
            var result = await _dispatcher.DispatchAsync(AppModule.Identity, actionCode, payload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }
    }
}


