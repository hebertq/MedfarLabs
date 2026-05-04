# Instrucciones para AntiGravity: Generación Completa de MedfarLabs.Core

> **Contexto:** Sistema SaaS de Expediente Clínico Electrónico (ECE) multitenancy.
> **Stack:** .NET 9 Native AOT, AWS Lambda, PostgreSQL, Dapper.AOT, System.Text.Json Source Generators, QuestPDF, SQS, S3, SES.
> **Arquitectura:** Monolito con Dispatcher de Acciones, API de Acciones, API de Reportería, PWA como capa de presentación.

---

## 1. FILOSOFÍA ARQUITECTÓNICA (Solution Architect Mindset)

Antes de escribir una línea de código, adopta estos principios:

1. **Dominio Sagrado:** AWS, DB, APIs externas son detalles de implementación. La lógica clínica nunca se contamina con infraestructura.
2. **Todo Falla:** Diseña para recuperación automática. Retry, Circuit Breaker, transacciones atómicas.
3. **Cold Start = Enemigo:** Cada milisegundo cuenta. Minimiza reflexión, usa Source Generation, FrozenDictionary, ValueTask.
4. **Privacidad Quirúrgica:** La auditoría de datos médicos debe ser total pero invisible. Cada acción lleva TraceId.
5. **SaaS Multitenant:** 10 o 10,000 organizaciones, misma base de código. OrganizationId y BranchId se inyectan automáticamente.

---

## 2. ESTÁNDARES DE PROGRAMACIÓN (.NET 9 Native AOT)

### 2.1 Prohibiciones AOT
- **NO** usar `dynamic`, `IEnumerable<dynamic>`, tipos anónimos en repositorios.
- **NO** usar `Expression.Compile()` — usar `System.Reflection.MethodInvoker` (.NET 9).
- **NO** usar `Type.MakeGenericType()` repetitivo.
- **NO** usar `Scrutor` (`Scan()`) — usar `Injectio` con atributos `[RegisterScoped]`, `[RegisterSingleton]` (Source Generators).

### 2.2 Serialización JSON
- Usar SIEMPRE `JsonSerializerContext` (Source Generation).
- Usar `DomainReflectionHelper.DispatcherJsonOptions` para operaciones internas del Dispatcher.
- Usar `JsonDocument.Parse().RootElement` para manipulación segura.
- Suprimir `IL3050/IL2026` SOLO en clases fuertemente tipadas preservadas.
- Naming: `JsonNamingPolicy.CamelCase` en todos los contextos.
- Preferir `DateOnly` y `TimeOnly` con handlers registrados.

### 2.3 Patrones de Diseño
- **Result Pattern:** Todo método de servicio retorna `BaseResponse<T>` o `BaseResponse<object>`. Nunca objetos crudos ni excepciones para flujo de control.
- **Thin Services:** Servicios = orquestadores. Lógica pesada en clases `Validator` o `Rules`.
- **Async por Defecto:** Todo I/O (DB, API, S3) es async, sufijo `Async`, propaga `CancellationToken`.
- **Strategy para Output Actions:** Procesos secundarios (auditoría, SQS, email) inyectados como `IEnumerable<IOutputAction>`.
- **DTOs:** Usar `record` con sufijos `RequestDTO` y `ResponseDTO`.

### 2.4 Convenciones de Nombres
- Repositorios: `I{Entity}Repository` / `{Entity}Repository` heredando de `BaseRepository`.
- Servicios: `{Module}Service` (orquestador).
- Validadores: `{Action}Validator` heredando de validador base.
- Dominios: `{Module}Domain` heredando de `BaseDomain`.
- Acciones: `AppAction.{Module}.{ActionName}` (enum int).

### 2.5 Manejo de Errores
- **NO** try-catch silenciosos. Capturar solo si hay acción compensatoria (rollback).
- Usar `BusinessValidationException` para errores de negocio → `DomainResponseMapper` genera 400.
- OutputActions fallidas: capturar, `LogCritical`, continuar (no abortar flujo del usuario).
- Mensajes de error con código de negocio: `Identity.UserNotFound`, `Billing.InvoiceNotFound`.

---

## 3. ESTRUCTURA DE BASE DE DATOS (PostgreSQL)

### 3.1 Esquemas Existentes (NO crear nuevos sin justificación)
```
identity      → Usuarios, organizaciones, personas, sucursales, doctores
clinical      → Pacientes, expedientes, diagnósticos, alergias, antecedentes, signos vitales
care          → Consultas, recetas, citas, diagnósticos de consulta
laboratory    → Órdenes, resultados, plantillas de exámenes, configuración por org
billing       → Facturas, pagos, suscripciones, planes SaaS, ledger
inventory     → Servicios, bodegas, stock, movimientos
pharmacy      → Medicamentos
security      → Módulos, acciones, roles, permisos, auditoría, telemetría, idempotencia
common        → Catálogos, tipos de cambio
system        → Geolocalización, menús de navegación
```

### 3.2 Reglas SQL Obligatorias
1. **Nombres calificados:** Siempre `schema.table`.
2. **Auditoría:** Toda tabla de negocio debe tener: `created_at` (DEFAULT NOW()), `updated_at`, `created_by`, `updated_by`.
3. **Soft Delete:** `is_active BOOLEAN DEFAULT TRUE` en todas las tablas de negocio.
4. **Row Version:** `row_version INT DEFAULT 1` para optimistic locking.
5. **Filtrado Multitenant:** Toda consulta DEBE incluir `WHERE organization_id = @OrganizationId`.
6. **Mapeo de Acciones:** Al crear funcionalidad nueva, insertar en `security.mst_action` con ID del enum `AppAction`.
7. **Enum Mapping:** Usar `fn_get_catalog_id_by_enum(catalog_id, enum_val)` en lugar de IDs hardcodeados.

### 3.3 Índices Requeridos
- Siempre índice en `(organization_id, is_active)` para tablas de negocio.
- Índice GIN para búsqueda full-text (diagnósticos, nombres de pacientes).
- Índice parcial `WHERE is_active = TRUE` para lookups frecuentes.
- Índice en `created_at DESC` para listados recientes.


---

## 4. MODELO DE DOMINIO CLÍNICO (Entidades Core)

### 4.1 Flujo del Paciente
```
identity.mst_person → clinical.mst_patient → clinical.mst_medical_record
                                                    ↓
                    care.mst_consultation ← care.mst_appointment
                            ↓
            ┌───────────────┼───────────────┐
            ↓               ↓               ↓
    care.mst_prescription  clinical.mst_vital_signs  laboratory.mst_lab_order
            ↓                                    ↓
    care.det_prescription_item          laboratory.det_lab_result
```

### 4.2 Entidades a Generar/Completar

#### A. GAPS CRÍTICOS (Prioridad 1)

**1. security.log_patient_access** — Auditoría de acceso a datos de pacientes (HIPAA/LGSPD)
```sql
CREATE TABLE IF NOT EXISTS security.log_patient_access (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    user_id BIGINT NOT NULL REFERENCES identity.mst_user(id),
    access_type_id INT REFERENCES common.mst_catalog_detail(id), -- e.g. VIEW, EDIT, PRINT, EXPORT
    resource_type_id INT REFERENCES common.mst_catalog_detail(id), -- e.g. MEDICAL_RECORD, PRESCRIPTION, LAB_RESULT
    resource_id BIGINT,
    reason VARCHAR(255),
    ip_address VARCHAR(45),
    user_agent TEXT,
    session_id VARCHAR(100),
    trace_id VARCHAR(100),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_patient_access_patient ON security.log_patient_access(patient_id, created_at DESC);
CREATE INDEX idx_patient_access_user ON security.log_patient_access(user_id, created_at DESC);
CREATE INDEX idx_patient_access_org ON security.log_patient_access(organization_id, created_at DESC);
```

