# MedfarLabs · Análisis Completo de Gaps
## Fakers · Tests · Sistema de Cobro por Eventos · Seguridad SQL/NoSQL
> Mayo 2025

---

## 0. Estado actual — números reales

| Área | Existe | Falta | % |
|---|---|---|---|
| **Fakers** | 20 archivos | 8 fakers críticos sin escenarios negativos | 55% |
| **Tests integración** | 43 tests en 12 archivos | ~77 tests más para cobertura de elite | 36% |
| **Seguridad SQL** | Dapper parametrizado (✅) | 3 patrones de riesgo activos | 85% |
| **Seguridad NoSQL/JSON** | JsonElement (✅) | Validación de profundidad faltante | 80% |
| **Billing por eventos** | ConsultationLedger (✅) | Pipeline completo: tabla + lambda + SQS | 20% |

---

## 1. Inventario de Fakers — lo que existe y lo que falta

### 1.1 Fakers existentes — cobertura actual

| Faker | Módulo | Escenarios cubiertos | Escenarios faltantes |
|---|---|---|---|
| `FakerPerson` | Identity | Registro básico | Persona con cédula duplicada, menor de edad |
| `FakerOrganization` | Identity | Org básica | Org con plan inactivo, org sin sucursal |
| `FakerAppUser` | Identity | Usuario estándar | Usuario bloqueado, usuario sin rol |
| `BranchFaker` | Identity | Sucursal básica | Sucursal en estado inactivo |
| `FakerClinical` | Clinical | Paciente estándar | Paciente sin expediente, paciente de otra org |
| `FakerPatientAlert` | Clinical | Alerta básica con severidad aleatoria | Alerta con fuente LAB_RESULT, alerta ya acknowledged |
| `FakerPatientContact` | Clinical | Contacto básico | Contacto sin email ni teléfono (debe fallar), 2+ primarios |
| `PatientConsentFaker` | Clinical | Consentimiento básico | Consentimiento revocado |
| `FakerConsultation` | Care | Consulta estándar | Consulta sin diagnóstico, consulta ya cerrada |
| `FakerPrescription` | Care | Receta básica | Receta con medicamento alérgico (debe alertar) |
| `FakerVitalSigns` | Care | Signos vitales básicos | Valores fuera de rango (críticos) |
| `FakerAppointment` | Care | Cita básica | Cita en fecha pasada, cita duplicada mismo horario |
| `FakerBilling` | Billing | Factura con ítems | Factura sin ítems (debe fallar) |
| `FakerPayment` | Billing | Pago básico | Pago mayor al saldo (cambio), pago en NIO |
| `ConsultationLedgerFaker` | Billing | Ledger básico | Ledger del mes anterior, ledger ya facturado |
| `FakerSaasPlan` | Billing | Plan básico | Plan pay-per-use, plan sin precio (debe fallar) |
| `SubscriptionInvoiceFaker` | Billing | Factura sub básica | Factura vencida, factura en grace period |
| `FakerLabOrder` | Laboratory | Orden básica | Orden rechazada, orden sin muestra |
| `FakerLabTemplate` | Laboratory | Clon y update de config | Config ya existente (debe fallar), reset a defaults |
| `FakerInventory` | Inventory | Servicio básico | Servicio sin precio, SKU duplicado |
| `MedicationFaker` | Pharmacy | Medicamento básico | Medicamento sin presentación |
| `SecurityFaker` | Security | Rol básico | Rol sin permisos asignados |

### 1.2 Fakers que NO existen — crear urgente

#### `FakerPatientAllergy.cs` — `tests/SharedFakers/Fakers/Clinical/`
```csharp
public static class FakerPatientAllergy
{
    public static Faker<CreatePatientAllergyRequestDTO> Create(long patientId, long orgId) =>
        new Faker<CreatePatientAllergyRequestDTO>("es")
            .CustomInstantiator(f => new CreatePatientAllergyRequestDTO(
                PatientId: patientId,
                Allergen: f.PickRandom("Penicilina", "Amoxicilina", "Ibuprofeno", "Mariscos", "Polen", "Látex"),
                AllergyTypeId: (int)f.PickRandom<AllergyTypeEnum>(),
                SeverityId: (int)f.PickRandom<AllergySeverityEnum>(),
                Reaction: f.Lorem.Sentence(4),
                OnsetDate: f.Date.Past(5)
            ) { OrganizationId = orgId });

    // Escenario: alergia crítica (anafilaxis)
    public static Faker<CreatePatientAllergyRequestDTO> CreateCritical(long patientId, long orgId) =>
        Create(patientId, orgId).RuleFor(r => r,
            f => new CreatePatientAllergyRequestDTO(
                patientId, "Penicilina", 1, (int)AllergySeverityEnum.Anaphylaxis,
                "Choque anafiláctico documentado", f.Date.Past(2)
            ) { OrganizationId = orgId });
}
```

