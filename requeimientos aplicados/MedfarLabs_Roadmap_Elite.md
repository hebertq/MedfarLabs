# MedfarLabs · Roadmap hacia un Sistema de Elite
> Audit completo del estado actual — Gaps, tests y fakers pendientes  
> Mayo 2025

---

## 0. Estado actual en números

| Módulo | Features | Tests | Fakers | % Listo |
|---|---|---|---|---|
| Identity | 8/8 | 4 tests | ✅ completo | 90% |
| Clinical (paciente, expediente, signos) | 10/10 | 3 tests | ✅ parcial | 80% |
| **Alertas clínicas** | 3/3 | **0 tests** | **❌ ninguno** | 40% |
| **Contactos de paciente** | 4/4 | **0 tests** | **❌ ninguno** | 40% |
| **Auditoría de acceso** | 3/3 | **0 tests** | **❌ ninguno** | 40% |
| Care (consulta, receta, citas) | 7/7 | 4 tests | ✅ parcial | 75% |
| **Dashboard clínico** | 0/1 | **0 tests** | **❌ ninguno** | 0% |
| **Alergias** | 0/1 | **0 tests** | **❌ ninguno** | 0% |
| Laboratory (órdenes, muestras, resultados) | 6/9 | 1 test | ✅ parcial | 55% |
| **Lab Templates** | 0/8 | **0 tests** | **❌ ninguno** | 20% |
| Billing (facturas paciente) | 8/8 | 4 tests | ✅ parcial | 70% |
| **Pago parcial / cierre de caja** | 1/3 | **0 tests** | **❌ ninguno** | 30% |
| Subscriptions SaaS | 7/7 | 2 tests | ✅ | 80% |
| Reporting (PDF) | 3/3 | 2 tests* | — | 60% |
| Observabilidad / Telemetría | parcial | **0 tests** | — | 50% |
| **Notificaciones Email** | 0/1 | **0 tests** | — | 20% |

*Los 2 tests de reporting guardan a una ruta hardcodeada de Windows (`C:\Users\GLOBALPRO\...`) — no son ejecutables en CI.

---

## 1. Tests y Fakers — lo que falta ahora mismo

### 1.1 Fakers nuevos a crear

**`FakerPatientAlert.cs`** — `tests/SharedFakers/Fakers/Clinical/`
```csharp
public static class FakerPatientAlert
{
    public static Faker<CreatePatientAlertRequestDTO> Create(long patientId, long orgId, int alertTypeId) =>
        new Faker<CreatePatientAlertRequestDTO>()
            .CustomInstantiator(f => new CreatePatientAlertRequestDTO(
                PatientId: patientId,
                AlertTypeId: alertTypeId,
                Severity: f.PickRandom("LOW", "MEDIUM", "HIGH", "CRITICAL"),
                Message: f.Lorem.Sentence(6),
                SourceTypeId: 1,
                SourceId: null
            ) { OrganizationId = orgId });

    public static async Task SeedAsync(IClinicalAlertService svc, long patientId, long orgId, int alertTypeId)
    {
        var request = Create(patientId, orgId, alertTypeId).Generate();
        await svc.CreateAlertAsync(request);
    }
}
```

**`FakerPatientContact.cs`** — `tests/SharedFakers/Fakers/Clinical/`
```csharp
public static class FakerPatientContact
{
    public static Faker<CreatePatientContactRequestDTO> Create(long patientId, long orgId) =>
        new Faker<CreatePatientContactRequestDTO>("es")
            .CustomInstantiator(f => new CreatePatientContactRequestDTO(
                PatientId: patientId,
                ContactTypeId: 1,
                FullName: f.Name.FullName(),
                Phone: f.Phone.PhoneNumber("####-####"),
                Email: f.Internet.Email(),
                Relationship: f.PickRandom("Madre", "Padre", "Cónyuge", "Hermano/a"),
                IsPrimary: true
            ) { OrganizationId = orgId });
}
```

