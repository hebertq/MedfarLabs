using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using MedfarLabs.Core.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Enums;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClinicalController : ControllerBase
    {
        private readonly IActionDispatcher _dispatcher;

        public ClinicalController(IActionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
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
            var result = await _dispatcher.DispatchAsync(AppModule.Clinical, actionCode, jsonPayload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }

        [HttpPost("{actionCode}")]
        public async Task<IActionResult> Post(int actionCode, [FromBody] JsonElement payload)
        {
            long branchId = Request.Headers.TryGetValue("X-Branch-Id", out var bStr) && long.TryParse(bStr, out var b) ? b : 0;
            var result = await _dispatcher.DispatchAsync(AppModule.Clinical, actionCode, payload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }
    }
}