#### `FakerOrgBillingEvent.cs` — `tests/SharedFakers/Fakers/Billing/`
```csharp
// Para el nuevo sistema de cobro por eventos (ver Sección 3)
public static class FakerOrgBillingEvent
{
    public static Faker<OrgBillingEvent> Create(long orgId, long invoiceId) =>
        new Faker<OrgBillingEvent>()
            .CustomInstantiator(f => new OrgBillingEvent
            {
                OrganizationId = orgId,
                EventType = f.PickRandom<BillingEventType>(),
                ReferenceId = invoiceId,
                Amount = f.Random.Decimal(10, 500),
                BillingYear = DateTime.UtcNow.Year,
                BillingMonth = DateTime.UtcNow.Month,
                StatusId = (int)BillingEventStatus.Pending
            });

    // Escenario: evento del mes anterior (para pruebas de cierre mensual)
    public static Faker<OrgBillingEvent> CreatePreviousMonth(long orgId, long invoiceId) =>
        Create(orgId, invoiceId).RuleFor(x => x.BillingMonth,
            DateTime.UtcNow.Month == 1 ? 12 : DateTime.UtcNow.Month - 1);
}
```

#### `FakerMonthlyBillingCycle.cs` — `tests/SharedFakers/Fakers/Billing/`
```csharp
// Para el ciclo mensual de cobro de suscripciones
public static class FakerMonthlyBillingCycle
{
    public static Faker<MonthlyBillingCycleRequestDTO> Create(long orgId) =>
        new Faker<MonthlyBillingCycleRequestDTO>()
            .CustomInstantiator(f => new MonthlyBillingCycleRequestDTO
            {
                OrganizationId = orgId,
                BillingYear = DateTime.UtcNow.Year,
                BillingMonth = DateTime.UtcNow.Month == 1 ? 12 : DateTime.UtcNow.Month - 1
            });
}
```

#### `FakerLabResultItem.cs` — `tests/SharedFakers/Fakers/Laboratory/`
```csharp
public static class FakerLabResultItem
{
    // Ítem con valor dentro de rango
    public static Faker<StructuredResultItemDTO> CreateNormal(long itemId) =>
        new Faker<StructuredResultItemDTO>()
            .CustomInstantiator(f => new StructuredResultItemDTO
            {
                ItemId = itemId,
                NumericValue = f.Random.Decimal(5, 10), // Dentro de rango típico
                TextValue = null
            });

    // Ítem fuera de rango — debe disparar alerta
    public static Faker<StructuredResultItemDTO> CreateOutOfRange(long itemId) =>
        new Faker<StructuredResultItemDTO>()
            .CustomInstantiator(f => new StructuredResultItemDTO
            {
                ItemId = itemId,
                NumericValue = f.Random.Decimal(50, 100), // Fuera de rango
                TextValue = null
            });

    // Ítem cualitativo
    public static Faker<StructuredResultItemDTO> CreateQualitative(long itemId) =>
        new Faker<StructuredResultItemDTO>()
            .CustomInstantiator(_ => new StructuredResultItemDTO
            {
                ItemId = itemId,
                NumericValue = null,
                TextValue = "Positivo"
            });
}
```

#### `FakerDashboard.cs` — `tests/SharedFakers/Fakers/Care/`
```csharp
public static class FakerDashboard
{
    public static GetDashboardStatsRequestDTO Create(long orgId, long branchId) =>
        new GetDashboardStatsRequestDTO { OrganizationId = orgId, BranchId = branchId };
}
```

#### `FakerPasswordReset.cs` — `tests/SharedFakers/Fakers/Identity/`
```csharp
public static class FakerPasswordReset
{
    public static Faker<ChangePasswordRequestDTO> Create(long userId) =>
        new Faker<ChangePasswordRequestDTO>()
            .CustomInstantiator(f => new ChangePasswordRequestDTO
            {
                UserId = userId,
                CurrentPassword = "OldPassword123!",
                NewPassword = f.Internet.Password(12),
            });

    // Escenario negativo: contraseña nueva igual a la actual
    public static ChangePasswordRequestDTO CreateSamePassword(long userId) =>
        new ChangePasswordRequestDTO
        {
            UserId = userId,
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!"
        };
}
```

#### `FakerClinicalNote.cs` — `tests/SharedFakers/Fakers/Care/`
```csharp
public static class FakerClinicalNote
{
    public static Faker<SaveClinicalNoteTemplateRequestDTO> Create(long orgId) =>
        new Faker<SaveClinicalNoteTemplateRequestDTO>("es")
            .CustomInstantiator(f => new SaveClinicalNoteTemplateRequestDTO
            {
                OrganizationId = orgId,
                Name = $"Nota SOAP - {f.Lorem.Word()}",
                NoteTypeId = 1, // SOAP
                TemplateContent = "S: {{subjetivo}}\nO: {{objetivo}}\nA: {{evaluacion}}\nP: {{plan}}",
                Variables = new[] { "subjetivo", "objetivo", "evaluacion", "plan" },
                IsDefault = false
            });
}
```

