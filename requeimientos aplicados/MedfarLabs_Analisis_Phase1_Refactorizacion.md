# MedfarLabs · Análisis de Refactorización — Entrega Phase 1
> Revisión del core actualizado: nuevos módulos, repositorios y correcciones  
> Mayo 2025

---

## 0. Resumen ejecutivo

Esta entrega representa un avance importante. Se incorporaron **3 módulos clínicos nuevos** (Alertas, Contactos de Paciente, Auditoría de Acceso), se resolvieron **5 de los 7 bugs críticos** identificados en el análisis anterior, y las constantes y el domain de Billing quedaron completamente alineados. Sin embargo persisten algunos problemas estructurales que hay que corregir antes de pasar a producción.

| Área | Estado anterior | Estado actual |
|---|---|---|
| Alertas clínicas | ❌ No existía | ✅ Completo (servicio, repo, domain, SQL) |
| Contactos de paciente | ❌ No existía | ✅ Completo |
| Auditoría de acceso a paciente | ❌ No existía | ✅ Completo |
| `BuscarFactura` en domain | 🔴 No registrado | ✅ Registrado y con método |
| `AnularFactura` en domain | 🔴 No registrado | ✅ Registrado |
| `GetAllInvoices` con `GetAllAsync()` | 🔴 Bug crítico | ✅ Corregido con `GetByOrganizationAsync` |
| `ActualizarFactura` con `GetAllAsync()` | 🔴 Bug crítico | ✅ Corregido con `GetByInvoiceIdAsync` |
| `GetInvoicePayments`, `GetPatientBalance`, `GetDailyClosing` | ❌ No existía | ✅ Acciones, DTOs, domain y repos nuevos |
| Estados de factura (enum desalineado) | 🔴 Bug | ⚠️ Parcialmente corregido |
| `GetInvoicePaymentsAsync` retorna `object` sin tipo | — | 🔴 Nuevo problema |
| `GetInvoicePayments` repo usa `GetAllAsync()` | — | 🔴 Nuevo bug introducido |
| Firma de métodos en `IClinicalAlertService` | — | 🔴 Incompatible con el dispatcher |
| `[ActionMapping]` en DTOs de alertas/contactos | — | 🔴 Faltante en todos |
| `PatientAlertEntity.CreatedBy` vs columna SQL `created_by_user_id` | — | 🔴 Mismatch |
| Validadores de facturas vacíos | 🔴 TODO | ⚠️ Sin cambios |

---

## 1. Lo que llegó bien — avances confirmados

### 1.1 Módulo de Alertas Clínicas — completo

Flujo completo y bien estructurado:
- **Entidad:** `PatientAlertEntity` con severidad, tipo, fuente, acknowledgment
- **Repositorio:** `PatientAlertRepository` con SQL limpio y filtros correctos
- **Servicio:** `ClinicalAlertService` desacoplado de `ClinicalService` — correcto, sigue Single Responsibility
- **Domain:** Los 3 handlers registrados en `ClinicalDomain` (4114, 4115, 4116)
- **OutputAction:** `AlertNotificationOutputAction` con el patrón `IOutputAction` correcto
- **SQL:** Tabla `clinical.mst_patient_alert` con índices en `(patient_id, is_active)` y `(severity, is_active)` — buenas decisiones de rendimiento
- **Validador:** `CreatePatientAlertValidator` implementado con las 4 severidades correctas

### 1.2 Módulo de Contactos de Paciente — completo

- Soft delete correcto (UPDATE `is_active = FALSE` en vez de DELETE físico)
- `IsPrimary` con ordenamiento DESC en la consulta — el contacto principal siempre llega primero
- Separación limpia entre servicio e interface

### 1.3 Auditoría de acceso — completo

- `security.log_patient_access` con `TIMESTAMPTZ` (correcto para auditoría con zona horaria)
- Paginación nativa en SQL (`LIMIT @Limit OFFSET @Offset`)
- Índices en `patient_id`, `user_id` y `organization_id` con `created_at DESC`

