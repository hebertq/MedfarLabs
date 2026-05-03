using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MedfarLabs.Core.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace MedFarLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BillingController : ControllerBase
    {
        private readonly IActionDispatcher _dispatcher;

        public BillingController(IActionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet("{actionCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int actionCode, [FromQuery] string? payload = null, [FromServices] MedfarLabs.Core.Domain.Interfaces.Security.IUserContext userContext = null)
        {
            JsonElement jsonPayload = default;
            if (!string.IsNullOrEmpty(payload))
            {
                try {
                    // Backward compatibility for GetInvoiceById which sends a plain number
                    if (actionCode == MedfarLabs.Core.Domain.Const.AppAction.Billling.GetInvoiceById && long.TryParse(payload, out _))
                    {
                        payload = $"{{\"InvoiceId\": {payload}}}";
                    }
                    else if (!payload.Contains("OrganizationId"))
                    {
                        // Parse as dictionary, add OrganizationId, then serialize back
                        var dict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(payload);
                        if (dict != null)
                        {
                            dict["OrganizationId"] = userContext.OrganizationId;
                            payload = JsonSerializer.Serialize(dict);
                        }
                    }
                    
                    jsonPayload = JsonSerializer.Deserialize<JsonElement>(payload);
                } catch {
                    // Ignore or handle invalid JSON in query
                }
            }
            else
            {
                // Inject UserContext defaults for empty payloads
                jsonPayload = JsonSerializer.Deserialize<JsonElement>($"{{\"OrganizationId\": {userContext.OrganizationId}}}");
            }
            
            long branchId = Request.Headers.TryGetValue("X-Branch-Id", out var bStr) && long.TryParse(bStr, out var b) ? b : 0;
            var result = await _dispatcher.DispatchAsync(AppModule.Billing, actionCode, jsonPayload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }

        [HttpPost("{actionCode}")]
        public async Task<IActionResult> Post(int actionCode, [FromBody] JsonElement payload, [FromServices] MedfarLabs.Core.Domain.Interfaces.Security.IUserContext userContext)
        {
            long branchId = Request.Headers.TryGetValue("X-Branch-Id", out var bStr) && long.TryParse(bStr, out var b) ? b : 0;
            var result = await _dispatcher.DispatchAsync(AppModule.Billing, actionCode, payload, HttpContext.TraceIdentifier, branchId);
            return Ok(result);
        }
    }
}