**`FakerAppointment.cs`** — `tests/SharedFakers/Fakers/Care/`
```csharp
public static class FakerAppointment
{
    public static Faker<AppointmentRequestDTO> Create(long orgId, long patientId, long doctorId) =>
        new Faker<AppointmentRequestDTO>()
            .CustomInstantiator(f => new AppointmentRequestDTO
            {
                OrganizationId = orgId,
                PatientId = patientId,
                DoctorUserId = doctorId,
                AppointmentDate = f.Date.Soon(7),
                StartTime = TimeOnly.FromDateTime(f.Date.Soon()),
                Reason = f.Lorem.Sentence(4),
                StatusId = 1
            });

    public static async Task SeedAsync(IMedicalCareService svc, long orgId, long patientId, long doctorId)
    {
        var request = Create(orgId, patientId, doctorId).Generate();
        await svc.AgendarCitaAsync(request);
    }
}
```

**`FakerPayment.cs`** — `tests/SharedFakers/Fakers/Billing/`
```csharp
public static class FakerPayment
{
    public static Faker<PaymentRequestDTO> Create(long invoiceId, long orgId) =>
        new Faker<PaymentRequestDTO>()
            .CustomInstantiator(f => new PaymentRequestDTO
            {
                OrganizationId = orgId,
                InvoiceId = invoiceId,
                PaymentMethodId = f.PickRandom(1, 2), // 1: Efectivo, 2: Tarjeta
                AmountPaid = f.Random.Decimal(50, 300),
                TransactionReference = f.Random.AlphaNumeric(10).ToUpper()
            });
}
```

**`FakerLabTemplate.cs`** — `tests/SharedFakers/Fakers/Laboratory/`
```csharp
public static class FakerLabTemplate
{
    public static Faker<CloneTemplateRequestDTO> Clone(long orgId, long templateId) =>
        new Faker<CloneTemplateRequestDTO>()
            .CustomInstantiator(_ => new CloneTemplateRequestDTO
            {
                OrganizationId = orgId,
                TemplateId = templateId
            });

    public static Faker<UpdateLabConfigItemRequestDTO> UpdateItem(long orgId, long orgConfigId, long itemId) =>
        new Faker<UpdateLabConfigItemRequestDTO>()
            .CustomInstantiator(f => new UpdateLabConfigItemRequestDTO
            {
                OrganizationId = orgId,
                OrgConfigId = orgConfigId,
                TemplateItemId = itemId,
                CustomName = f.Lorem.Word() + " (custom)",
                CustomRefMin = f.Random.Decimal(1, 10),
                CustomRefMax = f.Random.Decimal(11, 50),
                IsHidden = false
            });
}
```

### 1.2 Tests de integración a crear

**`ClinicalAlertServiceTests.cs`** — `tests/IntegrationTests/Service/`
```
Tests requeridos:
✅ CreateAlertAsync_DebePersistirAlertaConSeveridadCritica
✅ GetActiveAlertsAsync_DebeRetornarSoloAlertasActivas
✅ AcknowledgeAlertAsync_DebeMarcarComoAcknowledged
✅ CreateAlertAsync_ConSeveridadInvalida_DebeFallarValidacion
✅ GetActiveAlertsAsync_PacienteSinAlertas_DebeRetornarListaVacia
```

**`PatientContactServiceTests.cs`** — `tests/IntegrationTests/Service/`
```
Tests requeridos:
✅ CreateContactAsync_DebePersistirContactoConTelefono
✅ GetContactsAsync_DebeOrdenarPrimariosPrimero
✅ UpdateContactAsync_DebeActualizarCamposCorrectamente
✅ DeleteContactAsync_DebeSoftDeleteNoFisicoEliminar
```

**`BillingPaymentFlowTests.cs`** — ampliar `BillingServiceTests.cs`
```
Tests requeridos:
✅ RegistrarPago_TotalExacto_DebeMarcarFacturaPagada
✅ RegistrarPago_MontoMenorAlTotal_DebeDejarSaldoPendiente  ← PAGO PARCIAL
✅ GetInvoicePaymentsAsync_DebeRetornarHistorialOrdenado
✅ GetPatientBalanceAsync_ConMultiplesFacturasPendientes
✅ GetDailyClosingAsync_ConPagosEfectivoYTarjeta
✅ AnularFactura_ConMotivo_DebeGuardarCancelReason
```

**`LaboratoryTemplateTests.cs`** — ampliar `LaboratoryServiceTests.cs`
```
Tests requeridos:
✅ CloneTemplateAsync_DebeCrearConfiguracionParaOrg
✅ GetTemplateWithConfigAsync_SinPersonalizacion_UsaValoresGlobales
✅ UpdateLabConfigItemAsync_DebeGuardarValoresCustom
✅ GetTemplateWithConfigAsync_ConPersonalizacion_UsaValoresCustom
✅ ResetTemplateToDefaultAsync_DebeLimpiarValoresCustom
✅ GetTemplateWithConfigAsync_ConPaciente_AplicaRangosDemograficos
```

