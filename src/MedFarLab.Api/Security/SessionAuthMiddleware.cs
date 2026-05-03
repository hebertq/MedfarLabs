using Microsoft.AspNetCore.Http;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Identity;
using MedfarLabs.Core.Domain.Interfaces.Security;

namespace MedFarLab.Api.Security
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository, IUserContext userContext)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Ignorar CORS Preflight o Swagger
            if (context.Request.Method == "OPTIONS" || path.Contains("/swagger") || path.Contains("api/Auth/Login"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Auth-Token", out var tokenValues) || string.IsNullOrEmpty(tokenValues.FirstOrDefault()))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Falta el token de sesión X-Auth-Token" });
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-User-Id", out var userIdValues) || !long.TryParse(userIdValues.FirstOrDefault(), out long userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Falta o es inválido el X-User-Id" });
                return;
            }

            long branchId = 0;
            if (context.Request.Headers.TryGetValue("X-Branch-Id", out var branchIdValues) && long.TryParse(branchIdValues.FirstOrDefault(), out var parsedBranchId))
            {
                branchId = parsedBranchId;
            }

            var activeToken = tokenValues.FirstOrDefault();
            


            // Validar concurrencia leyendo de base de datos para usuarios reales
            var user = await userRepository.GetByIdAsync(userId);
            
            if (user == null || !user.IsActive)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Usuario inactivo o no existe." });
                return;
            }

            if (string.IsNullOrEmpty(user.SessionToken) || user.SessionToken != activeToken)
            {
                // Otro dispositivo inició sesión y cambió el SessionToken de la BD
                // Por lo tanto, cerramos sesión a este dispositivo.
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Sesión inválida o expirada porque ha iniciado sesión en otro equipo." });
                return;
            }

            // --- INJECT TO SYSTEM ---
            // Poblar el IUserContext real del Action Dispatcher con los datos de Base de Datos
            userContext.UserId = user.Id;
            userContext.OrganizationId = user.OrganizationId;
            userContext.BranchId = branchId;
            
            // Populate HttpContext.Items for HttpUserContext fallback due to scope issues
            context.Items["UserId"] = user.Id;
            context.Items["OrganizationId"] = user.OrganizationId;
            context.Items["BranchId"] = branchId;
            
            await _next(context);
        }
    }
}
