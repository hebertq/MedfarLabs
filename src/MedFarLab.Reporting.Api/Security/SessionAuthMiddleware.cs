using Microsoft.AspNetCore.Http;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Identity;
using MedfarLabs.Core.Domain.Interfaces.Security;
using System.Linq;
using System.Threading.Tasks;

namespace MedFarLab.Reporting.Api.Security
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
            if (context.Request.Method == "OPTIONS" || path.Contains("/swagger"))
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

            var activeToken = tokenValues.FirstOrDefault();
            
            bool isMockUser = userId >= 995;
            if (isMockUser)
            {
                userContext.UserId = userId;
                
                // Set dummy organization for mock users
                userContext.OrganizationId = userId switch
                {
                    998 => 1, // clinicadmin
                    997 => 1, // labadmin
                    996 => 1, // pharmacyadmin
                    995 => 1, // fulladmin
                    _ => 1    // masteradmin
                };
                
                await _next(context);
                return;
            }

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
            
            await _next(context);
        }
    }
}