**2. clinical.mst_patient_alert** — Alertas clínicas activas en el expediente
```sql
CREATE TABLE IF NOT EXISTS clinical.mst_patient_alert (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    alert_type_id INT NOT NULL REFERENCES common.mst_catalog_detail(id),
    severity_id INT REFERENCES common.mst_catalog_detail(id), -- e.g. LOW, MEDIUM, HIGH, CRITICAL
    message TEXT NOT NULL,
    source_type_id INT REFERENCES common.mst_catalog_detail(id), -- e.g. ALLERGY, LAB_RESULT, MANUAL, PRESCRIPTION
    source_id BIGINT,
    is_acknowledged BOOLEAN DEFAULT FALSE,
    acknowledged_by_user_id BIGINT REFERENCES identity.mst_user(id),
    acknowledged_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT,
    updated_at TIMESTAMPTZ,
    updated_by_user_id BIGINT,
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_patient_alert_patient ON clinical.mst_patient_alert(patient_id, is_active) WHERE is_active = TRUE;
CREATE INDEX idx_patient_alert_severity ON clinical.mst_patient_alert(severity, is_active) WHERE is_active = TRUE;
```

**3. clinical.mst_patient_contact** — Contactos de emergencia/familiares
```sql
CREATE TABLE IF NOT EXISTS clinical.mst_patient_contact (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    contact_type_id INT REFERENCES common.mst_catalog_detail(id), -- EMERGENCY, FAMILY, LEGAL_GUARDIAN
    full_name VARCHAR(200) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    relationship VARCHAR(50),
    is_primary BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT,
    updated_at TIMESTAMPTZ,
    updated_by_user_id BIGINT,
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_patient_contact_patient ON clinical.mst_patient_contact(patient_id, is_active);
```

#### B. GAPS ALTOS (Prioridad 2)

**5. pharmacy.mst_medication → inventory.mst_service link**
```sql
ALTER TABLE pharmacy.mst_medication
    ADD COLUMN IF NOT EXISTS service_id BIGINT REFERENCES inventory.mst_service(id),
    ADD COLUMN IF NOT EXISTS organization_id BIGINT NOT NULL DEFAULT 1;
CREATE INDEX idx_medication_service ON pharmacy.mst_medication(service_id, organization_id);
```

**6. care.mst_appointment_series** — Citas recurrentes
```sql
CREATE TABLE IF NOT EXISTS care.mst_appointment_series (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    doctor_user_id BIGINT NOT NULL REFERENCES identity.mst_user(id),
    branch_id BIGINT NOT NULL REFERENCES identity.mst_branch(id),
    facility_room_id BIGINT REFERENCES identity.mst_facility_room(id),
    recurrence_pattern_id INT REFERENCES common.mst_catalog_detail(id), -- e.g. DAILY, WEEKLY, MONTHLY, CUSTOM
    recurrence_interval INT DEFAULT 1,
    recurrence_days JSONB, -- [1,3,5] para lunes, miércoles, viernes
    start_date DATE NOT NULL,
    end_date DATE,
    max_occurrences INT,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    reason_notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT,
    updated_at TIMESTAMPTZ,
    updated_by_user_id BIGINT,
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_appointment_series_doctor ON care.mst_appointment_series(doctor_user_id, is_active);
CREATE INDEX idx_appointment_series_patient ON care.mst_appointment_series(patient_id, is_active);
```

**7. care.log_prescription_fulfillment** — Seguimiento de recetas
```sql
CREATE TABLE IF NOT EXISTS care.log_prescription_fulfillment (
    id BIGSERIAL PRIMARY KEY,
    prescription_item_id BIGINT NOT NULL REFERENCES care.det_prescription_item(id),
    pharmacy_organization_id BIGINT REFERENCES identity.mst_organization(id),
    quantity_dispensed DECIMAL(10,2),
    unit_price DECIMAL(10,2),
    dispensed_at TIMESTAMPTZ,
    dispensed_by_user_id BIGINT REFERENCES identity.mst_user(id),
    patient_confirmed BOOLEAN DEFAULT FALSE,
    confirmation_date DATE,
    notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_prescription_fulfillment_item ON care.log_prescription_fulfillment(prescription_item_id);
```

**8. identity.mst_branch_config** — Configuración por sucursal
```sql
CREATE TABLE IF NOT EXISTS identity.mst_branch_config (
    id BIGSERIAL PRIMARY KEY,
    branch_id BIGINT UNIQUE NOT NULL REFERENCES identity.mst_branch(id),
    business_hours JSONB NOT NULL DEFAULT '{}',
    timezone VARCHAR(50) DEFAULT 'America/Managua',
    appointment_slot_minutes INT DEFAULT 15,
    max_appointments_per_slot INT DEFAULT 1,
    auto_confirm_appointments BOOLEAN DEFAULT FALSE,
    reminder_hours_before INT DEFAULT 24,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT,
    updated_at TIMESTAMPTZ,
    updated_by_user_id BIGINT,
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
```

**9. billing.mst_insurance_provider + billing.mst_insurance_coverage**
```sql
CREATE TABLE IF NOT EXISTS billing.mst_insurance_provider (
    id BIGSERIAL PRIMARY KEY,
    organization_id BIGINT NOT NULL,
    name VARCHAR(200) NOT NULL,
    tax_id VARCHAR(50),
    contact_phone VARCHAR(20),
    contact_email VARCHAR(100),
    policy_number_prefix VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1
);

CREATE TABLE IF NOT EXISTS billing.mst_insurance_coverage (
    id BIGSERIAL PRIMARY KEY,
    provider_id BIGINT NOT NULL REFERENCES billing.mst_insurance_provider(id),
    service_id BIGINT NOT NULL REFERENCES inventory.mst_service(id),
    coverage_percentage DECIMAL(5,2) DEFAULT 100.00,
    patient_copay DECIMAL(10,2) DEFAULT 0.00,
    coverage_limit DECIMAL(10,2),
    effective_date DATE NOT NULL,
    expiration_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_insurance_coverage_provider ON billing.mst_insurance_coverage(provider_id, is_active);
CREATE INDEX idx_insurance_coverage_service ON billing.mst_insurance_coverage(service_id, is_active);
```

#### C. GAPS MEDIOS (Prioridad 3)

**10. clinical.mst_medical_attachment** — Archivos médicos centralizados
```sql
CREATE TABLE IF NOT EXISTS clinical.mst_medical_attachment (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    file_type_id INT REFERENCES common.mst_catalog_detail(id), -- RADIOGRAPHY, ULTRASOUND, DOCUMENT
    file_name VARCHAR(255) NOT NULL,
    s3_key VARCHAR(500) NOT NULL,
    s3_bucket VARCHAR(100) NOT NULL,
    file_size_bytes BIGINT,
    mime_type VARCHAR(100),
    description TEXT,
    consultation_id BIGINT REFERENCES care.mst_consultation(id),
    uploaded_by_user_id BIGINT REFERENCES identity.mst_user(id),
    uploaded_at TIMESTAMPTZ DEFAULT NOW(),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_medical_attachment_patient ON clinical.mst_medical_attachment(patient_id, file_type_id);
CREATE INDEX idx_medical_attachment_consultation ON clinical.mst_medical_attachment(consultation_id);
```