#### `FakerScheduleException.cs` — `tests/SharedFakers/Fakers/Care/`
```csharp
public static class FakerScheduleException
{
    public static Faker<CreateScheduleExceptionRequestDTO> Create(long doctorId, long orgId) =>
        new Faker<CreateScheduleExceptionRequestDTO>()
            .CustomInstantiator(f => new CreateScheduleExceptionRequestDTO(
                DoctorUserId: doctorId,
                ExceptionDate: DateOnly.FromDateTime(f.Date.Soon(30)),
                ExceptionTypeId: 1, // VACATION
                StartTime: null,
                EndTime: null,
                Reason: f.Lorem.Sentence(3),
                IsAllDay: true
            ) { OrganizationId = orgId });
}
```

---

## 2. Tests que faltan — 77 adicionales para cobertura de elite

### 2.1 Tests de módulos sin cobertura

#### `PatientAllergyServiceTests.cs` — **0 tests actualmente → crear 8**
```
✅ CreateAllergyAsync_DebePersistirAlergiaMedicamento
✅ CreateAllergyAsync_AlergiaAnafilictica_DebeCrearAlertaCritica       ← OutputAction
✅ GetAllergiesAsync_DebeRetornarSoloAlergiasPaciente
✅ GetAllergiesAsync_PacienteOtraOrg_NoDebeRetornarDatos              ← Multitenancy
✅ UpdateAllergyAsync_DebeActualizarSeveridad
✅ DeleteAllergyAsync_DebeSoftDelete
✅ CreateAllergyAsync_AlergiaExistente_MismoPaciente_DebePermitir      ← Edge case
✅ CreateAllergyAsync_SinAlergeno_DebeFallarValidacion                 ← Negativo
```

#### `DashboardServiceTests.cs` — **0 tests actualmente → crear 5**
```
✅ GetDashboardStatsAsync_DebeRetornarCitasDeHoy
✅ GetDashboardStatsAsync_SinCitas_DebeRetornarCeros
✅ GetDashboardStatsAsync_ConResultadosLabListos_DebeIncluirConteo
✅ GetDashboardStatsAsync_OrgSinActividad_NoDebeFallar
✅ GetDashboardStatsAsync_OrganizacionDiferente_AislamientoCorrecto    ← Multitenancy
```

#### `PasswordManagementTests.cs` — **0 tests actualmente → crear 6**
```
✅ ChangePasswordAsync_ConPasswordCorrecta_DebeActualizar
✅ ChangePasswordAsync_PasswordActualIncorrecta_DebeFallar
✅ ChangePasswordAsync_NuevaPasswordIgualActual_DebeFallar
✅ RequestPasswordResetAsync_EmailExistente_DebeEnviarToken
✅ RequestPasswordResetAsync_EmailNoExistente_NoDebeRevelarInfo       ← Security
✅ ConfirmPasswordResetAsync_TokenVencido_DebeFallar
```

#### `BillingEventTests.cs` — **0 tests actualmente → crear 9** (ver Sección 3)
```
✅ GenerarFactura_DebeCrearEventoBillingEnMaestro
✅ GetOrgBillingEventsAsync_DebeRetornarEventosDelMes
✅ CerrarPeriodoMensualAsync_DebeAgruparEventosPorOrg
✅ CerrarPeriodoMensualAsync_OrgPayPerUse_DebeCalcularMontoPorConsultas
✅ CerrarPeriodoMensualAsync_OrgFijo_DebeUsarPrecioMensualDePlan
✅ CerrarPeriodoMensualAsync_SinEventos_NoDebeGenerarFactura
✅ GenerarFacturaSaas_DebeEnviarNotificacionSQS                       ← OutputAction
✅ CerrarPeriodo_GracePeriodActivo_NoDebeSuspender
✅ CerrarPeriodo_FueraDGrace_DebeMarcarPastDue
```

#### `LabTemplateWorkflowTests.cs` — ampliar `LaboratoryWorkflowTests.cs` → **crear 8**
```
✅ GetTemplateWithConfigAsync_SinClon_UsaValoresGlobales
✅ GetTemplateWithConfigAsync_ConClon_UsaValoresCustom
✅ GetTemplateWithConfigAsync_ConPaciente_AplicaRangosDemograficos
✅ UpdateLabConfigItemAsync_CambiaRango_GetDevuelveNuevoRango
✅ ResetTemplateToDefaultAsync_LimpiaValoresCustom
✅ SaveStructuredResultAsync_ValorFueraRango_MarcaIsOutOfRange
✅ SaveStructuredResultAsync_ValorCualitativo_AceptaTexto
✅ CloneTemplateAsync_YaExiste_DebeFallar
```

