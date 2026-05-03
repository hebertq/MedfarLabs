using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MedfarLabs.Core.Domain.Interfaces;
using MedfarLabs.Core.Reporting;
using System.Reflection;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) => {
    options.ValidateOnBuild = false;
    options.ValidateScopes = false;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => 
{
    c.DocumentFilter<MedFarLab.Reporting.Api.Swagger.ActionDispatcherDocumentFilter>();
});

// Configure MediatR for CQRS Handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MedFarLab.Application.Features.Billing.Queries.GetAllInvoicesQuery).Assembly));

// Add Reporting Services specifically
builder.Services.AddTransient<IReportGenerator, ReportGenerator>();

// Add the Action Dispatching system
// Inyectar los servicios CQRS del Core
builder.Services.AddApplicationServices();
builder.Services.AddActionDispatching();

// === SE AGREGA INFRAESTRUCTURA PARA QUE EL DISPATCHER NO FALLE ===
builder.Services.AddInfrastructureServices(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=medfarlab;Username=medfarlab;Password=root765*",
    builder.Configuration["SecuritySettings:EncryptionKey"] ?? "12345678901234567890123456789012",
    builder.Configuration["SecuritySettings:HashSalt"] ?? "MySuperSecretSalt123"
);

// CORS explicitly open for PWA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- INICIALIZAR CACHÉ DE SEGURIDAD REQUERIDA POR DISPATCHER ---
using (var scope = app.Services.CreateScope())
{
    var securityRepo = scope.ServiceProvider.GetRequiredService<MedfarLabs.Core.Domain.Interfaces.Repositories.Security.ISecurityRepository>();
    var cache = scope.ServiceProvider.GetRequiredService<MedfarLabs.Core.Domain.Interfaces.Security.IGlobalSecurityCache>();
    cache.InitializeAsync(securityRepo).GetAwaiter().GetResult();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<MedfarLabs.Core.Domain.Common.Exceptions.ExceptionHandlingMiddleware>();
app.UseMiddleware<MedFarLab.Reporting.Api.Security.SessionAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
