using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using MedfarLabs.Core.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Enums;

namespace MedFarLab.Reporting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportController : ControllerBase
    {
        private readonly IActionDispatcher _dispatcher;

        public ReportController(IActionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet("{actionCode}")]
        public async Task<IActionResult> Get(int actionCode, [FromQuery] string payload = null)
        {
            JsonElement jsonPayload = default;
            if (!string.IsNullOrEmpty(payload))
            {
                try {
                    jsonPayload = JsonSerializer.Deserialize<JsonElement>(payload);
                } catch {
                    // Ignore or handle invalid JSON in query
                }
            }
            
            var result = await _dispatcher.DispatchAsync(AppModule.Report, actionCode, jsonPayload, HttpContext.TraceIdentifier, 0);
            return Ok(result);
        }

        [HttpPost("{actionCode}")]
        public async Task<IActionResult> Post(int actionCode, [FromBody] JsonElement payload)
        {
            var result = await _dispatcher.DispatchAsync(AppModule.Report, actionCode, payload, HttpContext.TraceIdentifier, 0);
            return Ok(result);
        }
    }
}
