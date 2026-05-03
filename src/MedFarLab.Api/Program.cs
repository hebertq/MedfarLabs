using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) => {
    options.ValidateOnBuild = false;
    options.ValidateScopes = false;
});

// 0. Instanciamos Swagger UI en lugar del básico
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    // Incluir XML de la Capa de Aplicación Base
    var appXmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, "MedFarLab.Application.xml");
    if (System.IO.File.Exists(appXmlPath)) c.IncludeXmlComments(appXmlPath);
    
    // Incluir XML de la Capa Core (donde residen los DTOs)
    var coreXmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Application.xml");
    if (System.IO.File.Exists(coreXmlPath)) c.IncludeXmlComments(coreXmlPath);

    // Inyectamos el inyector de rutas dinámicas por ActionMapping en Swagger
    c.DocumentFilter<MedFarLab.Api.Swagger.ActionDispatcherDocumentFilter>();
});

// --- CONEXIÓN AL NÚCLEO CORE (Importado vía NuGet / Custom) ---
builder.Services.AddApplicationServices();
builder.Services.AddActionDispatching();
builder.Services.AddEventHandlers();

builder.Services.AddInfrastructureServices(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not configured in appsettings.json"),
    builder.Configuration["SecuritySettings:EncryptionKey"] ?? throw new InvalidOperationException("EncryptionKey not configured"),
    builder.Configuration["SecuritySettings:HashSalt"] ?? throw new InvalidOperationException("HashSalt not configured")
);

// Override IUserContext to solve ActionDispatcher Scoping bug
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MedfarLabs.Core.Domain.Interfaces.Security.IUserContext, MedFarLab.Api.Security.HttpUserContext>();

// Registering CQRS / Clean Architecture manually since NuGet isn't doing it yet
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MedFarLab.Application.Features.Billing.Queries.GetAllInvoicesQuery).Assembly));

// Satisfy DI Validation for MediatR Auto-Discovery
builder.Services.AddHttpClient<MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient, MedfarLabs.Core.Infrastructure.Http.Services.Generic.ExternalServiceClient>(client => 
{
    client.BaseAddress = new Uri("http://localhost:5030/"); // Dummy for API itself
});
builder.Services.AddTransient(typeof(MedFarLab.Application.Common.Resilience.OfflineCommandHandler<>));
builder.Services.AddTransient<MedfarLabs.Core.Domain.Interfaces.IReportGenerator, MedfarLabs.Core.Reporting.ReportGenerator>();

// 1. Añadimos Soporte a Controladores Clásicos REST (Para comunicarse con PWA) y Filtros
builder.Services.AddControllers(options => 
{
    options.Filters.Add<MedFarLab.Api.Filters.ApiExceptionFilterAttribute>();
});

builder.Services.AddCors();

// Solución DI temporal local para suplir IAmazonSQS requerido por QueueOutputAction
builder.Services.AddSingleton<Amazon.SQS.IAmazonSQS>(new Amazon.SQS.AmazonSQSClient("dummy_key", "dummy_secret", Amazon.RegionEndpoint.USEast1));

var app = builder.Build();

// --- INICIALIZAR CACHÉ DE SEGURIDAD ---
using (var scope = app.Services.CreateScope())
{
    var securityRepo = scope.ServiceProvider.GetRequiredService<MedfarLabs.Core.Domain.Interfaces.Repositories.Security.ISecurityRepository>();
    var cache = scope.ServiceProvider.GetRequiredService<MedfarLabs.Core.Domain.Interfaces.Security.IGlobalSecurityCache>();
    cache.InitializeAsync(securityRepo).GetAwaiter().GetResult();
}

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
app.UseMiddleware<MedfarLabs.Core.Domain.Common.Exceptions.ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedFarLab B2B API v1");
        c.DisplayOperationId(); // Opcional
    });
}

app.UseHttpsRedirection();
app.UseAuthorization(); // Vital para B2B

app.UseMiddleware<MedFarLab.Api.Security.SessionAuthMiddleware>();

// --- ENDPOINTS Y CONTROLADORES ---
app.MapControllers();

// Redirección amigable desde la raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

public partial class Program { }