### 2.2 Tests adicionales en módulos existentes

#### `BillingServiceTests.cs` — ampliar de 10 → **20 tests**
```
Faltantes:
✅ RegistrarPago_MontoMenorAlTotal_DejaBalancePendiente               ← Pago parcial
✅ RegistrarPago_MontoExacto_MarcaFacturaPagada
✅ RegistrarPago_MontoMayorAlSaldo_UsaExcedente                       ← Edge case
✅ RegistrarPago_EnNIO_RegistraTasaDeCambio
✅ AnularFactura_ConMotivo_GuardaCancelReason
✅ AnularFactura_YaPagada_DebeFallar                                   ← Negativo
✅ GetDailyClosingAsync_ConPagosEfectivoYTarjeta_AgrupaPorMetodo
✅ GetPatientBalanceAsync_ConMultiplesFacturas_SumaCorrectamente
✅ BuscarFactura_PorNumero_RetornaResultado
✅ BuscarFactura_DatosDeOtraOrg_NoRetornaNada                         ← Multitenancy
```

#### `AppointmentServiceTests.cs` — ampliar de 2 → **8 tests**
```
Faltantes:
✅ AgendarCita_FechaEnPasado_DebeFallar
✅ AgendarCita_MismoHorarioMismoDoctorMismaSucursal_DebeFallar        ← Conflicto
✅ AgendarCita_EnviaEmailConfirmacion                                  ← OutputAction
✅ CancelarCita_DebeCambiarEstado
✅ GetCitasDeHoy_DebeRetornarSoloDelDia
✅ GetCitasDeHoy_OrgDiferente_AislamientoCorrecto
```

#### `MedicalCareServiceTests.cs` — ampliar de 4 → **12 tests**
```
Faltantes:
✅ CerrarConsulta_DebeGenerarBorradorDeFactura                        ← Auto factura
✅ CerrarConsulta_RegistraEnConsultationLedger
✅ CerrarConsulta_RegistraBillingEvent
✅ RegistrarVitalSigns_FueraDRango_DebeCrearAlerta                    ← OutputAction
✅ GetConsultacionesPorPaciente_SoloMismaOrg                          ← Multitenancy
✅ GenerarReceta_ConMedicamentoAlergico_DebeAdvertir
✅ GenerarReceta_SinItems_DebeFallar
✅ GetPrescriptionFulfillment_DebeRetornarEstado
```

#### `SecurityDataIsolationTests.cs` — ampliar de 1 → **6 tests**
```
Faltantes:
✅ Ensure_Lab_Result_IsolationAcrossOrgs
✅ Ensure_Invoice_IsolationAcrossOrgs
✅ Ensure_Appointment_IsolationAcrossOrgs
✅ Attempt_SQLInjectionInSearch_MustFail                              ← Security
✅ Attempt_SQLInjectionInInvoiceNumber_MustFail                       ← Security
✅ Attempt_XSSInPatientName_MustBeSanitized                           ← Security
```

### 2.3 Tests de Reporting — corregir y ampliar de 2 → **6 tests**
```
Corregir:
✅ GenerarFacturaPDF_A4_DebeTenerContenido           ← Eliminar ruta Windows hardcodeada
✅ GenerarFacturaPDF_Ticket_DebeSerMasPequeno

Nuevos:
✅ GenerarFacturaPDF_ConPagoParcial_MuestraAbono
✅ GenerarFacturaPDF_ConDescuento_MuestraDescuento
✅ GenerarLabResultadoPDF_ConValoresFueraRango_MarcaEnRojo
✅ GenerarRecetaPDF_DebeTenerFirmaYSello
```

---

## 3. Sistema de Cobro por Eventos — pipeline completo

### 3.1 Arquitectura propuesta

```
Factura emitida por médico
        │
        ▼
[OutputAction: OrgBillingEventOutputAction]
        │
        ▼
billing.log_org_billing_event   ← registro maestro-detalle
(organization_id, event_type, reference_id, amount, billing_month)
        │
        ▼ (cada día 30 del mes)
[AWS Lambda: MonthlyBillingJob]
        │
        ├── Consulta log_org_billing_event WHERE status = Pending AND billing_month = mes anterior
        ├── Agrupa por organization_id
        ├── Calcula monto: fijo (plan mensual) O variable (sum(amount) para pay-per-use)
        ├── Genera billing.tbl_subscription_invoice
        ├── Actualiza status de eventos → Billed
        ├── Envía SQS → NotificationWorker (email/push de factura)
        └── Actualiza subscription.current_period_end
```

### 3.2 Nuevas entidades de dominio