### 1.4 Billing domain — ahora 100% conectado

Todos los handlers están registrados:
- `BuscarFactura` (3005) — ya tiene implementación y está en el domain
- `AnularFactura` (3002) — ya registrado
- `GetInvoicePayments`, `GetPatientBalance`, `GetDailyClosing` (3020, 3021, 3022)
- Toda la cadena de SaaS plans y subscriptions (3013–3019)

### 1.5 `InvoiceRepository` — nuevos métodos sólidos

- `GetByOrganizationAsync` — query paginada con filtros opcionales en SQL ✅
- `SearchAsync` — JOIN con `mst_patient` y `mst_catalog_detail`, filtros encadenados ✅
- `UpdateBalanceAsync` — maneja `paid_at` con CASE expresivo en SQL ✅
- `GetDailyClosingAsync` — GROUP BY con JOIN al catálogo de métodos de pago ✅
- `GetByInvoiceIdAsync` en `InvoiceItemRepository` ✅

---

## 2. Bugs nuevos introducidos en esta entrega

### 2.1 🔴 CRÍTICO — `GetInvoicePaymentsAsync` usa `GetAllAsync()` sin filtro

**Archivo:** `BillingService.cs`

```csharp
// ACTUAL — carga TODOS los pagos de la tabla en memoria
var payments = await _unitOfWork.Payments.GetAllAsync();
var invoicePayments = payments.Where(p => p.InvoiceId == request.InvoiceId 
    && p.OrganizationId == request.OrganizationId).ToList();
```

Es el mismo patrón que se corrigió en `GetAllInvoices` pero se reintrodujo aquí. Agregar en `IPaymentRepository` e implementar:

```csharp
Task<IEnumerable<Payment>> GetByInvoiceIdAsync(long invoiceId, long organizationId);
```

Implementación en `PaymentRepository.cs`:
```csharp
public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(long invoiceId, long organizationId)
{
    var sql = $"SELECT * FROM {_tableName} WHERE invoice_id = @InvoiceId AND organization_id = @OrgId ORDER BY payment_date DESC;";
    return await _connection.QueryAsync<Payment>(sql, new { InvoiceId = invoiceId, OrgId = organizationId }, _transaction);
}
```

Y en el servicio:
```csharp
public async Task<BaseResponse<IEnumerable<Payment>>> GetInvoicePaymentsAsync(GetInvoicePaymentsRequestDTO request)
{
    var payments = await _unitOfWork.Payments.GetByInvoiceIdAsync(request.InvoiceId, request.OrganizationId);
    return BaseResponse<IEnumerable<Payment>>.Success(payments);
}
```

### 2.2 🔴 CRÍTICO — `GetInvoicePaymentsAsync` retorna `BaseResponse<object>`

**Archivo:** `IBillingService.cs` e implementación

El método está declarado como:
```csharp
Task<BaseResponse<object>> GetInvoicePaymentsAsync(GetInvoicePaymentsRequestDTO request);
```

Esto rompe el AOT y pierde toda la información de tipos. Debe ser:
```csharp
Task<BaseResponse<IEnumerable<PaymentResponseDTO>>> GetInvoicePaymentsAsync(GetInvoicePaymentsRequestDTO request);
```

Crear `PaymentResponseDTO` en `src/Application/Features/Billing/Dtos/Response/PaymentResponseDTO.cs`:
```csharp
public record PaymentResponseDTO
{
    [JsonPropertyName("id")]               public long Id { get; init; }
    [JsonPropertyName("invoice_id")]       public long InvoiceId { get; init; }
    [JsonPropertyName("amount")]           public decimal Amount { get; init; }
    [JsonPropertyName("payment_method")]   public string? PaymentMethod { get; init; }
    [JsonPropertyName("payment_date")]     public DateTime PaymentDate { get; init; }
    [JsonPropertyName("reference")]        public string? TransactionReference { get; init; }
    [JsonPropertyName("status")]           public int StatusId { get; init; }
}
```