**11. clinical.mst_vaccination_record** — Esquema de vacunación
```sql
CREATE TABLE IF NOT EXISTS clinical.mst_vaccination_record (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    vaccine_name VARCHAR(200) NOT NULL,
    vaccine_code VARCHAR(50), -- Código WHO o local
    dose_number INT,
    total_doses INT,
    application_date DATE NOT NULL,
    next_dose_date DATE,
    applied_by_user_id BIGINT REFERENCES identity.mst_user(id),
    batch_number VARCHAR(50),
    manufacturer VARCHAR(100),
    notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_vaccination_patient ON clinical.mst_vaccination_record(patient_id, application_date DESC);
```

**12. care.mst_clinical_note_template** — Plantillas de notas
```sql
CREATE TABLE IF NOT EXISTS care.mst_clinical_note_template (
    id BIGSERIAL PRIMARY KEY,
    organization_id BIGINT NOT NULL,
    name VARCHAR(200) NOT NULL,
    note_type_id INT REFERENCES common.mst_catalog_detail(id), -- SOAP, EVOLUTION, DISCHARGE, REFERRAL
    template_content TEXT NOT NULL,
    variables JSONB, -- ["patient_name", "diagnosis", "medications"]
    is_default BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT,
    updated_at TIMESTAMPTZ,
    updated_by_user_id BIGINT,
    row_version INT DEFAULT 1
);
CREATE INDEX idx_note_template_org ON care.mst_clinical_note_template(organization_id, note_type_id, is_active);
```

**13. care.mst_doctor_schedule_exception** — Excepciones de agenda
```sql
CREATE TABLE IF NOT EXISTS care.mst_doctor_schedule_exception (
    id BIGSERIAL PRIMARY KEY,
    doctor_user_id BIGINT NOT NULL REFERENCES identity.mst_user(id),
    exception_date DATE NOT NULL,
    exception_type_id INT REFERENCES common.mst_catalog_detail(id), -- VACATION, SICK_LEAVE, BLOCKED
    start_time TIME,
    end_time TIME,
    reason TEXT,
    is_all_day BOOLEAN DEFAULT TRUE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    organization_id BIGINT NOT NULL
);
CREATE INDEX idx_doctor_exception_date ON care.mst_doctor_schedule_exception(doctor_user_id, exception_date);
```

**14. inventory.mst_service_price** — Precios por organización
```sql
CREATE TABLE IF NOT EXISTS inventory.mst_service_price (
    id BIGSERIAL PRIMARY KEY,
    organization_id BIGINT NOT NULL,
    service_id BIGINT NOT NULL REFERENCES inventory.mst_service(id),
    price DECIMAL(18,2) NOT NULL,
    cost DECIMAL(18,2) DEFAULT 0.00,
    currency_code VARCHAR(10) DEFAULT 'USD',
    effective_date DATE NOT NULL,
    expiration_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    row_version INT DEFAULT 1,
    UNIQUE(organization_id, service_id, effective_date)
);
CREATE INDEX idx_service_price_org ON inventory.mst_service_price(organization_id, service_id, is_active);
```

#### D. ÍNDICES ADICIONALES REQUERIDOS
```sql
-- Búsqueda full-text de pacientes
CREATE INDEX idx_patient_fulltext ON identity.mst_person 
USING gin(to_tsvector('spanish', COALESCE(first_name,'') || ' ' || COALESCE(middle_name,'') || ' ' || COALESCE(last_name,'') || ' ' || COALESCE(second_last_name,'')));

-- Búsqueda por doctor en citas
CREATE INDEX idx_appointment_doctor_date ON care.mst_appointment(doctor_user_id, scheduled_date, status_id);

-- Búsqueda de facturas por paciente y estado
CREATE INDEX idx_invoice_patient_status ON billing.mst_invoice(patient_id, status_id);

-- Búsqueda de pagos por fecha
CREATE INDEX idx_payment_date_range ON billing.mst_payments(payment_date, organization_id);

-- Búsqueda de recetas por paciente
CREATE INDEX idx_prescription_patient_date ON care.mst_prescription(patient_id, created_at DESC);
```


---

## 5. REPOSITORIOS (Capa de Persistencia)

### 5.1 Reglas de Repositorios
- Heredar de `BaseRepository` para conexión y transacciones.
- Usar Dapper.AOT con tipos fuertes (`record`).
- Para JSONB usar `JsonElement` o el objeto mapeado con `JsonbTypeHandler`.
- Registrar en `IUnitOfWork` y `ConfigureServices.cs`.
- Usar `[IgnoreOnUpdate]` en propiedades inmutables (`CreatedAt`, `CreatedByUserId`).
- SIEMPRE filtrar por `organization_id`.

### 5.2 Repositorios a Generar

#### Prioridad 1 (Críticos)
```csharp
// Interfaces
public interface IPatientAccessLogRepository : IBaseRepository { }
public interface IPatientAlertRepository : IBaseRepository { }
public interface IPatientContactRepository : IBaseRepository { }
public interface IElectronicInvoiceRepository : IBaseRepository { }

// Implementaciones (patrón estándar)
public class PatientAccessLogRepository : BaseRepository, IPatientAccessLogRepository
{
    public PatientAccessLogRepository(IDbConnection connection) : base(connection) { }

    public async Task<long> LogAccessAsync(PatientAccessLogEntity entity, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO security.log_patient_access 
            (patient_id, user_id, access_type, resource_type, resource_id, reason, ip_address, user_agent, session_id, trace_id, organization_id)
            VALUES (@PatientId, @UserId, @AccessType, @ResourceType, @ResourceId, @Reason, @IpAddress, @UserAgent, @SessionId, @TraceId, @OrganizationId)
            RETURNING id";
        return await _connection.QuerySingleAsync<long>(sql, entity);
    }

    public async Task<IEnumerable<PatientAccessLogEntity>> GetAccessHistoryAsync(long patientId, long organizationId, int limit = 100, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT * FROM security.log_patient_access 
            WHERE patient_id = @PatientId AND organization_id = @OrganizationId
            ORDER BY created_at DESC LIMIT @Limit";
        return await _connection.QueryListAsync<PatientAccessLogEntity>(sql, new { patientId, organizationId, limit });
    }
}
```

#### Prioridad 2 (Altos)
```csharp
public interface IAppointmentSeriesRepository : IBaseRepository { }
public interface IPrescriptionFulfillmentRepository : IBaseRepository { }
public interface IBranchConfigRepository : IBaseRepository { }
public interface IInsuranceProviderRepository : IBaseRepository { }
public interface IInsuranceCoverageRepository : IBaseRepository { }
```

#### Prioridad 3 (Medios)
```csharp
public interface IMedicalAttachmentRepository : IBaseRepository { }
public interface IVaccinationRecordRepository : IBaseRepository { }
public interface IClinicalNoteTemplateRepository : IBaseRepository { }
public interface IDoctorScheduleExceptionRepository : IBaseRepository { }
public interface IServicePriceRepository : IBaseRepository { }
```

---

## 6. SERVICIOS (Capa de Aplicación)

### 6.1 Reglas de Servicios
- Thin Services: solo orquestan, no contienen lógica de negocio pesada.
- Retornar SIEMPRE `BaseResponse<T>`.
- Usar `ExecuteInTransactionAsync` para atomicidad.
- Inyectar validadores y repositorios por constructor.
- NO inyectar dependencias secundarias directamente (usar `IOutputAction`).

### 6.2 Servicios a Generar/Extender