**`OrgBillingEvent.cs`** — `src/Domain/Entities/Billing/`
```csharp
public class OrgBillingEvent : BaseEntity
{
    [DbColumn("organization_id")] public long OrganizationId { get; set; }
    [DbColumn("event_type_id")]   public int EventTypeId { get; set; }   // FK catalog BILLING_EVENT_TYPE
    [DbColumn("reference_id")]    public long? ReferenceId { get; set; }  // invoice_id, consultation_id, etc.
    [DbColumn("amount")]          public decimal Amount { get; set; }
    [DbColumn("billing_year")]    public int BillingYear { get; set; }
    [DbColumn("billing_month")]   public int BillingMonth { get; set; }
    [DbColumn("status_id")]       public int StatusId { get; set; }       // Pending / Billed / Void
    [DbColumn("billed_invoice_id")] public long? BilledInvoiceId { get; set; } // FK a subscription_invoice
    [DbColumn("trace_id")]        public string? TraceId { get; set; }
    [DbColumn("created_at")]      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [IgnoreOnUpdate]              public long CreatedByUserId { get; set; }
}
```

### 3.3 Nuevos enums

**`BillingEventType.cs`** — `src/Domain/Enums/Billing/`
```csharp
public enum BillingEventType
{
    [Display(Name = "Factura emitida al paciente")]
    PatientInvoiceIssued = 1,

    [Display(Name = "Consulta cerrada (pay-per-use)")]
    ConsultationClosed = 2,

    [Display(Name = "Pago recibido del paciente")]
    PatientPaymentReceived = 3,

    [Display(Name = "Laboratorio: resultado entregado")]
    LabResultDelivered = 4,

    [Display(Name = "Ajuste manual de crédito")]
    ManualCreditAdjustment = 5
}
```

**`BillingEventStatus.cs`** — `src/Domain/Enums/Billing/`
```csharp
public enum BillingEventStatus
{
    [Display(Name = "Pendiente de facturar")]
    Pending = 1,

    [Display(Name = "Facturado")]
    Billed = 2,

    [Display(Name = "Anulado")]
    Void = 3
}
```

### 3.4 OutputAction nuevo

**`OrgBillingEventOutputAction.cs`** — `src/Application/Features/Billing/OutputActions/`
```csharp
[RegisterScoped(ServiceType = typeof(IOutputAction))]
public class OrgBillingEventOutputAction : IOutputAction
{
    private readonly IOrgBillingEventRepository _eventRepo;
    private readonly ILogger<OrgBillingEventOutputAction> _logger;

    // Se dispara cuando el médico emite una factura al paciente
    public bool ShouldExecute(OutputContextDto context) =>
        context.Response.IsSuccess &&
        context.ActionId == AppAction.Billling.GenerarFactura &&
        Environment.GetEnvironmentVariable("EXECUTION_CONTEXT") == "Main";

    public async Task ExecuteAsync(OutputContextDto context)
    {
        try
        {
            var invoiceId = context.Response.Data is long id ? id : 0;
            if (invoiceId == 0) return;

            var billingEvent = new OrgBillingEvent
            {
                OrganizationId = context.UserContext.OrganizationId,
                EventTypeId = (int)BillingEventType.PatientInvoiceIssued,
                ReferenceId = invoiceId,
                Amount = 0, // Se llena desde la factura en el repo
                BillingYear = DateTime.UtcNow.Year,
                BillingMonth = DateTime.UtcNow.Month,
                StatusId = (int)BillingEventStatus.Pending,
                TraceId = context.TraceId,
                CreatedByUserId = context.UserContext.UserId
            };

            await _eventRepo.AddAsync(billingEvent);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "Error registrando BillingEvent para factura. TraceId: {TraceId}", context.TraceId);
        }
    }
}
```

### 3.5 Lambda de cierre mensual

**`MonthlyBillingJob.cs`** — `src/Infrastructure/Jobs/`
```csharp
// Esta Lambda se dispara el día 30 de cada mes vía EventBridge Scheduler
public class MonthlyBillingJob
{
    private readonly IBillingService _billingService;
    private readonly ILogger<MonthlyBillingJob> _logger;

    public async Task HandleAsync()
    {
        var targetYear = DateTime.UtcNow.Month == 1
            ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year;
        var targetMonth = DateTime.UtcNow.Month == 1
            ? 12 : DateTime.UtcNow.Month - 1;

        _logger.LogInformation(
            "Iniciando cierre mensual. Periodo: {Year}/{Month}", targetYear, targetMonth);

        var result = await _billingService.EjecutarCierreMensualAsync(
            new MonthlyBillingCycleRequestDTO
            {
                BillingYear = targetYear,
                BillingMonth = targetMonth
            });

        if (!result.IsSuccess)
            _logger.LogCritical(
                "FALLO cierre mensual {Year}/{Month}: {Message}", targetYear, targetMonth, result.Message);
    }
}
```

### 3.6 Script SQL — `025_Org_Billing_Events.sql`