### 2.3 🔴 CRÍTICO — Firma de `IClinicalAlertService` incompatible con el dispatcher

**Archivo:** `IClinicalAlertService.cs`

El dispatcher de `BaseDomain` funciona así: deserializa el DTO y lo pasa como **primer argumento** al método del servicio. Pero los métodos del `ClinicalAlertService` tienen firmas como:

```csharp
// ACTUAL — incompatible con el dispatcher:
Task<BaseResponse<IEnumerable<PatientAlertResponseDTO>>> GetActiveAlertsAsync(
    long patientId, long organizationId, CancellationToken ct = default);

Task<BaseResponse<object>> AcknowledgeAlertAsync(
    AcknowledgeAlertRequestDTO request, long userId, CancellationToken ct = default);

Task<BaseResponse<PatientAlertResponseDTO>> CreateAlertAsync(
    CreatePatientAlertRequestDTO request, long userId, CancellationToken ct = default);
```

`GetActiveAlertsAsync` espera `(long, long)` — el dispatcher no puede rutar un DTO aquí.  
`AcknowledgeAlertAsync` y `CreateAlertAsync` esperan `(DTO, long userId)` — el dispatcher no pasa `userId` como segundo parámetro separado.

**Corrección** — alinear con el patrón del resto del sistema (DTO como único parámetro, `OrganizationId` y `UserId` ya inyectados en el DTO por `BaseDomain`):

```csharp
public interface IClinicalAlertService
{
    Task<BaseResponse<IEnumerable<PatientAlertResponseDTO>>> GetActiveAlertsAsync(
        GetPatientAlertsRequestDTO request);                    // DTO contiene PatientId + OrganizationId

    Task<BaseResponse<bool>> AcknowledgeAlertAsync(
        AcknowledgeAlertRequestDTO request);                    // DTO contiene AlertId + OrganizationId + UserId

    Task<BaseResponse<PatientAlertResponseDTO>> CreateAlertAsync(
        CreatePatientAlertRequestDTO request);                  // DTO contiene todo
}
```

Igualmente para `IPatientContactService`:
```csharp
// ACTUAL — incompatible:
Task<BaseResponse<IEnumerable<PatientContactResponseDTO>>> GetContactsAsync(
    long patientId, long organizationId, CancellationToken ct = default);

// CORRECTO:
Task<BaseResponse<IEnumerable<PatientContactResponseDTO>>> GetContactsAsync(
    GetPatientContactsRequestDTO request);
```

### 2.4 🔴 CRÍTICO — DTOs de alertas y contactos sin `[ActionMapping]`

**Archivos:** `CreatePatientAlertRequestDTO.cs`, `GetPatientAlertsRequestDTO.cs`, `AcknowledgeAlertRequestDTO.cs`, `GetPatientContactsRequestDTO.cs`, `CreatePatientContactRequestDTO.cs`, `UpdatePatientContactRequestDTO.cs`

El `BaseDomain` construye el `_dtoRegistry` usando reflexión sobre `[ActionMappingAttribute]`. Sin ese atributo el dispatcher retorna `"Acción XXXX no soportada"` aunque el handler esté registrado.

```csharp
// AGREGAR en cada DTO:
[ActionMapping(AppModule.Clinical, AppAction.Clinical.GetPatientAlerts)]
public record GetPatientAlertsRequestDTO(long PatientId) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.CreatePatientAlert)]
public record CreatePatientAlertRequestDTO(...) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.AcknowledgeAlert)]
public record AcknowledgeAlertRequestDTO(long AlertId) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.GetPatientContacts)]
public record GetPatientContactsRequestDTO(long PatientId) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.CreatePatientContact)]
public record CreatePatientContactRequestDTO(...) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.UpdatePatientContact)]
public record UpdatePatientContactRequestDTO(...) : IHasOrganization { ... }

[ActionMapping(AppModule.Clinical, AppAction.Clinical.DeletePatientContact)]
public record DeletePatientContactRequestDTO(long ContactId) : IHasOrganization { ... }
```