#### Servicio de Alertas Clínicas (`ClinicalAlertService`)
```csharp
[RegisterScoped]
public class ClinicalAlertService : BaseService
{
    private readonly IPatientAlertRepository _alertRepository;
    private readonly IGlobalSecurityCache _securityCache;

    public async Task<BaseResponse<IEnumerable<PatientAlertResponseDTO>>> GetActiveAlertsAsync(
        long patientId, long organizationId, CancellationToken ct = default)
    {
        var alerts = await _alertRepository.GetActiveByPatientAsync(patientId, organizationId, ct);
        return BaseResponse<IEnumerable<PatientAlertResponseDTO>>.Success(
            alerts.Select(a => new PatientAlertResponseDTO(a)));
    }

    public async Task<BaseResponse<object>> AcknowledgeAlertAsync(
        AcknowledgeAlertRequestDTO request, long userId, CancellationToken ct = default)
    {
        await _alertRepository.AcknowledgeAsync(request.AlertId, userId, ct);
        return BaseResponse<object>.Success(new { message = "Alert acknowledged" });
    }
}
```

#### Servicio de Contactos de Emergencia (`PatientContactService`)
```csharp
[RegisterScoped]
public class PatientContactService : BaseService
{
    public async Task<BaseResponse<IEnumerable<PatientContactResponseDTO>>> GetContactsAsync(
        long patientId, long organizationId, CancellationToken ct = default);

    public async Task<BaseResponse<PatientContactResponseDTO>> CreateContactAsync(
        CreatePatientContactRequestDTO request, long organizationId, long userId, CancellationToken ct = default);

    public async Task<BaseResponse<object>> UpdateContactAsync(
        UpdatePatientContactRequestDTO request, long organizationId, long userId, CancellationToken ct = default);
}
```

#### Servicio de Facturación Electrónica (`ElectronicInvoiceService`)
```csharp
[RegisterScoped]
public class ElectronicInvoiceService : BaseService
{
    public async Task<BaseResponse<ElectronicInvoiceResponseDTO>> GenerateElectronicInvoiceAsync(
        long invoiceId, string countryCode, long organizationId, CancellationToken ct = default);

    public async Task<BaseResponse<ElectronicInvoiceResponseDTO>> GetElectronicInvoiceStatusAsync(
        long electronicInvoiceId, long organizationId, CancellationToken ct = default);

    public async Task<BaseResponse<object>> CancelElectronicInvoiceAsync(
        long electronicInvoiceId, string reason, long organizationId, CancellationToken ct = default);
}
```

#### Servicio de Auditoría de Acceso (`PatientAccessLogService`)
```csharp
[RegisterScoped]
public class PatientAccessLogService : BaseService
{
    public async Task<BaseResponse<object>> LogAccessAsync(
        LogPatientAccessRequestDTO request, long organizationId, long userId, string traceId, CancellationToken ct = default);

    public async Task<BaseResponse<AccessHistoryResponseDTO>> GetAccessHistoryAsync(
        long patientId, long organizationId, int page = 1, int pageSize = 50, CancellationToken ct = default);
}
```

---

## 7. DOMINIOS Y ACCIONES (Dispatcher)

### 7.1 Reglas del Dispatcher
- **Deserialización Directa:** Usar `JsonSerializer.Deserialize(element, ...)` directamente. NO `GetRawText()` para DTOs.
- **Reflexión Nativa:** Usar `MethodInvoker.Create()` en lugar de `MethodInfo.Invoke`.
- **Caché de Handlers:** `ConcurrentDictionary` para registros dinámicos, `FrozenDictionary` para inmutables.
- **Llave de Caché:** `(Type serviceType, string methodName)` como `ValueTuple`.
- **Output Actions Paralelas:** Ejecutar mediante `Task.WhenAll`.
- **Inyección Automática:** `BaseDomain` inyecta `OrganizationId`, `UserId`, `BranchId` en DTOs que implementen `IHasOrganization` / `IHasBranch`.

### 7.2 Acciones del Enum AppAction a Generar

#### Módulo Clinical (4xxx)
```csharp
public enum AppAction
{
    // ... acciones existentes ...

    // Alertas Clínicas
    GetPatientAlerts = 4114,
    AcknowledgeAlert = 4115,
    CreatePatientAlert = 4116,

    // Contactos de Emergencia
    GetPatientContacts = 4117,
    CreatePatientContact = 4118,
    UpdatePatientContact = 4119,
    DeletePatientContact = 4120,

    // Archivos Médicos
    GetMedicalAttachments = 4121,
    UploadMedicalAttachment = 4122,
    DeleteMedicalAttachment = 4123,

    // Vacunación
    GetVaccinationRecords = 4124,
    RegisterVaccination = 4125,

    // Auditoría de Acceso
    GetPatientAccessHistory = 4126,
}
```

#### Módulo Billing (3xxx)
```csharp
public enum AppAction
{
    // ... acciones existentes ...

    // Seguros
    GetInsuranceProviders = 3026,
    CreateInsuranceProvider = 3027,
    UpdateInsuranceCoverage = 3028,

    // Precios por Organización
    GetServicePrices = 3029,
    UpdateServicePrice = 3030,
}
```

#### Módulo Care (5xxx)
```csharp
public enum AppAction
{
    // ... acciones existentes ...

    // Citas Recurrentes
    CreateAppointmentSeries = 5007,
    GetAppointmentSeries = 5008,
    CancelAppointmentSeries = 5009,

    // Seguimiento de Recetas
    RegisterPrescriptionFulfillment = 5010,
    GetPrescriptionFulfillment = 5011,

    // Plantillas de Notas
    GetClinicalNoteTemplates = 5012,
    SaveClinicalNoteTemplate = 5013,

    // Excepciones de Agenda
    CreateScheduleException = 5014,
    GetScheduleExceptions = 5015,
}
```

#### Módulo Identity (2xxx)
```csharp
public enum AppAction
{
    // ... acciones existentes ...

    // Configuración por Sucursal
    GetBranchConfig = 2111,
    UpdateBranchConfig = 2112,
}
```

### 7.3 Dominios a Generar