```sql
-- BLOQUE A: Catálogos nuevos
INSERT INTO common.mst_catalog (id, name, description) VALUES
(32, 'BILLING_EVENT_TYPE', 'Tipos de eventos de facturación SaaS'),
(33, 'BILLING_EVENT_STATUS', 'Estados del evento de facturación')
ON CONFLICT (id) DO NOTHING;

INSERT INTO common.mst_catalog_detail (catalog_id, code, name, enum_mapping) VALUES
(32, 'EVT_INV',  'Factura emitida al paciente',       1),
(32, 'EVT_CONS', 'Consulta cerrada (pay-per-use)',     2),
(32, 'EVT_PAY',  'Pago recibido del paciente',        3),
(32, 'EVT_LAB',  'Resultado de laboratorio entregado',4),
(32, 'EVT_ADJ',  'Ajuste manual de crédito',          5),
(33, 'EVS_PEN',  'Pendiente de facturar',             1),
(33, 'EVS_BIL',  'Facturado',                         2),
(33, 'EVS_VOI',  'Anulado',                           3)
ON CONFLICT (catalog_id, code) DO NOTHING;

-- BLOQUE B: Tabla de eventos de facturación
CREATE TABLE IF NOT EXISTS billing.log_org_billing_event (
    id                 BIGSERIAL PRIMARY KEY,
    organization_id    BIGINT NOT NULL REFERENCES identity.mst_organization(id),
    event_type_id      INT NOT NULL REFERENCES common.mst_catalog_detail(id),
    reference_id       BIGINT,                      -- invoice_id, consultation_id, etc.
    amount             DECIMAL(18,2) NOT NULL DEFAULT 0,
    billing_year       INT NOT NULL,
    billing_month      INT NOT NULL CHECK (billing_month BETWEEN 1 AND 12),
    status_id          INT NOT NULL REFERENCES common.mst_catalog_detail(id),
    billed_invoice_id  BIGINT REFERENCES billing.tbl_subscription_invoice(id),
    trace_id           VARCHAR(100),
    created_at         TIMESTAMPTZ DEFAULT NOW(),
    created_by_user_id BIGINT
);

-- Índices críticos para el job mensual
CREATE INDEX IF NOT EXISTS idx_org_billing_event_period
    ON billing.log_org_billing_event(organization_id, billing_year, billing_month, status_id);
CREATE INDEX IF NOT EXISTS idx_org_billing_event_reference
    ON billing.log_org_billing_event(reference_id, event_type_id);

-- BLOQUE C: Nueva acción
INSERT INTO security.mst_action (id, module_id, name) VALUES
(3023, 3, 'Billing.EjecutarCierreMensual'),
(3024, 3, 'Billing.GetOrgBillingEvents')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- BLOQUE D: AppAction enum (actualizar en C#)
-- public const int EjecutarCierreMensual = 3023;
-- public const int GetOrgBillingEvents   = 3024;
```

---

## 4. Auditoría de Seguridad — SQL Injection y NoSQL

### 4.1 Veredicto general

El sistema está **mayormente protegido** gracias a Dapper con parámetros `@param`. Sin embargo existen **3 patrones de riesgo activo** que hay que corregir.

### 4.2 ✅ Lo que está bien protegido

| Área | Por qué está seguro |
|---|---|
| Login (`UserRepository.GetByUsernameAsync`) | `@username` parametrizado — no hay concatenación |
| `BaseRepository.GenerateInsertSql()` | Columnas vienen de `DbColumnAttribute` (reflexión sobre propiedades del tipo) — no de input del usuario |
| `BaseRepository.GenerateUpdateSql()` | Ídem — parámetros `@PropertyName` generados por reflexión |
| Dispatcher (`BaseDomain`) | `JsonElement` deserializado a DTO tipado antes de llegar a cualquier repo |
| Autenticación JWT | Claims extraídos del token firmado — no del body de la request |
| `PatientAccessLogRepository` | SQL con `@param` en todos los filtros |
| `InvoiceRepository.GetByOrganizationAsync` | Filtros dinámicos con `sql += " AND col = @param"` — los valores van como parámetros, no concatenados |

### 4.3 🔴 Riesgo 1 — `EnumExtensions.ToCatalogSql()` inyecta enteros en SQL

**Archivo:** `src/Domain/Extensions/EnumExtensions.cs:49`

```csharp
// ACTUAL — inyección de valor en SQL aunque sea un int:
public static string ToCatalogSql(this Enum enumValue, int catalogId)
{
    return $"common.get_catalog_id({catalogId}, {Convert.ToInt32(enumValue)})";
    //                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    //                              Ambos valores se inyectan directamente en el SQL
}
```

Aunque los enums son enteros y difícilmente explotables desde el exterior, si algún repositorio usa este método con un valor que venga del usuario (por ejemplo un `statusId` enviado por el cliente), el entero podría ser manipulado.