> **Nota:** `DeletePatientContact` (4120) no tiene DTO actualmente. Crear `DeletePatientContactRequestDTO` con `ContactId` e implementar con el mismo patrón.

### 2.5 🔴 Mismatch entre `PatientAlertEntity` y columna SQL `created_by_user_id`

**Entidad:** `PatientAlertEntity.CreatedBy { get; set; }` (nombre de propiedad)  
**SQL en repositorio:** `@CreatedByUserId` en el INSERT  
**Tabla SQL:** columna `created_by_user_id`

Dapper mapea por nombre de propiedad. La propiedad `CreatedBy` no coincide con `@CreatedByUserId`. El INSERT va a fallar silenciosamente insertando `NULL` en `created_by_user_id`.

**Corrección en la entidad:**
```csharp
// Cambiar:
public long CreatedBy { get; set; }
// Por:
public long CreatedByUserId { get; set; }
```

Y en `ClinicalAlertService.CreateAlertAsync`:
```csharp
// Cambiar:
CreatedBy = userId,
// Por:
CreatedByUserId = userId,
```

### 2.6 ⚠️ `InvoiceRepository.SearchAsync` tiene comentario SQL en producción

**Archivo:** `InvoiceRepository.cs`

```sql
-- Línea problemática en el SQL:
LEFT JOIN common.mst_catalog_detail cd ON i.status_id = cd.id AND cd.catalog_id = 1 
-- Assuming 1 is InvoiceStatus catalog or similar, or just map in code. 
-- Wait, if there's no catalog...
```

Este comentario con dudas quedó en el código de producción. Hay que decidir la estrategia y limpiar. Lo más simple dado que `InvoiceStatusEnum` ya existe en el core:

```csharp
// Opción recomendada: no hacer JOIN al catálogo para el status, 
// mapear el enum en C# al construir el DTO de respuesta:
StatusName = ((InvoiceStatusEnum)i.StatusId).ToString()
```

Y eliminar el JOIN a `mst_catalog_detail` en `SearchAsync`.

### 2.7 ⚠️ `GetDailyClosingRequestDTO` con formato de código roto

**Archivo:** `GetDailyClosingRequestDTO.cs`

El archivo tiene el código sin indentación y con saltos de línea extraños — parece que fue generado o pegado incorrectamente. Aunque compila, viola los estándares del proyecto. Reformatear al estándar del resto:

```csharp
[ActionMapping(AppModule.Billing, AppAction.Billling.GetDailyClosing)]
public record GetDailyClosingRequestDTO : IHasOrganization, IHasBranch
{
    [JsonIgnore] public long OrganizationId { get; set; }
    [JsonIgnore] public long BranchId { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; init; } = DateTime.Today;
}
```

### 2.8 ⚠️ `AlertNotificationOutputAction` — email service no inyectado

**Archivo:** `AlertNotificationOutputAction.cs`

El comentario en el código lo dice explícitamente:
```csharp
// Wait, email service is not injected because we don't have its definition right now. 
// Logging it as a placeholder to ensure the workflow completes without breaking.
```

`IEmailService` ya existe en `src/Application/Common/Interfaces/IEmailService.cs` e `EmailService.cs` tiene implementación. Inyectarlo:

```csharp
public class AlertNotificationOutputAction : IOutputAction
{
    private readonly ILogger<AlertNotificationOutputAction> _logger;
    private readonly IEmailService _emailService;          // AGREGAR

    public AlertNotificationOutputAction(
        ILogger<AlertNotificationOutputAction> logger,
        IEmailService emailService)                        // AGREGAR
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task ExecuteAsync(OutputContextDto context)
    {
        // ... validación de severidad CRITICAL/HIGH ...
        
        // Reemplazar el await Task.CompletedTask por:
        await _emailService.SendAlertEmailAsync(patientId, message, alertSeverity);
    }
}
```