```csharp
// ClinicalDomain.cs - Extender existente
public partial class ClinicalDomain : BaseDomain
{
    [AppAction(AppAction.GetPatientAlerts)]
    public async Task<BaseResponse<IEnumerable<PatientAlertResponseDTO>>> GetPatientAlertsAsync(
        GetPatientAlertsRequestDTO request, CancellationToken ct = default)
    {
        return await _clinicalAlertService.GetActiveAlertsAsync(request.PatientId, request.OrganizationId, ct);
    }

    [AppAction(AppAction.AcknowledgeAlert)]
    public async Task<BaseResponse<object>> AcknowledgeAlertAsync(
        AcknowledgeAlertRequestDTO request, CancellationToken ct = default)
    {
        return await _clinicalAlertService.AcknowledgeAlertAsync(request, _userContext.UserId, ct);
    }

    [AppAction(AppAction.GetPatientContacts)]
    public async Task<BaseResponse<IEnumerable<PatientContactResponseDTO>>> GetPatientContactsAsync(
        GetPatientContactsRequestDTO request, CancellationToken ct = default)
    {
        return await _patientContactService.GetContactsAsync(request.PatientId, request.OrganizationId, ct);
    }

    [AppAction(AppAction.GetMedicalAttachments)]
    public async Task<BaseResponse<IEnumerable<MedicalAttachmentResponseDTO>>> GetMedicalAttachmentsAsync(
        GetMedicalAttachmentsRequestDTO request, CancellationToken ct = default)
    {
        return await _medicalAttachmentService.GetAttachmentsAsync(request.PatientId, request.OrganizationId, ct);
    }

    [AppAction(AppAction.GetVaccinationRecords)]
    public async Task<BaseResponse<IEnumerable<VaccinationRecordResponseDTO>>> GetVaccinationRecordsAsync(
        GetVaccinationRecordsRequestDTO request, CancellationToken ct = default)
    {
        return await _vaccinationService.GetRecordsAsync(request.PatientId, request.OrganizationId, ct);
    }

    [AppAction(AppAction.GetPatientAccessHistory)]
    public async Task<BaseResponse<AccessHistoryResponseDTO>> GetPatientAccessHistoryAsync(
        GetAccessHistoryRequestDTO request, CancellationToken ct = default)
    {
        return await _accessLogService.GetAccessHistoryAsync(request.PatientId, request.OrganizationId, request.Page, request.PageSize, ct);
    }
}

// BillingDomain.cs - Extender existente
public partial class BillingDomain : BaseDomain
{
    [AppAction(AppAction.GetInsuranceProviders)]
    public async Task<BaseResponse<IEnumerable<InsuranceProviderResponseDTO>>> GetInsuranceProvidersAsync(
        GetInsuranceProvidersRequestDTO request, CancellationToken ct = default)
    {
        return await _insuranceService.GetProvidersAsync(request.OrganizationId, ct);
    }

    [AppAction(AppAction.GetServicePrices)]
    public async Task<BaseResponse<IEnumerable<ServicePriceResponseDTO>>> GetServicePricesAsync(
        GetServicePricesRequestDTO request, CancellationToken ct = default)
    {
        return await _servicePriceService.GetPricesAsync(request.OrganizationId, request.ServiceId, ct);
    }
}

// CareDomain.cs - Extender existente
public partial class CareDomain : BaseDomain
{
    [AppAction(AppAction.CreateAppointmentSeries)]
    public async Task<BaseResponse<AppointmentSeriesResponseDTO>> CreateAppointmentSeriesAsync(
        CreateAppointmentSeriesRequestDTO request, CancellationToken ct = default)
    {
        return await _appointmentService.CreateSeriesAsync(request, request.OrganizationId, _userContext.UserId, ct);
    }

    [AppAction(AppAction.RegisterPrescriptionFulfillment)]
    public async Task<BaseResponse<object>> RegisterPrescriptionFulfillmentAsync(
        RegisterFulfillmentRequestDTO request, CancellationToken ct = default)
    {
        return await _prescriptionService.RegisterFulfillmentAsync(request, request.OrganizationId, _userContext.UserId, ct);
    }

    [AppAction(AppAction.GetClinicalNoteTemplates)]
    public async Task<BaseResponse<IEnumerable<ClinicalNoteTemplateResponseDTO>>> GetClinicalNoteTemplatesAsync(
        GetNoteTemplatesRequestDTO request, CancellationToken ct = default)
    {
        return await _noteTemplateService.GetTemplatesAsync(request.OrganizationId, request.NoteTypeId, ct);
    }

    [AppAction(AppAction.CreateScheduleException)]
    public async Task<BaseResponse<object>> CreateScheduleExceptionAsync(
        CreateScheduleExceptionRequestDTO request, CancellationToken ct = default)
    {
        return await _scheduleService.CreateExceptionAsync(request, request.OrganizationId, _userContext.UserId, ct);
    }
}

// IdentityDomain.cs - Extender existente
public partial class IdentityDomain : BaseDomain
{
    [AppAction(AppAction.GetBranchConfig)]
    public async Task<BaseResponse<BranchConfigResponseDTO>> GetBranchConfigAsync(
        GetBranchConfigRequestDTO request, CancellationToken ct = default)
    {
        return await _branchConfigService.GetConfigAsync(request.BranchId, request.OrganizationId, ct);
    }

    [AppAction(AppAction.UpdateBranchConfig)]
    public async Task<BaseResponse<BranchConfigResponseDTO>> UpdateBranchConfigAsync(
        UpdateBranchConfigRequestDTO request, CancellationToken ct = default)
    {
        return await _branchConfigService.UpdateConfigAsync(request, request.OrganizationId, _userContext.UserId, ct);
    }
}
```


---

## 8. VALIDADORES (Business Rules)

### 8.1 Reglas de Validación
- Caché de validadores: diccionario estático `_validatorTypeCache`, NO `MakeGenericType` por petición.
- `BaseDomain` inyecta identidad automáticamente en DTOs con `IHasOrganization`.
- Usar `ExecuteInTransactionAsync` para atomicidad (Commit + eventos).
- Lanzar `BusinessValidationException` para respuestas 400 consistentes.

### 8.2 Validadores a Generar

```csharp
public class CreatePatientAlertValidator : BaseValidator<CreatePatientAlertRequestDTO>
{
    public override void Validate(CreatePatientAlertRequestDTO dto)
    {
        FailIf(dto.PatientId <= 0, "PatientId.Required", "El paciente es obligatorio");
        FailIf(string.IsNullOrWhiteSpace(dto.Message), "Message.Required", "El mensaje de alerta es obligatorio");
        FailIf(dto.Severity != "LOW" && dto.Severity != "MEDIUM" && dto.Severity != "HIGH" && dto.Severity != "CRITICAL",
            "Severity.Invalid", "Severidad inválida");
        FailIf(dto.AlertTypeId <= 0, "AlertTypeId.Required", "El tipo de alerta es obligatorio");
    }
}

public class CreatePatientContactValidator : BaseValidator<CreatePatientContactRequestDTO>
{
    public override void Validate(CreatePatientContactRequestDTO dto)
    {
        FailIf(dto.PatientId <= 0, "PatientId.Required", "El paciente es obligatorio");
        FailIf(string.IsNullOrWhiteSpace(dto.FullName), "FullName.Required", "El nombre del contacto es obligatorio");
        FailIf(string.IsNullOrWhiteSpace(dto.Phone) && string.IsNullOrWhiteSpace(dto.Email),
            "Contact.Required", "Debe proporcionar al menos un teléfono o email");
    }
}

public class CreateAppointmentSeriesValidator : BaseValidator<CreateAppointmentSeriesRequestDTO>
{
    public override void Validate(CreateAppointmentSeriesRequestDTO dto)
    {
        FailIf(dto.PatientId <= 0, "PatientId.Required", "El paciente es obligatorio");
        FailIf(dto.DoctorUserId <= 0, "DoctorUserId.Required", "El médico es obligatorio");
        FailIf(dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow), "StartDate.Invalid", "La fecha de inicio no puede ser en el pasado");
        FailIf(dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate, "EndDate.Invalid", "La fecha fin no puede ser anterior a la fecha inicio");
        FailIf(dto.RecurrenceInterval <= 0, "RecurrenceInterval.Invalid", "El intervalo de recurrencia debe ser mayor a 0");
    }
}
```

---

## 9. OUTPUT ACTIONS (Eventos Asíncronos)

### 9.1 Reglas de Output Actions
- Todo proceso NO estrictamente necesario para la respuesta HTTP debe ser `IOutputAction`.
- Ejecutar en paralelo con `Task.WhenAll`.
- Si falla: capturar, `LogCritical`, continuar (no abortar).
- Respetar `EXECUTION_CONTEXT == "Main"` para evitar loops en workers.
- Inyectar `TraceId` en mensajes SQS.

### 9.2 Output Actions a Generar