**Corrección:** Verificar que `ToCatalogSql` solo se use con valores de enum tipados en C#, nunca con `int` provenientes de DTOs. Agregar un guard:

```csharp
// Corrección preventiva — agregar validación del rango:
public static string ToCatalogSql(this Enum enumValue, int catalogId)
{
    var intVal = Convert.ToInt32(enumValue);
    // Los valores de enum C# son conocidos en compilación, esto es solo defensa en profundidad
    if (!Enum.IsDefined(enumValue.GetType(), enumValue))
        throw new ArgumentException($"Valor de enum no válido: {intVal}");
    return $"common.get_catalog_id({catalogId}, {intVal})";
}
```

### 4.4 🔴 Riesgo 2 — `_tableName` en BaseRepository sin validación

**Archivo:** `src/Infrastructure/Persistence/Struct/BaseRepository.cs`

```csharp
// ACTUAL — _tableName se inyecta en SQL sin sanitizar:
return $"INSERT INTO {_tableName} ({columns}) VALUES ({values}) RETURNING id;";
return $"UPDATE {_tableName} SET {sets} WHERE id = @Id";
```

`_tableName` se pasa en el constructor de cada repositorio concreto. En el código actual siempre es un literal como `"billing.mst_invoice"`. Pero si en el futuro alguien crea un repositorio genérico donde `tableName` venga de configuración o de un parámetro externo, hay riesgo.

**Corrección:** Agregar whitelist de nombres de tabla válidos:

```csharp
private static readonly FrozenSet<string> _allowedTableNames = new HashSet<string>
{
    "billing.mst_invoice", "billing.mst_payments", "billing.tbl_subscription",
    "clinical.mst_patient", "clinical.mst_patient_alert", "clinical.mst_patient_contact",
    "care.mst_consultation", "care.mst_prescription", "care.mst_appointment",
    "identity.mst_user", "identity.mst_person", "identity.mst_organization",
    "laboratory.mst_lab_order", "laboratory.det_lab_result",
    // ... todos los nombres válidos
}.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

protected BaseRepository(IDbConnection connection, string tableName, ...)
{
    if (!_allowedTableNames.Contains(tableName))
        throw new InvalidOperationException($"Nombre de tabla no permitido: {tableName}");
    _tableName = tableName;
    // ...
}
```

### 4.5 🟡 Riesgo 3 — `InvoiceRepository.SearchAsync` construye SQL dinámico con string append

**Archivo:** `src/Infrastructure/Persistence/Repositories/Billing/InvoiceRepository.cs`

```csharp
// ACTUAL — safe en valores pero comentario con dudas en producción:
if (!string.IsNullOrEmpty(invoiceNumber))
    sql += " AND i.invoice_number ILIKE @invoiceNumber";
//  ↑ Los valores siempre van como @param — esto ESTÁ bien ✅

// PROBLEMA: el comentario en el JOIN:
LEFT JOIN common.mst_catalog_detail cd ON i.status_id = cd.id AND cd.catalog_id = 1
-- Assuming 1 is InvoiceStatus catalog... Wait, if there's no catalog...
// ↑ Este código nunca debió llegar a producción
```

El append de condiciones es seguro porque los valores van parametrizados. Pero hay que limpiar el comentario con dudas y corregir el catalog_id:

```csharp
// Corrección:
// catalog_id = 18 es INVOICE_STATUS según seeds
LEFT JOIN common.mst_catalog_detail cd ON i.status_id = cd.id AND cd.catalog_id = 18
// Eliminar el comentario de dudas
```

### 4.6 ⚠️ Riesgo 4 — Deserialización JSON sin límite de profundidad

**Archivo:** `src/Application/Common/Dispatcher/BaseDomain.cs` + `DomainReflectionHelper.cs`

```csharp
// ACTUAL — sin MaxDepth configurado:
dto = DomainReflectionHelper.DeserializeDto(dtoType, data);
// JsonElement.GetRawText() → JsonSerializer.Deserialize<T>(json, options)
```

Un atacante podría enviar un JSON con anidamiento extremo (JSON Bomb) para causar stack overflow en el deserializador.

**Corrección en `DomainReflectionHelper` o en la configuración de JsonOptions:**
```csharp
private static readonly JsonSerializerOptions _safeOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    MaxDepth = 10, // AGREGAR — previene JSON bombs
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
```

### 4.7 ⚠️ Riesgo 5 — `Console.WriteLine` con SQL en BaseRepository (data leakage)

**Archivo:** `src/Infrastructure/Persistence/Struct/BaseRepository.cs:198`

```csharp
// ACTUAL — expone SQL completo en logs de producción:
Console.WriteLine($"[DEBUG-SQL] Executing Insert: {sql}");
Console.WriteLine($"[DEBUG-SQL] Executing Update: {sql}");
```