**`AppointmentServiceTests.cs`** — `tests/IntegrationTests/Service/`
```
Tests requeridos:
✅ AgendarCita_DebeCrearCitaConEstadoPendiente
✅ AgendarCita_FechaEnPasado_DebeFallarValidacion (pendiente implementar regla)
```

### 1.3 Corregir tests de Reporting — ruta hardcodeada

**Archivo:** `ReportingServiceTests.cs`

Las rutas `C:\Users\GLOBALPRO\...` hacen que los tests fallen en cualquier máquina que no sea la del desarrollador y en CI/CD. Reemplazar con:

```csharp
// Reemplazar:
string outputPath = @"C:\Users\GLOBALPRO\...\FACTURA_A4_TEST.pdf";
await File.WriteAllBytesAsync(outputPath, pdfBytes);
Assert.True(File.Exists(outputPath));

// Por:
Assert.That(pdfBytes.Length, Is.GreaterThan(1000), "El PDF debe tener contenido real");
Assert.That(pdfBytes[0], Is.EqualTo(0x25), "Debe comenzar con '%' (header PDF)");
Assert.That(pdfBytes[1], Is.EqualTo(0x50), "Debe continuar con 'P'");
// Guardar solo si estamos en modo DEBUG local:
if (Environment.GetEnvironmentVariable("SAVE_TEST_PDFS") == "true")
    await File.WriteAllBytesAsync(Path.GetTempFileName() + ".pdf", pdfBytes);
```

### 1.4 Integrar fakers nuevos al `MasterSeeder`

Agregar al loop del seeder para que los tests de integración tengan datos realistas:

```csharp
// En el bucle de pacientes del MasterSeeder, después de registrar al paciente:

// Contacto de emergencia
await FakerPatientContact.Create(patientId, orgId).Generate()
    .Let(async r => await patientContactService.CreateContactAsync(r, adminUserId));

// Alerta clínica (para el 30% de los pacientes)
if (i % 3 == 0)
    await FakerPatientAlert.SeedAsync(alertService, patientId, orgId, alertTypeId);

// Cita médica
await FakerAppointment.SeedAsync(careService, orgId, patientId, adminUserId);
```

---

## 2. Features que faltan construir para ser un sistema de elite

### 2.1 🔴 CRÍTICO — Módulo de Alergias (`RegistrarAlergia = 4109`)

La constante existe pero no hay: entidad, repositorio, servicio, DTO, tabla SQL ni handler registrado. Las alergias son **información de seguridad crítica para el paciente** — un médico necesita verlas antes de prescribir.

**Qué construir:**

```
Entidad:    PatientAllergyEntity (allergen, severity, reaction, onset_date, is_active)
Tabla SQL:  clinical.mst_patient_allergy
Repo:       IPatientAllergyRepository + PatientAllergyRepository
DTOs:       CreatePatientAllergyRequestDTO [ActionMapping(4109)]
            GetPatientAllergiesRequestDTO  [ActionMapping(nuevo 4121)]
            UpdatePatientAllergyRequestDTO [ActionMapping(nuevo 4122)]
Service:    IPatientAllergyService + PatientAllergyService
Domain:     Registrar en ClinicalDomain
SQL:        En migración 023 — tabla + acciones + índice
```

Campos clave de la tabla:
```sql
CREATE TABLE clinical.mst_patient_allergy (
    id BIGSERIAL PRIMARY KEY,
    patient_id BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    organization_id BIGINT NOT NULL,
    allergen VARCHAR(200) NOT NULL,       -- "Penicilina", "Mariscos", "Polen"
    allergy_type_id INT,                  -- Medicamento, Alimento, Ambiental
    severity VARCHAR(20),                 -- MILD, MODERATE, SEVERE, LIFE_THREATENING
    reaction TEXT,                        -- Descripción de la reacción
    onset_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT
);
```

### 2.2 🔴 CRÍTICO — Dashboard Clínico (`ClinicalDashboardStatsDTO` sin implementar)