```csharp
// AuditOutputAction.cs — Registra auditoría de acceso a pacientes
public class PatientAccessAuditOutputAction : IOutputAction
{
    private readonly IPatientAccessLogRepository _accessLogRepository;
    private readonly ILogger<PatientAccessAuditOutputAction> _logger;

    public async Task ExecuteAsync(OutputContextDto context)
    {
        if (context.ActionId is not (AppAction.GetPatientRecord or AppAction.GetPatientDirectory or AppAction.GetConsultationDetails))
            return;

        try
        {
            var logEntry = new PatientAccessLogEntity
            {
                PatientId = context.GetPatientIdFromPayload(),
                UserId = context.UserId,
                AccessType = "VIEW",
                ResourceType = context.ActionId == AppAction.GetPatientRecord ? "MEDICAL_RECORD" : "PATIENT_DIRECTORY",
                TraceId = context.TraceId,
                OrganizationId = context.OrganizationId,
                IpAddress = context.IpAddress,
                SessionId = context.SessionId
            };

            await _accessLogRepository.LogAccessAsync(logEntry);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to log patient access. TraceId: {TraceId}", context.TraceId);
        }
    }
}

// AlertNotificationOutputAction.cs — Notifica alertas clínicas críticas
public class AlertNotificationOutputAction : IOutputAction
{
    private readonly IEmailService _emailService;
    private readonly ILogger<AlertNotificationOutputAction> _logger;

    public async Task ExecuteAsync(OutputContextDto context)
    {
        if (context.ActionId != AppAction.CreatePatientAlert)
            return;

        var alertSeverity = context.GetPayloadValue<string>("severity");
        if (alertSeverity != "CRITICAL" && alertSeverity != "HIGH")
            return;

        try
        {
            var patientId = context.GetPayloadValue<long>("patientId");
            var message = context.GetPayloadValue<string>("message");

            await _emailService.SendAsync(new EmailRequest
            {
                To = context.GetDoctorEmail(),
                Subject = $"Alerta Clínica Crítica - Paciente #{patientId}",
                Body = $"Se ha registrado una alerta crítica: {message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to send alert notification. TraceId: {TraceId}", context.TraceId);
        }
    }
}

```

---

## 10. DTOs (Data Transfer Objects)

### 10.1 Reglas de DTOs
- Usar `record` para inmutabilidad.
- Sufijos obligatorios: `RequestDTO` y `ResponseDTO`.
- Implementar `IHasOrganization` / `IHasBranch` cuando aplique para inyección automática.
- Usar `DateOnly` y `TimeOnly` para fechas sin hora/horas sin fecha.

### 10.2 DTOs a Generar

```csharp
// === ALERTAS CLÍNICAS ===
public record GetPatientAlertsRequestDTO(long PatientId) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record PatientAlertResponseDTO(
    long Id,
    long PatientId,
    int AlertTypeId,
    string Severity,
    string Message,
    int SourceTypeId,
    bool IsAcknowledged,
    DateTime? AcknowledgedAt,
    DateTime CreatedAt
);

public record AcknowledgeAlertRequestDTO(long AlertId) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record CreatePatientAlertRequestDTO(
    long PatientId,
    int AlertTypeId,
    string Severity,
    string Message,
    int SourceTypeId,
    long? SourceId
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

// === CONTACTOS DE EMERGENCIA ===
public record GetPatientContactsRequestDTO(long PatientId) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record PatientContactResponseDTO(
    long Id,
    long PatientId,
    int? ContactTypeId,
    string FullName,
    string Phone,
    string Email,
    string Relationship,
    bool IsPrimary
);

public record CreatePatientContactRequestDTO(
    long PatientId,
    int? ContactTypeId,
    string FullName,
    string Phone,
    string Email,
    string Relationship,
    bool IsPrimary
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record UpdatePatientContactRequestDTO(
    long Id,
    int? ContactTypeId,
    string FullName,
    string Phone,
    string Email,
    string Relationship,
    bool IsPrimary
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

// === CITAS RECURRENTES ===
public record CreateAppointmentSeriesRequestDTO(
    long PatientId,
    long DoctorUserId,
    long BranchId,
    long? FacilityRoomId,
    int RecurrencePatternId,
    int RecurrenceInterval,
    int[]? RecurrenceDays,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? MaxOccurrences,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? ReasonNotes
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record AppointmentSeriesResponseDTO(
    long Id,
    long PatientId,
    long DoctorUserId,
    int RecurrencePatternId,
    DateOnly StartDate,
    DateOnly? EndDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GeneratedAppointmentsCount
);

// === SEGUIMIENTO DE RECETAS ===
public record RegisterFulfillmentRequestDTO(
    long PrescriptionItemId,
    long? PharmacyOrganizationId,
    decimal QuantityDispensed,
    decimal? UnitPrice,
    DateTime? DispensedAt,
    bool PatientConfirmed,
    DateOnly? ConfirmationDate,
    string? Notes
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

// === PLANTILLAS DE NOTAS CLÍNICAS ===
public record GetNoteTemplatesRequestDTO(
    int? NoteTypeId
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record ClinicalNoteTemplateResponseDTO(
    long Id,
    string Name,
    int NoteTypeId,
    string TemplateContent,
    string[] Variables,
    bool IsDefault
);

// === EXCEPCIONES DE AGENDA ===
public record CreateScheduleExceptionRequestDTO(
    long DoctorUserId,
    DateOnly ExceptionDate,
    int ExceptionTypeId,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason,
    bool IsAllDay
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

// === CONFIGURACIÓN POR SUCURSAL ===
public record GetBranchConfigRequestDTO(long BranchId) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record BranchConfigResponseDTO(
    long Id,
    long BranchId,
    Dictionary<string, BusinessHoursDTO> BusinessHours,
    string Timezone,
    int AppointmentSlotMinutes,
    int MaxAppointmentsPerSlot,
    bool AutoConfirmAppointments,
    int ReminderHoursBefore
);

public record BusinessHoursDTO(string Open, string Close);

public record UpdateBranchConfigRequestDTO(
    long BranchId,
    Dictionary<string, BusinessHoursDTO> BusinessHours,
    string Timezone,
    int AppointmentSlotMinutes,
    int MaxAppointmentsPerSlot,
    bool AutoConfirmAppointments,
    int ReminderHoursBefore
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

// === AUDITORÍA DE ACCESO ===
public record GetAccessHistoryRequestDTO(
    long PatientId,
    int Page = 1,
    int PageSize = 50
) : IHasOrganization
{
    public long OrganizationId { get; set; }
}

public record AccessHistoryResponseDTO(
    IEnumerable<AccessLogEntryDTO> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record AccessLogEntryDTO(
    long Id,
    long PatientId,
    long UserId,
    int AccessTypeId,
    int ResourceTypeId,
    long? ResourceId,
    string? Reason,
    DateTime AccessedAt,
    string? IpAddress
);

// === SEGUROS ===
public record InsuranceProviderResponseDTO(
    long Id,
    string Name,
    string? TaxId,
    string? ContactPhone,
    string? ContactEmail,
    string? PolicyNumberPrefix
);

public record ServicePriceResponseDTO(
    long Id,
    long ServiceId,
    decimal Price,
    decimal Cost,
    string CurrencyCode,
    DateOnly EffectiveDate,
    DateOnly? ExpirationDate
);
```

---

## 11. SERIALIZACIÓN JSON (Source Generation)

### 11.1 Reglas de Serialización
- Usar SIEMPRE `JsonSerializerContext` para evitar reflexión.
- Usar `ToFrozenDictionary()` para registros inmutables.
- CamelCase en todos los contextos.
- Crear contextos por feature.

