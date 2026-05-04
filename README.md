# 🏥 MedfarLabs - Plataforma SaaS de Salud Integral

![API REST](https://img.shields.io/badge/API%20REST-ASP.NET%20Core-blue) ![PWA](https://img.shields.io/badge/PWA-Blazor%20WebAssembly-purple) ![Multitenancy](https://img.shields.io/badge/Multitenancy-SaaS-green) ![.NET](https://img.shields.io/badge/.NET-8.0-512bd4)

**MedfarLabs** es una solución completa de software de gestión para laboratorios clínicos, clínicas y farmacias. Integra una **API REST empresarial** con una **Progressive Web App (PWA)** moderna, proporcionando un ecosistema SaaS multitenant escalable, seguro y conforme con estándares médicos internacionales.

> 💡 **Nota:** Este repositorio es la **solución cliente** (API + PWA). La lógica de dominio centralizada se encuentra en el paquete NuGet **[MedfarLabs.Core](https://github.com/hebertq/MedfarLabs.Core)**.

---

## 📋 Índice

- [🎯 Características Principales](#características-principales)
- [🏗️ Arquitectura General](#arquitectura-general)
- [📦 Componentes de la Solución](#componentes-de-la-solución)
- [🔄 Flujos de Negocio Principales](#flujos-de-negocio-principales)
- [⚙️ Estándares de Desarrollo](#estándares-de-desarrollo)
- [🚀 Guía de Inicio Rápido](#guía-de-inicio-rápido)
- [🧪 Testing](#testing)
- [📚 Documentación](#documentación)

---

## 🎯 Características Principales

### 🏥 Módulos Funcionales

| Módulo | Descripción | Actores |
|:-------|:-----------|:--------|
| **👤 Identidad & Seguridad** | Gestión de usuarios, roles (RBAC) y permisos granulares | Admin, Médicos, Staff |
| **🧬 Laboratorio** | Gestión completa de órdenes, muestras y resultados con análisis automatizados | Técnicos, Bioanalistas |
| **👨‍⚕️ Clínica** | Historias clínicas, citas médicas, consultas y prescripciones | Médicos, Recepcionistas |
| **💊 Farmacia** | Dispensación de medicamentos, control de inventario, venta de insumos | Farmacéuticos |
| **📦 Inventario** | Stock de reactivos, insumos, medicamentos con alertas de reabastecimiento | Almacenistas |
| **💰 Facturación** | Pagos unificados, integración con seguros, conciliación de caja | Cajeros, Contables |
| **📊 Reportería** | Informes clínicos, PDFs firmados digitalmente, cierre diario | Administradores |
| **⚙️ Sistema** | Configuración de planes, menús dinámicos, auditoría y trazabilidad | Super Admins |

### 🌟 Características Técnicas

✅ **Multitenancy SaaS:** Aislamiento de datos por organización con seguridad garantizada  
✅ **API Event-Driven:** Comunicación asincrónica mediante eventos de dominio y SQS  
✅ **PWA Offline-First:** Sincronización automática cuando hay conexión  
✅ **Seguridad Enterprise:** JWT, RBAC, cifrado AES, auditoría de acciones  
✅ **DDD (Domain-Driven Design):** Arquitectura limpia y escalable  
✅ **CQRS & MediatR:** Separación clara entre comandos y consultas  
✅ **Idempotencia:** Prevención de duplicados en operaciones críticas  
✅ **Native AOT Compatible:** Optimizado para Serverless (AWS Lambda)  

---

## 🏗️ Arquitectura General

```
┌─────────────────────────────────────────────────────────────┐
│                    Cliente Web/Móvil                         │
│             (PWA con Blazor WebAssembly)                     │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP/HTTPS
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              MedFarLab.Api (ASP.NET Core 8)                  │
│  - Controladores REST                                        │
│  - Middleware de Autorización (SessionAuthMiddleware)       │
│  - Action Dispatcher (IApplicationDispatcher)               │
│  - Manejo de Excepciones Global                             │
└────────────────────┬────────────────────────────────────────┘
                     │ (IPC / NuGet)
                     ▼
┌─────────────────────────────────────────────────────────────┐
│         MedfarLabs.Core (NuGet Package)                       │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐     │
│  │  Domain      │  │ Application  │  │Infrastructure │     │
│  │  - Entities  │  │ - Handlers   │  │ - Dapper ORM  │     │
│  │  - Specs     │  │ - CQRS       │  │ - AWS SQS     │     │
│  │  - Events    │  │ - Validation │  │ - AES Encrypt │     │
│  └──────────────┘  └──────────────┘  └───────────────┘     │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
    PostgreSQL   AWS SQS      Workers
```

---

## 📦 Componentes de la Solución

### 1. **MedFarLab.Api** - API REST Empresarial
**Ubicación:** `src/MedFarLab.Api`

**Responsabilidades:**
- Exponer endpoints REST para consumo desde PWA y clientes terceros
- Validación de JWT y autorización basada en roles (RBAC)
- Enrutamiento de peticiones al `IApplicationDispatcher`
- Generación de documentación Swagger/OpenAPI
- Manejo centralizado de excepciones

**Tecnologías:**
- ASP.NET Core 8
- MediatR (CQRS)
- Swagger/Swashbuckle
- JWT Bearer Authentication

**Ejemplo de Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("type/{organizationTypeId}")]
    public async Task<IActionResult> GetMenusByOrgType(int organizationTypeId)
    {
        var query = new GetMenusByOrganizationTypeQuery(organizationTypeId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### 2. **MedFarLab.Pwa** - Progressive Web App
**Ubicación:** `src/MedFarLab.Pwa`

**Responsabilidades:**
- Interfaz responsive para desktop, tablet y móvil
- Funcionamiento offline con sincronización automática
- Gestión de estado global (`AppState`)
- Componentes reutilizables con patrón Smart/Dumb
- Navegación dinámica según roles del usuario

**Tecnologías:**
- Blazor WebAssembly (.NET 8)
- MudBlazor (componentes Material Design)
- LocalStorage para persistencia offline
- Service Worker para PWA

**Estructura de Componentes:**
```
Pages/
├── Patient/
│   ├── PatientRecord.razor          (Smart Component - Lógica)
│   ├── PatientRecord.razor.cs       (Code-Behind)
│   ├── PatientRecord.razor.css      (Estilos Scoped)
│   └── PatientContactModal.razor    (Dumb Component - UI)
├── Lab/
└── Billing/
```

### 3. **MedfarLabs.Core** - Núcleo Compartido (NuGet)
**Ubicación:** Repositorio separado

**Componentes:**
- **Domain:** Entidades, especificaciones, eventos de dominio
- **Application:** Handlers, Dispatchers, Validadores
- **Infrastructure:** Persistencia (Dapper), SQS, Seguridad AES
- **Migrations:** Scripts SQL con DbUp

---

## 🔄 Flujos de Negocio Principales

La plataforma implementa **22 flujos de negocio** documentados en la carpeta `/flujos`:

### 📌 Flujo 01: Admisión, Facturación y Orden Médica
**Actores:** Recepcionista, Cajero, Paciente

1. **Identificación:** Búsqueda o registro del paciente en módulo Care
2. **Selección de Exámenes:** Se agregan servicios al carrito
3. **Validación:** Sistema verifica disponibilidad de insumos/reactivos
4. **Facturación:** Generación de factura en módulo Billing
5. **Pago:** Registro del ingreso (Efectivo, Tarjeta, Crédito)
6. **Orden:** Disparo de evento `PaymentConfirmedIntegrationEvent` → Laboratorio crea orden

### 📌 Flujo 02: Gestión de Muestras y Resultados
**Actores:** Técnico de Laboratorio, Bioanalista, Supervisor

1. **Toma de Muestra:** Ejecución de `CollectSampleCommand`
2. **Recepción:** Cambio de estado a "En Proceso"
3. **Carga de Resultados:** Validación automática contra rangos (Edad/Sexo)
4. **Revisión y Firma:** Supervisor valida → dispara `LabTestValidatedEvent`

### 📌 Flujo 09: Procesamiento Unificado de Pagos
**Concepto:** Consolidación de cargos de múltiples fuentes antes de facturar

- **Fuentes de Cargo:** Care (consultas), Laboratory (exámenes), Inventory (medicinas)
- **Aplicación de Beneficios:** Descuentos por plan del paciente
- **Idempotencia:** Frontend envía `RequestId` para evitar cobros duplicados
- **Evento de Cierre:** Al confirmar pago → estados cambian automáticamente en Lab y Care

### 📌 Flujo 14: Control de Acceso (RBAC) y Auditoría
**Niveles de Validación:**
1. **Tenant Level:** Verifica módulo activo según plan
2. **Role Level:** Verifica si el rol tiene la acción permitida
3. **Owner Level:** Verifica acceso al registro específico
4. **Auditoría:** Todos los accesos se registran en `ActionEventRepository`

### 📌 Flujo 17: Notificaciones de Resultados
**Disparador:** `LabTestValidatedEvent`

1. **Patrón Outbox:** Mensajes en tabla `OutboxMessages` garantizan entrega
2. **Email Handler:** PDF con resultados → email cifrado al paciente
3. **PWA Webhook:** Notificación push al Service Worker
4. **Reintentos:** Hasta 3 intentos con Circuit Breaker (Polly)

**Para ver todos los flujos documentados:**
```bash
dir flujos/
# 01-Admision-Facturacion.md.md
# 02-Fase-Analitica-Laboratorio.md.md
# 03-Sincronizacion-Offline.md.md
# ... y más
```

---

## ⚙️ Estándares de Desarrollo

La carpeta `.skills/` contiene directivas para mantener código de grado médico.

### 🎯 Backend Standards

**Ubicación:** `.skills/backend-standards.md`

#### Controladores Delgados (Thin Controllers)
```csharp
[HttpPost("crear")]
public async Task<IActionResult> Crear([FromBody] CrearPacienteRequestDTO request)
{
    var response = await _dispatcher.DispatchAsync<long>(request);
    return response.IsSuccess ? Ok(response) : BadRequest(response);
}
```

**Reglas:**
- ✅ Nunca contener lógica de negocio en controladores
- ✅ Delegar TODO al `IApplicationDispatcher`
- ✅ Retornar siempre `BaseResponse<T>`
- ✅ Usar JWT para extraer `TenantId`, `BranchId`, `UserId`
- ✅ NO bloquear con `.Result` o `.Wait()` en código async
- ✅ Usar `IOutputAction` para tareas asincrónicas (emails, logs)

### 🎨 Frontend Standards

**Ubicación:** `.skills/frontend-standards.md`

#### Patrón Smart & Dumb Components
```
Smart Component (Página)
├── Inyecta servicios [Inject]
├── Maneja ciclo de vida OnInitializedAsync
├── Obtiene datos
└── Pasa a componentes hijos via [Parameter]
    │
    └── Dumb Component (UI)
        ├── Solo recibe [Parameter]
        ├── Emite eventos via EventCallback
        └── CERO inyección de dependencias
```

#### Patrón Code-Behind (Obligatorio)
```
NombrePagina.razor          → HTML + Componentes
NombrePagina.razor.cs       → Lógica C# (clase partial)
NombrePagina.razor.css      → Estilos Scoped
```

**Ejemplo:**
```csharp
// PatientRecord.razor.cs
public partial class PatientRecord
{
    [Inject] public HttpClient HttpClient { get; set; }
    [Inject] public AppState AppState { get; set; }
    
    private PatientModel patient;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        patient = await HttpClient.GetFromJsonAsync(/* ... */);
        isLoading = false;
    }
}
```

### 🏥 MedfarLab Elite Directives

**Ubicación:** `.skills/medfarlab-elite-directives.md`

#### Mandamiento del Triple Archivo
```
✅ PERMITIDO:   Nombre.razor (HTML) + Nombre.razor.cs + Nombre.razor.css
❌ PROHIBIDO:   Bloques @code de más de 5 líneas en .razor
```

#### Skeleton First (No Spinners)
```csharp
@if (IsLoading)
{
    <MedFarSkeleton Type="PatientRecord" />
}
else
{
    <PatientDetails Patient="patient" />
}
```

#### UI Premium (Glassmorphism)
```css
.surface-glass {
    backdrop-filter: blur(12px);
    background: rgba(255, 255, 255, 0.7);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}
```

#### Seguridad Clínica (Poka-Yoke)
```razor
<!-- Alergias SIEMPRE visibles -->
<div class="sticky-top alert-critical">
    @if (patient.Allergies.Any())
    {
        <MudAlert Severity="Severity.Error">
            ⚠️ @string.Join(", ", patient.Allergies)
        </MudAlert>
    }
</div>
```

#### Trazabilidad (TraceId)
```csharp
// Todo error debe incluir TraceId para soporte
if (!response.IsSuccess)
{
    _snackbar.Add($"Error: {response.Message} [TraceId: {response.TraceId}]");
}
```

---

## 🚀 Guía de Inicio Rápido

### Requisitos Previos
- **.NET 8 SDK** o superior
- **PostgreSQL 14+**
- **Node.js 18+** (opcional, si usas build tools adicionales)
- **Git**

### 1. Clonar el Repositorio

```bash
git clone https://github.com/hebertq/MedfarLabs.git
cd MedfarLabs
```

### 2. Configurar Variables de Entorno

Crear archivo `appsettings.Development.json` en `src/MedFarLab.Api/`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=medfarlab_dev;Username=postgres;Password=your_password"
  },
  "SecuritySettings": {
    "EncryptionKey": "your_32_char_encryption_key_here",
    "HashSalt": "your_salt_here"
  },
  "Jwt": {
    "Secret": "your_jwt_secret_key_minimum_32_characters",
    "Issuer": "MedfarLabs",
    "Audience": "MedfarLabsClients"
  }
}
```

### 3. Restaurar Dependencias

```bash
dotnet restore
```

### 4. Aplicar Migraciones (via MedfarLabs.Core)

```bash
dotnet ef database update --project src/MedFarLab.Api
# O si usas DbUp
dotnet run --project src/MedfarLabs.Core.Migrations
```

### 5. Ejecutar la API

```bash
dotnet run --project src/MedFarLab.Api
```

API disponible en: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

### 6. Ejecutar la PWA (en otra terminal)

```bash
cd src/MedFarLab.Pwa
dotnet watch run
```

PWA disponible en: `https://localhost:5003`

### 7. Login Inicial

**Usuario:** `admin@medfarlab.local`  
**Contraseña:** `InitialPassword123!`

(Cambiar en primer acceso)

---

## 🧪 Testing

### Tests de Integración

**Ubicación:** `tests/MedfarLabs.Core.IntegrationTests`

```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true
```

**Características:**
- Testcontainers para PostgreSQL aislado
- `MasterSeeder` para datos de prueba
- `DbCleaner` para limpieza entre tests
- Simulación E2E del `UniversalHandler`

**Ejemplo:**
```csharp
[Test]
public async Task Registrar_Debe_DetectarDuplicados()
{
    var response = (APIGatewayProxyResponse)await _handler.FunctionHandler(
        jsonInput, mockContext
    );
    Assert.That(response.StatusCode, Is.EqualTo(400));
}
```

### Tests de PWA

```bash
cd src/MedFarLab.Pwa
# Ejecutar con Selenium/Playwright (configurar según necesidad)
dotnet test
```

---

## 📚 Documentación

### Carpetas Clave

| Carpeta | Contenido |
|---------|-----------|
| `flujos/` | 22 diagramas y especificaciones de flujos de negocio |
| `.skills/` | Estándares de código (backend, frontend, directives) |
| `docs/` | Documentación técnica adicional |
| `src/` | Código fuente (API + PWA + Application) |
| `tests/` | Suite de tests de integración |

### Documentos Importantes

- **Flujos de Negocio:** `flujos/README.md` (si existe)
- **Backend:** `.skills/backend-standards.md`
- **Frontend:** `.skills/frontend-standards.md`
- **Directives:** `.skills/medfarlab-elite-directives.md`
- **Contribuciones:** `CONTRIBUTING.md`

### Dependencia Externa: MedfarLabs.Core

Para detalles sobre la arquitectura interna, validadores, handlers y especificaciones de dominio:

👉 [Visita el repositorio MedfarLabs.Core](https://github.com/hebertq/MedfarLabs.Core)

---

## 🔐 Seguridad

### Autenticación
- **JWT Bearer Tokens** con duración configurable
- **Claims:** `sub`, `tenant_id`, `branch_id`, `roles`
- **Renovación automática** en PWA antes de expiración

### Autorización
- **RBAC (Role-Based Access Control)** granular por acción
- **Data Isolation:** TenantId obligatorio en todas las queries
- **Soft Delete:** Registros nunca se eliminan, solo marcan como inactivos

### Encriptación
- **AES-256** para datos sensibles en BD
- **HTTPS obligatorio** en producción
- **Audit Trail:** Todos los accesos registrados con TraceId

---

## 🐛 Troubleshooting

### Problema: "Connection refused" a PostgreSQL
```bash
# Verificar que PostgreSQL está corriendo
docker ps | grep postgres

# O iniciar contenedor
docker run --name medfarlab-postgres \
  -e POSTGRES_PASSWORD=your_password \
  -p 5432:5432 \
  -d postgres:15
```

### Problema: JWT inválido
- Verificar que la clave `jwt.Secret` en `appsettings.json` es >= 32 caracteres
- Limpiar localStorage en PWA (F12 → Application → Storage → Clear All)

### Problema: Migraciones no se aplican
```bash
# Resetear base de datos completamente (DEV ONLY)
dotnet ef database drop --force
dotnet ef database update
```

---

## 📝 Licencia

[Define tu licencia aquí - MIT, Apache 2.0, etc.]

---

## 👥 Contribuidores

- **hebertq** - Arquitecto Principal

¿Quieres contribuir? Véase `CONTRIBUTING.md`

---

## 📞 Soporte

Para reportar bugs o solicitar features:
- 📧 Email: support@medfarlab.local
- 🐛 Issues: GitHub Issues en este repositorio
- 💬 Discussiones: GitHub Discussions

---

**Última actualización:** Mayo 2026  
**Versión:** 1.0.0-alpha
