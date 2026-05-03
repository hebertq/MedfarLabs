using Blazored.LocalStorage;
using MedFarLab.Application.Common.Interfaces;
using MedFarLab.Application.Common.Resilience;
using MedFarLab.Application.Common.Services;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Infrastructure.Http.Services.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MedFarLab.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // En MedFarLab.Pwa/Program.cs
            services.AddBlazoredLocalStorage();
            services.AddScoped<IOfflineStorage, BrowserOfflineStorage>();

            // El orquestador de sincronización
            services.AddScoped<SyncManager>();
            // Registramos el manejador genérico que definimos antes
            services.AddScoped(typeof(OfflineCommandHandler<>));

            return services;
        }
    }
}