### 11.2 Contextos a Generar/Extender

```csharp
// ClinicalJsonContext.cs
[JsonSerializable(typeof(GetPatientAlertsRequestDTO))]
[JsonSerializable(typeof(PatientAlertResponseDTO))]
[JsonSerializable(typeof(AcknowledgeAlertRequestDTO))]
[JsonSerializable(typeof(CreatePatientAlertRequestDTO))]
[JsonSerializable(typeof(GetPatientContactsRequestDTO))]
[JsonSerializable(typeof(PatientContactResponseDTO))]
[JsonSerializable(typeof(CreatePatientContactRequestDTO))]
[JsonSerializable(typeof(UpdatePatientContactRequestDTO))]
[JsonSerializable(typeof(GetMedicalAttachmentsRequestDTO))]
[JsonSerializable(typeof(MedicalAttachmentResponseDTO))]
[JsonSerializable(typeof(GetVaccinationRecordsRequestDTO))]
[JsonSerializable(typeof(VaccinationRecordResponseDTO))]
[JsonSerializable(typeof(GetAccessHistoryRequestDTO))]
[JsonSerializable(typeof(AccessHistoryResponseDTO))]
[JsonSerializable(typeof(AccessLogEntryDTO))]
public partial class ClinicalJsonContext : JsonSerializerContext { }

// BillingJsonContext.cs
[JsonSerializable(typeof(GetInsuranceProvidersRequestDTO))]
[JsonSerializable(typeof(InsuranceProviderResponseDTO))]
[JsonSerializable(typeof(GetServicePricesRequestDTO))]
[JsonSerializable(typeof(ServicePriceResponseDTO))]
public partial class BillingJsonContext : JsonSerializerContext { }

// CareJsonContext.cs
[JsonSerializable(typeof(CreateAppointmentSeriesRequestDTO))]
[JsonSerializable(typeof(AppointmentSeriesResponseDTO))]
[JsonSerializable(typeof(RegisterFulfillmentRequestDTO))]
[JsonSerializable(typeof(GetNoteTemplatesRequestDTO))]
[JsonSerializable(typeof(ClinicalNoteTemplateResponseDTO))]
[JsonSerializable(typeof(CreateScheduleExceptionRequestDTO))]
public partial class CareJsonContext : JsonSerializerContext { }

// IdentityJsonContext.cs
[JsonSerializable(typeof(GetBranchConfigRequestDTO))]
[JsonSerializable(typeof(BranchConfigResponseDTO))]
[JsonSerializable(typeof(BusinessHoursDTO))]
[JsonSerializable(typeof(UpdateBranchConfigRequestDTO))]
public partial class IdentityJsonContext : JsonSerializerContext { }
```

---

## 12. OBSERVABILIDAD Y TRAZABILIDAD

### 12.1 Reglas de Observabilidad
- **Structured Logging:** NO usar interpolación (`$"Error: {ex.Message}"`). Usar Message Templates.
- **TraceId:** Propagar en TODOS los flujos (API -> Dispatcher -> SQS Worker).
- **Logs Warning+:** Deben incluir implícita o explícitamente el `TraceId`.
- **Recursividad:** Límite estricto `MAX_RECURSION_DEPTH = 3` en background jobs.
- **Separación:** Telemetría (técnica) vs Auditoría (negocio). Asociar `ParentAuditId`.

### 12.2 Métricas a Registrar

```csharp
// En TelemetryOutputAction
public class TelemetryMetrics
{
    public const string ActionDuration = "medfar.action.duration_ms";
    public const string ActionSuccess = "medfar.action.success";
    public const string DbQueryDuration = "medfar.db.query.duration_ms";
    public const string ColdStartDuration = "medfar.lambda.cold_start_ms";
    public const string PatientAccessCount = "medfar.patient.access_count";
    public const string AlertTriggeredCount = "medfar.alert.triggered_count";
}
```

### 12.3 Alertas Operacionales
- Latencia de API > 3 segundos -> `LogCritical`.
- Error rate > 1% -> Alerta.
- DB connections > 80% -> Alerta.
- Cold start > 500ms -> `LogWarning`.
- Acceso a paciente sin autorización -> `LogCritical` + notificación seguridad.

---

## 13. SEGURIDAD Y MULTITENANCY

### 13.1 Reglas de Seguridad
- **Inyección Automática:** `BaseDomain` inyecta `OrganizationId`, `UserId`, `BranchId`.
- **Filtrado SQL:** Toda query DEBE tener `WHERE organization_id = @OrganizationId`.
- **Validación de Cruce:** Antes de update, verificar que el registro pertenece a la org del usuario.
- **Claims Mínimos:** JWT debe tener `nameid/sub`, `organization_id`, `branch_id`.
- **Caché de Permisos:** `GlobalSecurityCache` (Singleton) con `HashSet<int>` por rol. Búsqueda O(1).
- **Validación Multi-Rol:** Acceso si CUALQUIERA de los roles del usuario tiene la acción.

### 13.2 Acciones de Seguridad a Registrar

Al crear las nuevas acciones (AppAction), insertar en `security.mst_action`:

```sql
-- Alertas Clínicas
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4114, 4, 'GetPatientAlerts'),
(4115, 4, 'AcknowledgeAlert'),
(4116, 4, 'CreatePatientAlert')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Contactos
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4117, 4, 'GetPatientContacts'),
(4118, 4, 'CreatePatientContact'),
(4119, 4, 'UpdatePatientContact'),
(4120, 4, 'DeletePatientContact')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Archivos Médicos
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4121, 4, 'GetMedicalAttachments'),
(4122, 4, 'UploadMedicalAttachment'),
(4123, 4, 'DeleteMedicalAttachment')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Vacunación
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4124, 4, 'GetVaccinationRecords'),
(4125, 4, 'RegisterVaccination')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Auditoría
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4126, 4, 'GetPatientAccessHistory')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Seguros
INSERT INTO security.mst_action (id, module_id, name) VALUES
(3026, 3, 'GetInsuranceProviders'),
(3027, 3, 'CreateInsuranceProvider'),
(3028, 3, 'UpdateInsuranceCoverage')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Precios
INSERT INTO security.mst_action (id, module_id, name) VALUES
(3029, 3, 'GetServicePrices'),
(3030, 3, 'UpdateServicePrice')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Citas Recurrentes
INSERT INTO security.mst_action (id, module_id, name) VALUES
(5007, 5, 'CreateAppointmentSeries'),
(5008, 5, 'GetAppointmentSeries'),
(5009, 5, 'CancelAppointmentSeries')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Recetas
INSERT INTO security.mst_action (id, module_id, name) VALUES
(5010, 5, 'RegisterPrescriptionFulfillment'),
(5011, 5, 'GetPrescriptionFulfillment')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Plantillas
INSERT INTO security.mst_action (id, module_id, name) VALUES
(5012, 5, 'GetClinicalNoteTemplates'),
(5013, 5, 'SaveClinicalNoteTemplate')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Excepciones
INSERT INTO security.mst_action (id, module_id, name) VALUES
(5014, 5, 'CreateScheduleException'),
(5015, 5, 'GetScheduleExceptions')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Config Sucursal
INSERT INTO security.mst_action (id, module_id, name) VALUES
(2111, 2, 'GetBranchConfig'),
(2112, 2, 'UpdateBranchConfig')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Asignar a roles Admin correspondientes
INSERT INTO security.map_role_action (role_id, action_id)
SELECT r.id, a.id
FROM security.mst_role r
CROSS JOIN security.mst_action a
WHERE r.name IN ('Admin-Clinical', 'Admin-Billing', 'Admin-Care', 'Admin-Identity')
  AND a.id IN (4114,4115,4116,4117,4118,4119,4120,4121,4122,4123,4124,4125,4126,
               3026,3027,3028,3029,3030,
               5007,5008,5009,5010,5011,5012,5013,5014,5015,
               2111,2112)
ON CONFLICT DO NOTHING;
```