---

## 3. Pendientes del análisis anterior no resueltos

### 3.1 Estados de factura — parcialmente resuelto

`GetPatientBalanceAsync` ya usa correctamente `status_id IN (1, 2, 5)` (Draft, Unpaid, PartiallyPaid), lo que confirma que el enum se corrigió a `Draft=1, Unpaid=2`. Pero en `BillingService.GenerarFacturaAsync` todavía se asigna:

```csharp
StatusId = 1, // 1: Emitida/Pendiente
```

Si `1 = Draft` y el estado correcto al emitir es `Unpaid = 2`, esto sigue incorrecto. Verificar y alinear con el enum.

### 3.2 Validadores de facturas — siguen vacíos con TODO

`InvoiceItemRules`, `PaymentRules`, `CancelInvoiceRules`, `GetInvoiceByIdRules` — sin cambios respecto a la entrega anterior. Siguen siendo TODOs.

### 3.3 `CancelInvoiceRequestDTO` — sigue siendo `class` sin `cancel_reason`

El análisis anterior indicó convertirlo a `record` y agregar `cancel_reason`. Sin cambios.

### 3.4 Pago parcial (`BalanceDue`) — no implementado aún

`UpdateBalanceAsync` en el repositorio existe y está bien construido, pero `RegistrarPagoAsync` en el servicio todavía marca la factura como pagada al 100% sin usar ese método. Falta conectar la lógica.

---

## 4. Gaps de serialización AOT — `BillingJsonContext`

Los siguientes tipos nuevos no están registrados en `BillingJsonContext.cs`:

```csharp
// AGREGAR:
[JsonSerializable(typeof(PaymentResponseDTO))]
[JsonSerializable(typeof(IEnumerable<PaymentResponseDTO>))]
[JsonSerializable(typeof(GetInvoicePaymentsRequestDTO))]
[JsonSerializable(typeof(GetPatientBalanceRequestDTO))]
[JsonSerializable(typeof(GetDailyClosingRequestDTO))]
[JsonSerializable(typeof(DailyClosingRow))]
[JsonSerializable(typeof(IEnumerable<DailyClosingRow>))]
```

---

## 5. Checklist priorizado para Antigravity

### 🔴 Crítico — el sistema no funciona sin estas correcciones

- [ ] Agregar `[ActionMapping]` a los 7 DTOs de alertas y contactos (§2.4)
- [ ] Corregir firmas de `IClinicalAlertService` e `IPatientContactService` para que reciban DTO como único parámetro (§2.3)
- [ ] Adaptar implementaciones en `ClinicalAlertService` y `PatientContactService` a las firmas corregidas
- [ ] Corregir `PatientAlertEntity.CreatedBy` → `CreatedByUserId` (§2.5)
- [ ] Crear `DeletePatientContactRequestDTO` con `[ActionMapping(4120)]` (§2.4)
- [ ] Reemplazar `GetAllAsync()` en `GetInvoicePaymentsAsync` por `GetByInvoiceIdAsync` (§2.1)
- [ ] Cambiar tipo de retorno de `GetInvoicePaymentsAsync` de `object` a `IEnumerable<PaymentResponseDTO>` (§2.2)
- [ ] Agregar `GetByInvoiceIdAsync` en `IPaymentRepository` e implementar en `PaymentRepository` (§2.1)

### 🟡 Alto — funcionalidad incompleta

