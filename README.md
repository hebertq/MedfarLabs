# 🏥 MedfarLab.Core

![Rest API](https://img.shields.io/badge/Rest--API-Blue) ![Lambda](https://img.shields.io/badge/AWS--Lambda-Orange) ![dotnet](https://img.shields.io/badge/dotnet-8.0-blue)

**MedfarLabs.Core** es el núcleo arquitectónico centralizado para un ecosistema de salud SaaS distribuido. Proporciona una estructura robusta basada en **Domain-Driven Design (DDD)** y un motor de ejecución híbrido denominado **Universal Worker** para maximizar la eficiencia en entornos Serverless.

---

## 📦 Estructura de Paquetes NuGet

El Core se distribuye en paquetes independientes para permitir que cada microservicio consuma solo las capas necesarias.

| Paquete | Responsabilidad | Dependencia Base |
| :--- | :--- | :--- |
| **`MedfarLabs.Core.Domain`** | Entidades base, Enums globales e interfaces de repositorio. | Ninguna |
| **`MedfarLabs.Core.Application`** | Dispatchers, servicios de aplicación y el `UniversalHandler`. | `Domain` |
| **`MedfarLabs.Core.Infrastructure`** | Persistencia (Dapper), AWS SQS y Seguridad AES. | `Application` |
| **`MedfarLabs.Core.Migrations`** | Motor de base de datos `DbUp` y scripts SQL fundacionales. | `Infrastructure` |
| **`MedfarLabs.Core.SharedFakers`** | Utilidades de testing con Bogus y `MasterSeeder` para integración. | `Application` |

---

## 🏗️ Arquitectura: El Universal Worker

El sistema implementa un patrón de **Punto de Entrada Único** a través de la clase `UniversalHandler.cs`. Una sola Lambda puede asumir dos roles dinámicamente según el disparador (Trigger) de AWS:

### 1. Contexto Sincrónico (Main)
* **Activador:** AWS API Gateway.
* **Flujo:** Validación de permisos $\rightarrow$ Ejecución de Dominio $\rightarrow$ Persistencia $\rightarrow$ Respuesta.
* **Salida Secundaria:** Encola automáticamente la acción en SQS para procesamiento de fondo vía `QueueOutputAction.cs`.

### 2. Contexto Asincrónico (Worker)
* **Activador:** Amazon SQS.
* **Flujo:** Recupera el `TraceId` original del mensaje y ejecuta tareas de fondo como Notificaciones, Auditoría subordinada y Telemetría.
* **Recursividad:** Soporta hasta 3 niveles de profundidad para evitar bucles infinitos de ejecución.

---

## 🚀 Guía de Implementación para Nuevos Módulos

Para extender la funcionalidad en un microservicio consumidor, siga este estándar:

### 1. Definir el Contrato (DTO)
Utilice el atributo `ActionMapping` para vincular el DTO a un módulo y acción específicos, implementando interfaces de blindaje de identidad:

```csharp
[ActionMapping(AppModule.Identity, AppAction.RegistrarPersona)]
public record PersonRequestDTO(
    [property: JsonPropertyName("primer_nombre")] string FirstName,
    [property: JsonPropertyName("correo")] string Email
) : IHasOrganization, IHasUser; 
```

### 2. Definir Reglas de Negocio
Hereda de BaseValidationRuleSet<T> para definir validaciones estructurales y de integridad:

```csharp
public class RegistrarPersonaRules : BaseValidationRuleSet<PersonRequestDTO>
{
    public override async Task ExecuteValidationAsync() 
    {
        ValidateStructuralRules(); // Ejecuta reglas básicas de formato
        // Lógica de base de datos adicional aquí...
        ThrowIfInvalid(); // Lanza BusinessValidationException si hay fallos
    }
}
```

### 3. Implementar la Estrategia de Dominio
Cree una clase que herede de BaseDomain para orquestar la vinculación automática entre acciones y servicios:

```csharp
public class IdentityDomain : BaseDomain
{
    public IdentityDomain(IServiceProvider sp) : base(typeof(IdentityDomain).Assembly, sp) 
    {
        // Registro de Mapeos: Acción -> Servicio -> Método
        RegisterActionHandler<IIdentityService>(AppAction.RegistrarPersona, nameof(IIdentityService.RegistrarPersonaAsync));
    }
    public override AppModule Module => AppModule.Identity;
}
```
## ⚡ Optimización de Alto Rendimiento
Para eliminar la latencia de la reflexión en .NET, el motor utiliza Delegados Compilados (Compiled Expressions) en la clase DomainReflectionHelper.cs. Esto garantiza que la instanciación de validadores y la invocación de servicios se realicen a una velocidad cercana al código nativo, optimizando los costos de ejecución en AWS Lambda.

## 🧪 Estrategia de Testing de Integración
El paquete SharedFakers permite realizar pruebas completas de infraestructura usando Testcontainers para PostgreSQL:

* **Aislamiento:** DbCleaner.cs vacía el esquema antes de cada test.
* **Jerarquía:** MasterSeeder.cs puebla una organización médica completa con pacientes, consultas y laboratorios.
* **Simulación E2E:** Los tests envían un JsonElement al UniversalHandler simulando la entrada real de AWS API Gateway.

```csharp
[Test]
public async Task Registrar_Debe_DetectarDuplicados() 
{
    var response = (APIGatewayProxyResponse)await _handler.FunctionHandler(jsonInput, mockContext);
    Assert.That(response.StatusCode, Is.EqualTo(400)); 
}
```
## ⚙️ Configuración en el Microservicio
Registre todo el ecosistema del Core en su capa de inicio (Program.cs o ConfigureServices.cs):

```csharp
// 1. Registro de Dispatcher, Estrategias y Reglas
builder.Services.AddActionDispatching(); 

// 2. Registro de Persistencia, Dapper y Seguridad AES
builder.Services.AddInfrastructureServices(connString, aesKey, salt);
```
// RECOMENDACIÓN: Cambiar Scoped por Singleton para evitar reflexiones repetitivas
services.Scan(scan => scan
    .FromAssemblies(assembly)
    .AddClasses(classes => classes.AssignableTo<IDomain>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime()); // <--- Mucho más eficiente