---

## 14. CLOUD INTEGRATION Y RESILIENCIA

### 14.1 Reglas de Resiliencia
- **NO** llamadas HTTP crudas. Usar `HttpClient` con Polly (Retry Jitter + Circuit Breaker).
- **Streams en S3:** Usar `Stream` para archivos grandes (radiografías/PDFs). Nunca `byte[]` completo.
- **Email:** Fallo silencioso. `LogWarning` y continuar. No romper transacción principal.
- **TraceId:** Inyectar en TODOS los headers HTTP externos.

### 14.2 Servicios Cloud a Integrar

```csharp
// S3Service para archivos médicos
public interface IMedicalAttachmentStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, long organizationId);
    Task<Stream> DownloadAsync(string s3Key, long organizationId);
    Task DeleteAsync(string s3Key, long organizationId);
}
```

---

## 15. REPORTES (QuestPDF)

### 15.1 Reglas de Reportes
- Heredar de `BaseReportTemplate<TModel>`.
- Implementar obligatoriamente: `ComposeHeader`, `ComposeContent`, `ComposeFooter`.
- Soporte dual: `ReportFormat.A4` y `ReportFormat.Ticket` (80mm).
- **NO** hardcoding de estilos. Usar `ReportStyleExtensions`.
- Generar en memoria (Stream), no en disco de Lambda.
- Retornar `ReportResponseDTO` con Base64, nombre y ContentType.

### 15.2 Reportes a Generar

```csharp
// Reporte de Auditoría de Acceso
public class PatientAccessAuditReportTemplate : BaseReportTemplate<PatientAccessAuditReportModel>
{
    public override void ComposeHeader(IContainer container) { /* Logo org, título, fecha */ }
    public override void ComposeContent(IContainer container) { /* Tabla de accesos */ }
    public override void ComposeFooter(IContainer container) { /* Paginación, firma */ }
}

// Reporte de Alertas Clínicas Activas
public class ActiveAlertsReportTemplate : BaseReportTemplate<ActiveAlertsReportModel>
{
    public override void ComposeContent(IContainer container) { /* Alertas por severidad */ }
}

// Reporte de Cuentas por Cobrar (Aging)
public class AccountsReceivableReportTemplate : BaseReportTemplate<AccountsReceivableReportModel>
{
    public override void ComposeContent(IContainer container) { /* Tabla 0-30, 31-60, 61-90, 90+ */ }
}
```

---

## 16. CHECKLIST DE ENTREGA

Para cada feature generada, verificar:

- [ ] **SQL:** Script de migración idempotente (`IF NOT EXISTS`, `ON CONFLICT`)
- [ ] **SQL:** Índices apropiados creados
- [ ] **SQL:** Columnas de auditoría (`created_at`, `updated_at`, `created_by`, `updated_by`)
- [ ] **SQL:** `is_active` y `row_version` presentes
- [ ] **SQL:** `organization_id` en todas las tablas de negocio
- [ ] **Repo:** Interfaz en `Domain/Interfaces/Repositories`
- [ ] **Repo:** Implementación en `Infrastructure/Persistence/Repositories`
- [ ] **Repo:** Registrado en `IUnitOfWork` y `ConfigureServices`
- [ ] **Repo:** Usa Dapper.AOT con tipos fuertes (record)
- [ ] **Repo:** Filtra por `organization_id`
- [ ] **Service:** Interfaz y implementación en `Application/Features/{Module}/Services`
- [ ] **Service:** Hereda de `BaseService`
- [ ] **Service:** Retorna `BaseResponse<T>`
- [ ] **Service:** Usa `ExecuteInTransactionAsync` cuando modifica datos
- [ ] **Validator:** Clase en `Application/Features/{Module}/Validators`
- [ ] **Validator:** Hereda de `BaseValidator<T>`
- [ ] **Validator:** Lanza `BusinessValidationException`
- [ ] **Domain:** Método con atributo `[AppAction]`
- [ ] **Domain:** Inyecta automáticamente `OrganizationId`/`BranchId`
- [ ] **DTOs:** Records con sufijos `RequestDTO`/`ResponseDTO`
- [ ] **DTOs:** Implementa `IHasOrganization` cuando aplique
- [ ] **DTOs:** Registrados en `JsonSerializerContext` correspondiente
- [ ] **OutputAction:** Implementa `IOutputAction` si es proceso secundario
- [ ] **OutputAction:** Maneja fallos con `LogCritical` y continúa
- [ ] **Security:** Acción registrada en `security.mst_action`
- [ ] **Security:** Acción asignada a roles correspondientes
- [ ] **Observabilidad:** Logs estructurados con Message Templates
- [ ] **Observabilidad:** TraceId propagado en todos los flujos
- [ ] **Tests:** Faker en `tests/SharedFakers`
- [ ] **Tests:** Prueba de integración heredando de `BaseIntegrationTest`
- [ ] **Tests:** Verifica estado de DB después de la acción

---

## 17. ORDEN DE IMPLEMENTACIÓN RECOMENDADO

### Fase 1: Fundamentos (Semana 1)
1. `security.log_patient_access` + Repositorio + Servicio + Acción
2. `clinical.mst_patient_alert` + Repositorio + Servicio + Acción
3. `clinical.mst_patient_contact` + Repositorio + Servicio + Acción

### Fase 2: Operación Diaria (Semana 2)
5. `pharmacy.mst_medication` -> `inventory.mst_service` link
6. `care.mst_appointment_series` + Repositorio + Servicio + Acción
7. `care.log_prescription_fulfillment` + Repositorio + Servicio + Acción
8. `identity.mst_branch_config` + Repositorio + Servicio + Acción

### Fase 3: Billing Avanzado (Semana 3)
9. `billing.mst_insurance_provider` + `billing.mst_insurance_coverage`
10. `inventory.mst_service_price` + Repositorio + Servicio + Acción
11. Reportes de QuestPDF para facturación

### Fase 4: Completitud (Semana 4)
12. `clinical.mst_medical_attachment` + S3 integration
13. `clinical.mst_vaccination_record`
14. `care.mst_clinical_note_template`
15. `care.mst_doctor_schedule_exception`

---

## 18. NOTAS FINALES PARA ANTIGRAVITY

1. **Todo código debe ser compatible con Native AOT.** Verificar con `dotnet publish -r linux-x64 -p:PublishAot=true`.
2. **No usar librerías con reflexión en runtime.** Preferir Source Generators.
3. **Toda tabla nueva debe tener índice en `organization_id`.**
4. **Toda acción nueva debe registrarse en `security.mst_action` y asignarse a roles.**
5. **Todo servicio debe retornar `BaseResponse<T>`.**
6. **Todo DTO debe usar `record` y sufijos `RequestDTO`/`ResponseDTO`.**
7. **Todo repositorio debe heredar de `BaseRepository` y usar Dapper.AOT.**
8. **Toda validación debe usar `BusinessValidationException`.**
9. **Todo proceso asíncrono secundario debe ser `IOutputAction`.**
10. **Todo log debe usar Message Templates (NO interpolación).**

**Generar todo el código siguiendo estrictamente estas instrucciones y los skills existentes de MedfarLabs.Core.**
