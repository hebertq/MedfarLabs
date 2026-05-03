using MedFarLab.Pwa;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddTransient<MedFarLab.Pwa.Http.TokenDelegatingHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7015/";

builder.Services.AddHttpClient<MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient, MedfarLabs.Core.Infrastructure.Http.Services.Generic.ExternalServiceClient>(client => 
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<MedFarLab.Pwa.Http.TokenDelegatingHandler>();

builder.Services.AddHttpClient("ReportingApi", client => 
{
    client.BaseAddress = new Uri("http://localhost:5050/");
}).AddHttpMessageHandler<MedFarLab.Pwa.Http.TokenDelegatingHandler>();

// Inyección de Lógica CQRS (Como Proxies HTTP hacia el API)
MedFarLab.Application.ConfigureServices.AddApplicationServices(builder.Services);

// Singleton state manager for the frontend mockup
builder.Services.AddSingleton<MedFarLab.Pwa.State.AppState>();

// Servicio de Cifrado para Almacenamiento Offline (Navegador)
builder.Services.AddSingleton<MedfarLabs.Core.Infrastructure.Common.Interfaces.IEncryptionService>(
    new MedfarLabs.Core.Infrastructure.Shared.Security.AesEncryptionService("PwaOfflineLocalKey#2026!MedFarLab1")
);

// Servicio de Notificaciones
builder.Services.AddScoped<MedFarLab.Pwa.Services.NotificationService>();

builder.Services.AddMudServices();
builder.Services.AddScoped<MedFarLab.Pwa.Services.IExportService, MedFarLab.Pwa.Services.ExportService>();
builder.Services.AddApplicationServices();
builder.Services.AddReportingServices();

// Configuramos los servicios de Autorización Mock de forma segura
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MedFarLab.Pwa.Providers.MockAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();