El DTO `ClinicalDashboardStatsDTO` existe con `TotalAppointmentsToday`, `PatientsWaiting`, `LabResultsReady`, `PatientQueue` — pero **no hay servicio, acción ni repositorio** que lo alimente. Es la pantalla principal del médico al entrar al sistema.

**Qué construir:**

```csharp
// Nueva acción:
public const int GetDashboardStats = 5007;

// En IMedicalCareService agregar:
Task<BaseResponse<ClinicalDashboardStatsDTO>> GetDashboardStatsAsync(GetDashboardStatsRequestDTO request);

// Query SQL en AppointmentRepository:
SELECT 
    COUNT(*) FILTER (WHERE DATE(appointment_date) = TODAY) as total_today,
    COUNT(*) FILTER (WHERE status_id = 1 AND DATE(appointment_date) = TODAY) as waiting,
    (SELECT COUNT(*) FROM laboratory.tbl_lab_result WHERE status_id = 3 AND org_id = @OrgId) as lab_ready
FROM care.mst_appointment WHERE organization_id = @OrgId;
```

### 2.3 🟡 ALTO — Cierre de Consulta → Borrador de Factura automático

`CerrarConsultaMedicaAsync` existe y registra el ledger. Pero no genera un borrador de factura automático que el recepcionista pueda confirmar y cobrar. Este es el flujo natural de una clínica:

1. Médico cierra consulta → sistema genera `Invoice` con `status_id = Draft (1)` automáticamente
2. Recepcionista ve el borrador → confirma y cobra → `status_id = Unpaid (2)`
3. Paciente paga → `status_id = Paid (3)`

**Cambio en `CerrarConsultaMedicaAsync`:**
```csharp
// Al final del método, después de cerrar el ledger:
// Auto-generar borrador de factura con el servicio "Consulta médica"
var consultationService = await _unitOfWork.Services.GetBySkuAsync("CONS-GEN", request.OrganizationId);
if (consultationService != null)
{
    await _unitOfWork.Invoices.AddAsync(new Invoice
    {
        OrganizationId = request.OrganizationId,
        PatientId      = consultation.PatientId,
        ConsultationId = consultation.Id,
        StatusId       = (int)InvoiceStatusEnum.Draft,
        InvoiceNumber  = $"DRAFT-{consultation.Id}",
        Subtotal       = consultationService.BasePrice,
        TotalAmount    = consultationService.BasePrice,
        BranchId       = request.BranchId
    });
}
```

### 2.4 🟡 ALTO — Notificaciones de Cita por Email/SMS

`AgendarCitaAsync` crea la cita pero no envía ninguna confirmación al paciente ni recordatorio. Dado que `IEmailService` ya existe, la implementación es directa:

**Crear `AppointmentNotificationOutputAction.cs`:**
```csharp
[RegisterScoped(ServiceType = typeof(IOutputAction))]
public class AppointmentNotificationOutputAction : IOutputAction
{
    public bool ShouldExecute(OutputContextDto context) =>
        context.ActionId == AppAction.Care.GestionarCita;

    public async Task ExecuteAsync(OutputContextDto context)
    {
        // Extraer datos de la cita del contexto
        // Enviar email de confirmación al paciente
        // Opcional: programar recordatorio 24h antes via SQS
    }
}
```

### 2.5 🟡 ALTO — Lab Templates conectados al Domain

Del análisis anterior: `CloneTemplate`, `UpdateConfig` y `SaveStructuredResult` tienen implementación completa pero **no están registrados en `LaboratoryDomain`**. Tres líneas que desbloquean todo el módulo de plantillas:

```csharp
// En LaboratoryDomain — AGREGAR:
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.CloneTemplate,
    nameof(ILaboratoryService.CloneTemplateAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.UpdateConfig,
    nameof(ILaboratoryService.UpdateLabConfigItemAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.SaveStructuredResult,
    nameof(ILaboratoryService.SaveStructuredResultAsync));
```

### 2.6 🟡 ALTO — Pago parcial en `RegistrarPagoAsync`

`UpdateBalanceAsync` en el repo está perfecto. Pero el servicio todavía no lo usa:

```csharp
// REEMPLAZAR en RegistrarPagoAsync:
var newAmountPaid = invoice.AmountPaid + request.AmountPaid;
var newBalance    = invoice.TotalAmount - newAmountPaid;
var newStatus     = newBalance <= 0
    ? (int)InvoiceStatusEnum.Paid
    : (int)InvoiceStatusEnum.PartiallyPaid;

await _unitOfWork.Invoices.UpdateBalanceAsync(invoice.Id, newAmountPaid, newBalance, newStatus);
```