En producción (Lambda) los logs de CloudWatch son accesibles y el SQL expuesto puede revelar estructura de la base de datos.

**Corrección:**
```csharp
// Reemplazar Console.WriteLine por logger condicional:
#if DEBUG
    _logger.LogDebug("[SQL] {Operation}: {Sql}", "Insert", sql);
#endif
// O eliminar completamente en producción:
// if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
//     _logger.LogDebug(...);
```

### 4.8 ✅ NoSQL / JSON — evaluación

El sistema no usa una base de datos NoSQL. Los campos JSONB de PostgreSQL se manejan con `JsonElement` en C# y se insertan vía Dapper con `::jsonb` cast. Esto es seguro porque:
- El contenido del JSONB nunca se ejecuta como SQL
- Dapper lo trata como string opaco
- PostgreSQL valida que sea JSON válido antes de almacenarlo

**Sin riesgo de NoSQL injection** en la arquitectura actual.

---

## 5. Resumen de gaps pendientes — orden de ataque

### Semana 1 — Tests y seguridad (sin features nuevas)
```
□ Crear los 8 fakers faltantes (§1.2)
□ Corregir ruta hardcodeada en ReportingServiceTests (2 tests)
□ Implementar SecurityDataIsolationTests con inyección SQL (§2.2)
□ Agregar MaxDepth en JsonOptions (§4.6) — 1 línea
□ Agregar whitelist de tableName en BaseRepository (§4.4)
□ Limpiar Console.WriteLine en BaseRepository (§4.7)
□ Fijar catalog_id = 18 en SearchAsync (§4.5)
```

### Semana 2 — Tests de módulos sin cobertura
```
□ PatientAllergyServiceTests (8 tests)
□ DashboardServiceTests (5 tests)
□ PasswordManagementTests (6 tests)
□ BillingEventTests (9 tests — depende de Semana 3)
□ LabTemplateWorkflowTests (8 tests)
```

### Semana 3 — Pipeline de billing por eventos
```
□ Crear OrgBillingEvent entidad + enum BillingEventType + BillingEventStatus
□ Crear IOrgBillingEventRepository + implementación
□ Registrar en IUnitOfWork
□ Crear OrgBillingEventOutputAction
□ Crear EjecutarCierreMensualAsync en IBillingService + implementación
□ Crear MonthlyBillingJob (Lambda handler)
□ Ejecutar 025_Org_Billing_Events.sql
□ Agregar constantes AppAction.EjecutarCierreMensual = 3023 / GetOrgBillingEvents = 3024
□ Registrar handlers en BillingDomain
□ Registrar DTOs nuevos en BillingJsonContext
```

### Semana 4 — Tests de amplificación
```
□ Ampliar BillingServiceTests de 10 → 20 (§2.2)
□ Ampliar AppointmentServiceTests de 2 → 8 (§2.2)
□ Ampliar MedicalCareServiceTests de 4 → 12 (§2.2)
□ Ampliar SecurityDataIsolationTests de 1 → 6 (§2.2)
□ Tests de Reporting: 2 → 6 (§2.3)
□ Integrar fakers nuevos al MasterSeeder
```

---

## 6. Tabla final — fakers vs tests completos

| Módulo | Faker | Tests actuales | Tests objetivo | Faltantes |
|---|---|---|---|---|
| Identity | ✅ 4 fakers | 4 | 10 | 6 |
| Clinical base | ✅ 3 fakers | 3 | 8 | 5 |
| **Alergias** | ❌ Crear | 0 | 8 | **8** |
| Alertas | ✅ FakerPatientAlert | 5 | 8 | 3 |
| Contactos | ✅ FakerPatientContact | 4 | 6 | 2 |
| **Dashboard** | ❌ Crear | 0 | 5 | **5** |
| Care (consulta/receta) | ✅ 4 fakers | 4 | 12 | 8 |
| **Citas (escenarios neg.)** | ⚠️ Parcial | 2 | 8 | 6 |
| Laboratory base | ✅ 3 fakers | 8 | 12 | 4 |
| **Lab results items** | ❌ Crear | 0 | 8 | **8** |
| Billing (facturas) | ✅ 3 fakers | 10 | 20 | 10 |
| **Billing events** | ❌ Crear | 0 | 9 | **9** |
| Subscriptions | ✅ 2 fakers | 2 | 6 | 4 |
| Reporting | — | 2 | 6 | 4 |
| Security | ✅ 1 faker | 1 | 6 | 5 |
| **Password reset** | ❌ Crear | 0 | 6 | **6** |
| **Clinical notes** | ❌ Crear | 0 | 4 | **4** |
| **Schedule exceptions** | ❌ Crear | 0 | 4 | **4** |
| **TOTAL** | 20/28 fakers | **43** | **120** | **77** |

---

*Fin del documento — MedfarLabs Core · Gaps completos Mayo 2025*