- [ ] Inyectar `IEmailService` en `AlertNotificationOutputAction` y enviar email real para CRITICAL/HIGH (§2.8)
- [ ] Limpiar comentario SQL en `InvoiceRepository.SearchAsync` y eliminar JOIN problemático al catálogo (§2.6)
- [ ] Reformatear `GetDailyClosingRequestDTO.cs` (§2.7)
- [ ] Verificar y corregir `StatusId = 1` en `GenerarFacturaAsync` para alinear con `InvoiceStatusEnum.Unpaid = 2` (§3.1)
- [ ] Registrar tipos nuevos en `BillingJsonContext` (§4)

### 🟢 Normal — completar lo pendiente del análisis anterior

- [ ] Implementar `PaymentRules`, `InvoiceItemRules`, `CancelInvoiceRules`, `GetInvoiceByIdRules` (§3.2)
- [ ] Convertir `CancelInvoiceRequestDTO` a `record` y agregar `cancel_reason` (§3.3)
- [ ] Implementar lógica de pago parcial en `RegistrarPagoAsync` usando `UpdateBalanceAsync` del repo (§3.4)

---

## 6. Mapa de archivos a corregir

| Archivo | Corrección | Prioridad |
|---|---|---|
| `src/Application/Features/Clinical/Interfaces/IClinicalAlertService.cs` | Corregir firmas de métodos | 🔴 |
| `src/Application/Features/Clinical/Interfaces/IPatientContactService.cs` | Corregir firmas de métodos | 🔴 |
| `src/Application/Features/Clinical/Services/ClinicalAlertService.cs` | Adaptar a firmas corregidas | 🔴 |
| `src/Application/Features/Clinical/Services/PatientContactService.cs` | Adaptar a firmas corregidas | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/CreatePatientAlertRequestDTO.cs` | Agregar `[ActionMapping]` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/GetPatientAlertsRequestDTO.cs` | Agregar `[ActionMapping]` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/AcknowledgeAlertRequestDTO.cs` | Agregar `[ActionMapping]` + `IHasUser` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/GetPatientContactsRequestDTO.cs` | Agregar `[ActionMapping]` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/CreatePatientContactRequestDTO.cs` | Agregar `[ActionMapping]` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/UpdatePatientContactRequestDTO.cs` | Agregar `[ActionMapping]` | 🔴 |
| `src/Application/Features/Clinical/Dtos/Request/DeletePatientContactRequestDTO.cs` | CREAR con `[ActionMapping(4120)]` | 🔴 |
| `src/Domain/Entities/Clinical/PatientAlertEntity.cs` | `CreatedBy` → `CreatedByUserId` | 🔴 |
| `src/Application/Features/Billing/Interfaces/IBillingService.cs` | `GetInvoicePaymentsAsync` → tipo correcto | 🔴 |
| `src/Application/Features/Billing/Services/BillingService.cs` | Corregir `GetInvoicePaymentsAsync` | 🔴 |
| `src/Domain/Interfaces/Repositories/Billing/IPaymentRepository.cs` | Agregar `GetByInvoiceIdAsync` | 🔴 |
| `src/Infrastructure/Persistence/Repositories/Billing/PaymentRepository.cs` | Implementar `GetByInvoiceIdAsync` | 🔴 |
| `src/Application/Features/Billing/Dtos/Response/PaymentResponseDTO.cs` | CREAR | 🔴 |
| `src/Application/Features/Clinical/OutputActions/AlertNotificationOutputAction.cs` | Inyectar `IEmailService` | 🟡 |
| `src/Infrastructure/Persistence/Repositories/Billing/InvoiceRepository.cs` | Limpiar comentario en `SearchAsync` | 🟡 |
| `src/Application/Features/Billing/Dtos/Request/GetDailyClosingRequestDTO.cs` | Reformatear | 🟡 |
| `src/Application/Features/Billing/Serialization/BillingJsonContext.cs` | Registrar 7 tipos nuevos | 🟡 |
| `src/Application/Features/Billing/Services/BillingService.cs` | `StatusId` en `GenerarFacturaAsync` | 🟡 |

---

*Fin del análisis — MedfarLabs Core · Phase 1 Refactorización*