### 2.7 🟢 NORMAL — Observabilidad: corregir tests de Reporting y completar `TelemetryOutputAction`

`TelemetryOutputAction.cs` existe pero hay que verificar que esté registrado para todos los módulos, no solo para algunos. El patrón del skill `observability-and-tracing` indica que debe dispararse en **toda ejecución del dispatcher** — actualmente solo lo hace para acciones específicas.

### 2.8 🟢 NORMAL — `GetDashboardStats` para Laboratorio

El laboratorio también necesita su dashboard: órdenes pendientes, muestras recibidas hoy, resultados listos para entrega. Mismo patrón que §2.2 pero en `LaboratoryService`.

### 2.9 🟢 NORMAL — Cambio de contraseña / Recuperación de cuenta

No existe ningún endpoint de `CambiarContrasena` ni `RecuperarContrasena`. Para producción son obligatorios.

```
Acciones nuevas:
2xxx: ChangePassword  — usuario autenticado cambia su contraseña
2xxx: RequestPasswordReset — genera token y envía email
2xxx: ConfirmPasswordReset — valida token y aplica nueva contraseña
```

---

## 3. Resumen priorizado — orden de ataque

```
SEMANA 1 — Estabilizar lo que existe
├── Corregir los 8 bugs críticos del análisis Phase 1 (§2 del doc anterior)
├── Crear FakerPatientAlert, FakerPatientContact, FakerPayment, FakerAppointment, FakerLabTemplate
├── Crear tests: ClinicalAlertServiceTests, PatientContactServiceTests, BillingPaymentFlowTests
└── Registrar 3 handlers faltantes en LaboratoryDomain

SEMANA 2 — Completar funcionalidad core
├── Implementar módulo de Alergias completo (entidad → repo → servicio → domain → SQL)
├── Implementar GetDashboardStatsAsync para médico
├── Conectar pago parcial en RegistrarPagoAsync
├── Crear LaboratoryTemplateTests + AppointmentServiceTests
└── Corregir tests de Reporting (ruta hardcodeada)

SEMANA 3 — Features de elite
├── AppointmentNotificationOutputAction (email confirmación cita)
├── Cierre consulta → auto borrador de factura
├── GetDashboardStats para laboratorio
├── Módulo cambio/recuperación de contraseña
└── Integrar fakers nuevos al MasterSeeder
```

---

## 4. Tabla: fakers vs tests — estado completo

| Módulo | Faker existe | Tests integración | Tests unitarios |
|---|---|---|---|
| Identity | ✅ FakerPerson, FakerOrganization, FakerAppUser | 4 tests | 3 tests |
| Clinical base | ✅ FakerClinical, FakerDoctorConfiguration, PatientConsentFaker | 3 tests | 2 tests |
| **Alertas** | ❌ **Crear FakerPatientAlert** | ❌ **0 tests** | — |
| **Contactos** | ❌ **Crear FakerPatientContact** | ❌ **0 tests** | — |
| **Acceso/Auditoría** | ❌ no aplica faker | ❌ **0 tests** | — |
| **Alergias** | ❌ **módulo no existe** | ❌ **0 tests** | — |
| Care (consulta, receta) | ✅ FakerConsultation, FakerPrescription, FakerVitalSigns | 4 tests | — |
| **Citas** | ❌ **Crear FakerAppointment** | ❌ **0 tests** | — |
| Laboratory | ✅ FakerLabOrder, FakerLaboratory | 1 test | — |
| **Lab Templates** | ❌ **Crear FakerLabTemplate** | ❌ **0 tests** | — |
| Billing (factura) | ✅ FakerBilling | 2 tests | — |
| **Pago / Cierre caja** | ❌ **Crear FakerPayment** | ❌ **0 tests** | — |
| Subscriptions | ✅ FakerSaasPlan, FakerSubscription | 2 tests | — |
| Reporting | — | 2 tests (ruta rota) | — |
| Security/Roles | ✅ SecurityFaker | 1 test | — |

**Total tests actuales: ~22**  
**Tests que deben existir para cobertura básica: ~60**  
**Tests para cobertura de elite (happy path + edge cases): ~120**

---

*Fin del roadmap — MedfarLabs Core · Hacia un sistema de elite*
