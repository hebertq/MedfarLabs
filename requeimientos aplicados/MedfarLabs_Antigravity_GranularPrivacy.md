# MedfarLabs · Core — Instrucciones Antigravity
## Módulo: Privacidad Granular por Organización (Expediente Único Compartido)
> Modelo: campo público (inter-org) vs campo privado (intra-org) por tipo de actor  
> Mayo 2025

---

## 0. Diagnóstico — estado actual del Core

### Lo que ya existe y funciona

El Core **ya tiene la base correcta** para este modelo. El SQL en `GetFullPatientRecordAsync` implementa el patrón `IsOwner` con un `CASE WHEN l.organization_id = @OrganizationId`:

```sql
-- YA EXISTE en PatientRepository.cs — el Core ya sabe quién es dueño:
CASE WHEN l.organization_id = @OrganizationId THEN
    -- Resumen completo: diagnóstico + notas SOAP
    COALESCE(analysis_data, plan_data, ..., 'Sin resumen emitido')
ELSE
    -- Solo el diagnóstico CIE-10
    COALESCE(diagnosis_code, 'Evaluación confidencial (Visible solo para clínica emisora)')
END as summary
```

Esto significa que el **principio de privacidad ya está implementado a nivel SQL**. Lo que falta es:

1. **Formalizarlo** con un modelo de datos claro (`access_level` en la consulta)
2. **Extenderlo** a más campos (el SQL actual solo protege `summary`, no `subjective/objective/analysis/plan` por separado)
3. **Bloquearlo por tipo de organización** (laboratorios y farmacias no deben ver expedientes)
4. **Exponerlo en el DTO** de forma estructurada para que la PWA sepa qué renderizar
5. **Auditarlo** con `log_patient_access` (ya existe la tabla, falta dispararlo siempre)

### Gaps críticos encontrados

| Gap | Impacto | Prioridad |
|---|---|---|
| `care.mst_consultation` no tiene `organization_id` | No se puede saber a qué org pertenece la nota sin join a `log_consultation_ledger` | 🔴 Crítico |
| `IUserContext` no expone `OrganizationTypeId` | No se puede bloquear laboratorios a nivel del dispatcher | 🔴 Crítico |
| `ConsultationHistoryDTO` no tiene `AccessLevel` | La PWA no puede saber qué campos mostrar | 🟡 Alto |
| `GetPatientRecord` sin bloqueo por `TipoOrganizacion` | Un usuario de farmacia podría ver expedientes si tiene el permiso asignado | 🔴 Crítico |
| `log_patient_access` no se dispara en `GetPatientRecord` actual | La auditoría de acceso cross-org no se está registrando | 🟡 Alto |
| No existe `[ClinicalPrivacyFilter]` como atributo reutilizable | La lógica de limpieza de campos está hardcodeada en SQL, no en la capa de aplicación | 🟢 Normal |

---

## 1. Modelo de datos — campos públicos vs privados

### 1.1 Clasificación de campos por nivel de acceso

| Campo | Tabla | Nivel | Visible para |
|---|---|---|---|
| Alergias (`mst_allergy.substance`) | Clinical | **Público** | Todos los médicos autorizados |
| Diagnóstico CIE-10 (`det_consultation_diagnosis.code`) | Care | **Público** | Todos los médicos autorizados |
| Medicamentos activos (`det_prescription_item`) | Care | **Público** | Todos los médicos autorizados |
| Antecedentes (`mst_patient_antecedent`) | Clinical | **Público** | Todos los médicos autorizados |
| Signos vitales históricos | Clinical | **Público** | Todos los médicos autorizados |
| Consentimientos del paciente | Clinical | **Público** | Solo org propietaria + paciente |
| `subjective_data` (Motivo de consulta) | Care | **Privado** | Solo org propietaria de la nota |
| `objective_data` (Examen físico) | Care | **Privado** | Solo org propietaria de la nota |
| `analysis_data` (Evaluación/Dx presuntivo) | Care | **Privado** | Solo org propietaria de la nota |
| `plan_data` (Plan terapéutico) | Care | **Privado** | Solo org propietaria de la nota |
| Notas de evolución completas | Care | **Privado** | Solo org propietaria de la nota |

### 1.2 Reglas por tipo de organización

| TipoOrganizacion | Puede ver expediente | Campos accesibles |
|---|---|---|
| `CLINICA (1)` | ✅ Sí | Públicos + Privados (solo de sus propias notas) |
| `MEDICO_INDEPENDIENTE (3)` | ✅ Sí | Públicos + Privados (solo de sus propias notas) |
| `CLINICA_ODONTOLOGICA (5)` | ✅ Sí (si el paciente consintió) | Públicos + Privados (solo de sus propias notas) |
| `LABORATORIO (2)` | ❌ Bloqueado | **Cero acceso** — ni datos públicos del expediente clínico |
| `FARMACIA (4)` | ❌ Bloqueado | **Cero acceso** — puede ver recetas activas pero no expediente |

---

## 2. Cambios en base de datos — `030_Granular_Privacy.sql`

**Archivo:** `src/Migrations/Scripts/030_Granular_Privacy.sql`

```sql
-- ============================================================
-- BLOQUE A: Agregar organization_id a care.mst_consultation
-- Esto elimina la dependencia de log_consultation_ledger para saber el propietario
-- ============================================================
ALTER TABLE care.mst_consultation
    ADD COLUMN IF NOT EXISTS organization_id BIGINT REFERENCES identity.mst_organization(id);

-- Rellenar datos históricos desde el ledger
UPDATE care.mst_consultation c
SET organization_id = l.organization_id
FROM billing.log_consultation_ledger l
WHERE l.consultation_id = c.id
  AND c.organization_id IS NULL;

-- Índice para la consulta de privacidad (la más frecuente del sistema)
CREATE INDEX IF NOT EXISTS idx_consultation_org_privacy
    ON care.mst_consultation(organization_id, id)
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_consultation_record_org
    ON care.mst_consultation(medical_record_id, organization_id);

-- ============================================================
-- BLOQUE B: Catálogo de niveles de acceso
-- ============================================================
INSERT INTO common.mst_catalog (id, name, description) VALUES
(35, 'CLINICAL_ACCESS_LEVEL', 'Nivel de acceso a datos clínicos del expediente')
ON CONFLICT (id) DO NOTHING;

INSERT INTO common.mst_catalog_detail (catalog_id, code, name, enum_mapping) VALUES
(35, 'ACC_OWNER',    'Propietario — acceso completo',                      1),
(35, 'ACC_SHARED',   'Compartido — solo datos públicos inter-org',          2),
(35, 'ACC_BLOCKED',  'Bloqueado — tipo de organización sin acceso clínico', 3)
ON CONFLICT (catalog_id, code) DO NOTHING;

-- ============================================================
-- BLOQUE C: Nueva acción para acceso cross-org al expediente
-- ============================================================
INSERT INTO security.mst_action (id, module_id, name) VALUES
(4127, 4, 'Clinical.GetSharedPatientRecord'),
(4128, 4, 'Clinical.RequestPatientRecordAccess')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- GetSharedPatientRecord: solo médicos (NO laboratorios, NO farmacias)
-- Asignar a roles clínicos SOLAMENTE
INSERT INTO security.map_role_action (role_id, action_id)
SELECT r.id, 4127
FROM security.mst_role r
WHERE r.name IN ('Admin-Clinical', 'Doctor', 'Medico-Independiente', 'Admin-Odontologia')
ON CONFLICT DO NOTHING;

-- ============================================================
-- BLOQUE D: Tabla de solicitudes de acceso cross-org (consentimiento del paciente)
-- ============================================================
CREATE TABLE IF NOT EXISTS clinical.mst_cross_org_access_request (
    id                   BIGSERIAL PRIMARY KEY,
    patient_id           BIGINT NOT NULL REFERENCES clinical.mst_patient(id),
    requesting_org_id    BIGINT NOT NULL REFERENCES identity.mst_organization(id),
    owning_org_id        BIGINT NOT NULL REFERENCES identity.mst_organization(id),
    requested_by_user_id BIGINT NOT NULL REFERENCES identity.mst_user(id),
    status_id            INT NOT NULL REFERENCES common.mst_catalog_detail(id), -- PENDING/APPROVED/DENIED/REVOKED
    reason               TEXT,
    approved_by_user_id  BIGINT REFERENCES identity.mst_user(id),
    approved_at          TIMESTAMPTZ,
    expires_at           TIMESTAMPTZ,                -- Acceso temporal (opcional)
    is_active            BOOLEAN DEFAULT TRUE,
    created_at           TIMESTAMPTZ DEFAULT NOW(),
    row_version          INT DEFAULT 1
);

CREATE INDEX IF NOT EXISTS idx_cross_org_access_patient
    ON clinical.mst_cross_org_access_request(patient_id, requesting_org_id, status_id)
    WHERE is_active = TRUE;

-- ============================================================
-- BLOQUE E: Catálogo de estado de solicitud de acceso
-- ============================================================
INSERT INTO common.mst_catalog (id, name, description) VALUES
(36, 'CROSS_ORG_ACCESS_STATUS', 'Estado de solicitud de acceso inter-organización')
ON CONFLICT (id) DO NOTHING;

INSERT INTO common.mst_catalog_detail (catalog_id, code, name, enum_mapping) VALUES
(36, 'CAS_PENDING',  'Pendiente de aprobación', 1),
(36, 'CAS_APPROVED', 'Aprobado',                2),
(36, 'CAS_DENIED',   'Denegado',                3),
(36, 'CAS_REVOKED',  'Revocado',                4)
ON CONFLICT (catalog_id, code) DO NOTHING;
```

---

## 3. Cambios en el dominio C#

### 3.1 Extender `IUserContext` con `OrganizationTypeId`

**Archivo:** `src/Domain/Interfaces/Security/IUserContext.cs`

```csharp
public interface IUserContext
{
    long UserId { get; set; }
    long OrganizationId { get; set; }
    long BranchId { get; set; }

    // AGREGAR: tipo de organización del usuario (cargado al inicio del request)
    int OrganizationTypeId { get; set; }   // TipoOrganizacion enum int

    Task<bool> HasPermissionAsync(int actionId);

    // AGREGAR: helper de conveniencia
    bool IsLaboratory => OrganizationTypeId == (int)TipoOrganizacion.LABORATORIO;
    bool IsPharmacy   => OrganizationTypeId == (int)TipoOrganizacion.FARMACIA;
    bool IsClinical   => OrganizationTypeId is (int)TipoOrganizacion.CLINICA
                                              or (int)TipoOrganizacion.MEDICO_INDEPENDIENTE
                                              or (int)TipoOrganizacion.CLINICA_ODONTOLOGICA;
}
```

**Archivo:** `src/Infrastructure/Shared/Security/UserContext.cs`

```csharp
public class UserContext : IUserContext
{
    public long UserId { get; set; }
    public long OrganizationId { get; set; }
    public long BranchId { get; set; }
    public int OrganizationTypeId { get; set; }  // AGREGAR
    // ... resto igual
}
```

**Archivo:** `src/Application/Common/Dispatcher/BaseDomain.cs` — donde se inyecta el contexto:

```csharp
// Donde se setea el contexto al inicio de ExecuteAsync:
userContext.UserId       = claims.UserId;
userContext.OrganizationId = claims.OrganizationId;
userContext.BranchId     = claims.BranchId;
userContext.OrganizationTypeId = claims.OrganizationTypeId;  // AGREGAR
```

### 3.2 Nuevo enum `ClinicalAccessLevel`

**Archivo:** `src/Domain/Enums/Clinical/ClinicalAccessLevel.cs`

```csharp
namespace MedfarLabs.Core.Domain.Enums.Clinical
{
    public enum ClinicalAccessLevel
    {
        /// <summary>
        /// El usuario pertenece a la org que creó la nota.
        /// Ve todos los campos: SOAP completo, diagnóstico presuntivo, examen físico.
        /// </summary>
        Owner = 1,

        /// <summary>
        /// El usuario es de otra org clínica autorizada.
        /// Solo ve: diagnóstico CIE-10, alergias, medicamentos activos, antecedentes.
        /// Los campos privados vienen como null.
        /// </summary>
        SharedReadOnly = 2,

        /// <summary>
        /// El usuario es de laboratorio, farmacia u org sin permiso clínico.
        /// El expediente clínico está completamente bloqueado.
        /// </summary>
        Blocked = 3
    }
}
```

### 3.3 Nuevo enum `CrossOrgAccessStatus`

**Archivo:** `src/Domain/Enums/Clinical/CrossOrgAccessStatus.cs`

```csharp
namespace MedfarLabs.Core.Domain.Enums.Clinical
{
    public enum CrossOrgAccessStatus
    {
        Pending  = 1,
        Approved = 2,
        Denied   = 3,
        Revoked  = 4
    }
}
```

### 3.4 Nuevo atributo `[ClinicalPrivacyFilter]`

**Archivo:** `src/Domain/Common/Attributes/ClinicalPrivacyFilterAttribute.cs`

```csharp
namespace MedfarLabs.Core.Domain.Common.Attributes
{
    /// <summary>
    /// Marca una propiedad del DTO de respuesta como campo privado clínico.
    /// El ClinicalPrivacyService aplica null a estos campos si el acceso es SharedReadOnly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ClinicalPrivacyFilterAttribute : Attribute
    {
        /// <summary>
        /// Nivel mínimo de acceso requerido para ver este campo.
        /// Default: Owner (solo la org propietaria puede verlo).
        /// </summary>
        public ClinicalAccessLevel RequiredLevel { get; }

        public ClinicalPrivacyFilterAttribute(ClinicalAccessLevel requiredLevel = ClinicalAccessLevel.Owner)
        {
            RequiredLevel = requiredLevel;
        }
    }
}
```

### 3.5 Extender `ConsultationHistoryDTO` con campos de privacidad

**Archivo:** `src/Application/Features/Clinical/Dtos/Response/PatientRecordResponseDTO.cs`

```csharp
public class ConsultationHistoryDTO
{
    public long ConsultationId { get; set; }
    public long DoctorUserId { get; set; }
    public int StatusId { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }

    // NUEVO: nivel de acceso que tiene el usuario sobre esta nota
    public int AccessLevel { get; set; }  // ClinicalAccessLevel enum

    // Campos PÚBLICOS — visibles para todos los médicos autorizados
    public string? DiagnosisCode { get; set; }      // CIE-10
    public string? DiagnosisDescription { get; set; }

    // Campos PRIVADOS — solo visibles para la org propietaria
    // null si AccessLevel == SharedReadOnly
    [ClinicalPrivacyFilter(ClinicalAccessLevel.Owner)]
    public string? SubjectiveData { get; set; }     // Motivo de consulta (S)

    [ClinicalPrivacyFilter(ClinicalAccessLevel.Owner)]
    public string? ObjectiveData { get; set; }      // Examen físico (O)

    [ClinicalPrivacyFilter(ClinicalAccessLevel.Owner)]
    public string? AnalysisData { get; set; }       // Evaluación/Dx presuntivo (A)

    [ClinicalPrivacyFilter(ClinicalAccessLevel.Owner)]
    public string? PlanData { get; set; }           // Plan terapéutico (P)

    // Mensaje de UI cuando el acceso es SharedReadOnly
    public string? PrivacyMessage { get; set; }
    // Ej: "Nota clínica de Clínica XYZ — Solo diagnóstico visible"
}
```

### 3.6 Nuevo servicio `IClinicalPrivacyService`

**Archivo:** `src/Application/Features/Clinical/Interfaces/IClinicalPrivacyService.cs`

```csharp
public interface IClinicalPrivacyService
{
    /// <summary>
    /// Determina el nivel de acceso que tiene un usuario sobre un paciente/consulta.
    /// Owner → misma organización que creó la nota.
    /// SharedReadOnly → médico de otra org clínica autorizada.
    /// Blocked → laboratorio, farmacia u org sin permiso clínico.
    /// </summary>
    ClinicalAccessLevel GetAccessLevel(long requesterOrgId, int requesterOrgType, long noteOwnerOrgId);

    /// <summary>
    /// Aplica el filtro de privacidad sobre una lista de ConsultationHistoryDTO.
    /// Anula los campos [ClinicalPrivacyFilter] si el acceso es SharedReadOnly.
    /// </summary>
    void ApplyPrivacyFilter(IEnumerable<ConsultationHistoryDTO> consultations, ClinicalAccessLevel level);
}
```

**Archivo:** `src/Application/Features/Clinical/Services/ClinicalPrivacyService.cs`

```csharp
[RegisterScoped]
public class ClinicalPrivacyService : IClinicalPrivacyService
{
    private static readonly FrozenSet<int> _blockedOrgTypes = new[]
    {
        (int)TipoOrganizacion.LABORATORIO,
        (int)TipoOrganizacion.FARMACIA
    }.ToFrozenSet();

    public ClinicalAccessLevel GetAccessLevel(long requesterOrgId, int requesterOrgType, long noteOwnerOrgId)
    {
        // Regla 1: Laboratorios y farmacias — bloqueados sin excepción
        if (_blockedOrgTypes.Contains(requesterOrgType))
            return ClinicalAccessLevel.Blocked;

        // Regla 2: Misma organización — acceso completo
        if (requesterOrgId == noteOwnerOrgId)
            return ClinicalAccessLevel.Owner;

        // Regla 3: Otra organización clínica — solo datos públicos
        return ClinicalAccessLevel.SharedReadOnly;
    }

    public void ApplyPrivacyFilter(IEnumerable<ConsultationHistoryDTO> consultations, ClinicalAccessLevel level)
    {
        if (level == ClinicalAccessLevel.Owner) return; // Sin filtro para propietarios

        foreach (var c in consultations)
        {
            if (c.AccessLevel == (int)ClinicalAccessLevel.Owner) continue;

            // Limpiar campos privados
            c.SubjectiveData = null;
            c.ObjectiveData = null;
            c.AnalysisData = null;
            c.PlanData = null;

            if (level == ClinicalAccessLevel.Blocked)
            {
                // Para orgs bloqueadas, también limpiar datos públicos de la nota
                c.DiagnosisCode = null;
                c.DiagnosisDescription = null;
                c.PrivacyMessage = "Acceso restringido para este tipo de organización.";
            }
            else
            {
                // SharedReadOnly: mantener diagnóstico CIE-10, limpiar SOAP
                c.PrivacyMessage = $"Nota privada — Solo diagnóstico visible (org emisora: {c.DoctorName})";
            }
        }
    }
}
```

---

## 4. Cambios en `ClinicalService.GetPatientRecordAsync`

**Archivo:** `src/Application/Features/Clinical/Services/ClinicalService.cs`

### 4.1 Inyectar el nuevo servicio y el UserContext

```csharp
public class ClinicalService : BaseService, IClinicalService
{
    private readonly IIdentityService _identityService;
    private readonly IMedicalCareService _medicalCareService;
    private readonly IClinicalPrivacyService _privacyService;  // AGREGAR
    private readonly IUserContext _userContext;                 // AGREGAR

    public ClinicalService(
        IUnitOfWork uow,
        IApplicationDispatcher dispatcher,
        IIdentityService identityService,
        IMedicalCareService medicalCareService,
        IClinicalPrivacyService privacyService,               // AGREGAR
        IUserContext userContext)                              // AGREGAR
        : base(uow, dispatcher)
    {
        _identityService = identityService;
        _medicalCareService = medicalCareService;
        _privacyService = privacyService;
        _userContext = userContext;
    }
```

### 4.2 Reemplazar `GetPatientRecordAsync` completo

```csharp
public async Task<BaseResponse<PatientRecordResponseDTO>> GetPatientRecordAsync(PatientRecordRequestDTO request)
{
    // PASO 1: Verificar bloqueo por tipo de organización
    // Laboratorios y farmacias no pueden ver expedientes clínicos
    if (_userContext.IsLaboratory || _userContext.IsPharmacy)
    {
        return BaseResponse<PatientRecordResponseDTO>.Failure(
            "Acceso denegado. El tipo de organización no tiene permisos para consultar expedientes clínicos.");
    }

    // PASO 2: Cargar datos con el nuevo SQL enriquecido (ver §5)
    var limit = await _unitOfWork.Patients.GetRecordHistoryLimitAsync(request.OrganizationId);
    var rawData = await _unitOfWork.Patients.GetFullPatientRecordAsync(
        request.PatientId, limit, request.OrganizationId);

    if (rawData.Demographics == null)
        return BaseResponse<PatientRecordResponseDTO>.Failure("Paciente no encontrado.");

    var demo = (DemographicsRow)rawData.Demographics;

    // PASO 3: Construir DTO base (datos siempre visibles)
    var dto = new PatientRecordResponseDTO
    {
        PatientId  = demo.PatientId,
        FullName   = demo.FullName,
        Identifier = demo.Identifier ?? "N/A",
        Age        = Convert.ToInt32(demo.Age),
        Gender     = demo.GenderId == 1 ? "Masculino" : demo.GenderId == 2 ? "Femenino" : "Otro",
        BloodType  = demo.BloodType ?? "N/D",
        Allergies  = demo.Allergies != null
            ? new List<string>(demo.Allergies.Split(',', StringSplitOptions.RemoveEmptyEntries))
            : new List<string>()
    };

    // PASO 4: Mapear consultas con privacidad granular por nota
    foreach (ConsultationRow c in rawData.Consultations)
    {
        // El SQL ahora devuelve NoteOwnerOrgId por cada consulta (ver §5)
        var noteOwnerOrgId = c.NoteOwnerOrgId; // NUEVO campo en ConsultationRow

        var accessLevel = _privacyService.GetAccessLevel(
            request.OrganizationId,
            _userContext.OrganizationTypeId,
            noteOwnerOrgId);

        var consultDto = new ConsultationHistoryDTO
        {
            ConsultationId      = c.ConsultationId,
            DoctorUserId        = c.doctoruserid != null ? Convert.ToInt64(c.doctoruserid) : 0,
            StatusId            = c.statusid != null ? Convert.ToInt32(c.statusid) : 1,
            Date                = c.Date,
            Title               = c.title ?? "Consulta General",
            DoctorName          = c.doctorname ?? "Médico",
            IsOwner             = accessLevel == ClinicalAccessLevel.Owner,
            AccessLevel         = (int)accessLevel,
            DiagnosisCode       = c.DiagnosisCode,      // NUEVO campo en ConsultationRow
            DiagnosisDescription = c.DiagnosisDescription, // NUEVO campo

            // Campos privados: solo se llenan si es Owner
            SubjectiveData = accessLevel == ClinicalAccessLevel.Owner ? c.SubjectiveData : null,
            ObjectiveData  = accessLevel == ClinicalAccessLevel.Owner ? c.ObjectiveData  : null,
            AnalysisData   = accessLevel == ClinicalAccessLevel.Owner ? c.AnalysisData   : null,
            PlanData       = accessLevel == ClinicalAccessLevel.Owner ? c.PlanData       : null,

            PrivacyMessage = accessLevel == ClinicalAccessLevel.SharedReadOnly
                ? $"Nota clínica privada — Diagnóstico disponible, detalle clínico visible solo para la clínica emisora."
                : null
        };

        dto.Consultations.Add(consultDto);
    }

    // PASO 5: Mapear antecedentes, prescripciones, consentimientos (sin cambios)
    // ... (código existente sin cambios)

    // PASO 6: Vitales (sin cambios)
    // ... (código existente sin cambios)

    return BaseResponse<PatientRecordResponseDTO>.Success(dto);
}
```

---

## 5. Cambios en el repositorio SQL

### 5.1 Actualizar `ConsultationRow` con nuevos campos

**Archivo:** `src/Domain/Interfaces/Repositories/Clinical/IPatientRepository.cs`

```csharp
public class ConsultationRow
{
    public long ConsultationId { get; set; }
    public DateTime Date { get; set; }
    public string? title { get; set; }
    public string? summary { get; set; }
    public string? doctorname { get; set; }
    public long? doctoruserid { get; set; }
    public int? statusid { get; set; }
    public bool IsOwner { get; set; }

    // NUEVOS campos para privacidad granular:
    public long NoteOwnerOrgId { get; set; }       // org que creó esta nota
    public string? DiagnosisCode { get; set; }      // CIE-10 (público)
    public string? DiagnosisDescription { get; set; } // (público)
    public string? SubjectiveData { get; set; }    // S — privado
    public string? ObjectiveData { get; set; }     // O — privado
    public string? AnalysisData { get; set; }      // A — privado
    public string? PlanData { get; set; }          // P — privado
}
```

### 5.2 Reemplazar la query de consultas en `GetFullPatientRecordAsync`

**Archivo:** `src/Infrastructure/Persistence/Repositories/Clinical/PatientRepository.cs`

Reemplazar el bloque `-- 4. Recent Consultations` con:

```sql
-- 4. Consultas con privacidad granular por nota
SELECT
    c.id                AS ConsultationId,
    c.created_at        AS Date,
    'Consulta Médica'   AS title,
    c.organization_id   AS NoteOwnerOrgId,

    -- Diagnóstico CIE-10 siempre visible (campo público)
    dc.code             AS DiagnosisCode,
    dc.description      AS DiagnosisDescription,

    -- Campos PRIVADOS: se envían siempre al servicio,
    -- el ClinicalPrivacyService decide si null-earlos
    c.subjective_data   AS SubjectiveData,
    c.objective_data    AS ObjectiveData,
    c.analysis_data     AS AnalysisData,
    c.plan_data         AS PlanData,

    -- Summary compuesto (para compatibilidad con PWA actual)
    CASE
        WHEN c.organization_id = @OrganizationId THEN
            COALESCE(
                NULLIF(TRIM(c.analysis_data), ''),
                NULLIF(TRIM(c.plan_data), ''),
                NULLIF(TRIM(c.objective_data), ''),
                NULLIF(TRIM(c.subjective_data), ''),
                'Sin resumen emitido'
            )
        ELSE
            COALESCE(dc.code || ' — ' || dc.description,
                     'Evaluación clínica — detalle disponible solo para clínica emisora')
    END                 AS summary,

    u.username          AS doctorname,
    c.doctor_user_id    AS doctoruserid,
    c.is_active         AS statusid,
    (c.organization_id = @OrganizationId) AS IsOwner

FROM care.mst_consultation c
INNER JOIN clinical.mst_medical_record mr ON c.medical_record_id = mr.id
LEFT JOIN identity.mst_user u ON c.doctor_user_id = u.id
LEFT JOIN LATERAL (
    SELECT mc.code, mc.description
    FROM care.det_consultation_diagnosis d
    INNER JOIN clinical.mst_diagnosis_code mc ON d.diagnosis_id = mc.id
    WHERE d.consultation_id = c.id
    ORDER BY d.id ASC LIMIT 1
) dc ON TRUE
WHERE mr.patient_id = @PatientId
  AND c.is_active = TRUE
ORDER BY c.created_at DESC
LIMIT @Limit;
```

> **Nota importante:** El SQL ahora retorna `SubjectiveData`, `ObjectiveData`, `AnalysisData` y `PlanData` **siempre** desde la DB. Es el `ClinicalPrivacyService` en la capa de aplicación quien los convierte a `null` según el nivel de acceso. Esto es correcto por diseño — la privacidad se aplica en la capa de aplicación, no en SQL, para poder auditarla.

---

## 6. Auditoría automática de acceso cross-org

### 6.1 Nuevo `CrossOrgAccessAuditOutputAction`

**Archivo:** `src/Application/Features/Clinical/OutputActions/CrossOrgAccessAuditOutputAction.cs`

```csharp
[RegisterScoped(ServiceType = typeof(IOutputAction))]
public class CrossOrgAccessAuditOutputAction : IOutputAction
{
    private readonly IPatientAccessLogRepository _accessLogRepo;
    private readonly ILogger<CrossOrgAccessAuditOutputAction> _logger;

    public bool ShouldExecute(OutputContextDto context) =>
        context.Response.IsSuccess &&
        context.ActionId == AppAction.Clinical.GetPatientRecord;

    public async Task ExecuteAsync(OutputContextDto context)
    {
        try
        {
            // Extraer patientId del payload de la request
            if (!context.RawInput.TryGetProperty("patient_id", out var pidElement)) return;
            var patientId = pidElement.GetInt64();

            // Registrar siempre en log_patient_access
            await _accessLogRepo.LogAccessAsync(new PatientAccessLogEntity
            {
                PatientId      = patientId,
                UserId         = context.UserContext.UserId,
                AccessTypeId   = 1,  // VIEW — catalog PATIENT_ACCESS_TYPE
                ResourceTypeId = 1,  // MEDICAL_RECORD
                ResourceId     = patientId,
                OrganizationId = context.UserContext.OrganizationId,
                TraceId        = context.TraceId,
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "Error registrando auditoría de acceso clínico. TraceId: {TraceId}", context.TraceId);
        }
    }
}
```

---

## 7. Nuevas constantes y acciones

**Archivo:** `src/Domain/Const/AppAction.Clinical.cs`

```csharp
// AGREGAR al bloque Clinical:
public const int GetSharedPatientRecord      = 4127; // Acceso cross-org al expediente
public const int RequestRecordAccess         = 4128; // Solicitar acceso a expediente de otra org
public const int ApproveRecordAccessRequest  = 4129; // Aprobar solicitud de acceso
public const int RevokeRecordAccess          = 4130; // Revocar acceso previamente concedido
public const int GetRecordAccessRequests     = 4131; // Listar solicitudes de acceso
```

---

## 8. Nuevos DTOs

**Archivo:** `src/Application/Features/Clinical/Dtos/Request/CrossOrgAccessRequestDTO.cs`

```csharp
// Solicitar acceso a expediente de otra organización
[ActionMapping(AppModule.Clinical, AppAction.Clinical.RequestRecordAccess)]
public record RequestRecordAccessRequestDTO : IHasOrganization, IHasUser
{
    [JsonIgnore] public long OrganizationId { get; set; }
    [JsonIgnore] public long UserId { get; set; }

    [JsonPropertyName("patient_id")]  public long PatientId { get; init; }
    [JsonPropertyName("reason")]      public string Reason { get; init; } = string.Empty;
    // expires_in_days: 0 = acceso permanente (hasta que se revoque)
    [JsonPropertyName("expires_in_days")] public int ExpiresInDays { get; init; } = 0;
}

// Respuesta con nivel de acceso del usuario sobre el expediente
public record PatientAccessLevelResponseDTO
{
    [JsonPropertyName("patient_id")]    public long PatientId { get; init; }
    [JsonPropertyName("access_level")]  public int AccessLevel { get; init; }
    [JsonPropertyName("access_name")]   public string AccessName { get; init; } = string.Empty;
    [JsonPropertyName("owner_org_id")]  public long? OwnerOrgId { get; init; }
    [JsonPropertyName("can_see_private_fields")] public bool CanSeePrivateFields { get; init; }
}

public class RequestRecordAccessRules : AbstractValidator<RequestRecordAccessRequestDTO>
{
    public RequestRecordAccessRules()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("Paciente inválido.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("El motivo de acceso es obligatorio.")
            .MaximumLength(500);
    }
}
```

---

## 9. Handlers en `ClinicalDomain`

**Archivo:** `src/Application/Features/Clinical/Domain/ClinicalDomain.cs`

```csharp
// Agregar después de los handlers existentes:
RegisterActionHandler<IClinicalService>(
    AppAction.Clinical.GetSharedPatientRecord,
    nameof(IClinicalService.GetPatientRecordAsync));
// Nota: usa el MISMO método — la privacidad se aplica internamente según el contexto

RegisterActionHandler<IClinicalPrivacyService>(
    AppAction.Clinical.RequestRecordAccess,
    nameof(IClinicalPrivacyService.RequestRecordAccessAsync));

RegisterActionHandler<IClinicalPrivacyService>(
    AppAction.Clinical.ApproveRecordAccessRequest,
    nameof(IClinicalPrivacyService.ApproveAccessRequestAsync));

RegisterActionHandler<IClinicalPrivacyService>(
    AppAction.Clinical.RevokeRecordAccess,
    nameof(IClinicalPrivacyService.RevokeAccessAsync));

RegisterActionHandler<IClinicalPrivacyService>(
    AppAction.Clinical.GetRecordAccessRequests,
    nameof(IClinicalPrivacyService.GetAccessRequestsAsync));
```

---

## 10. Serialización AOT

**Archivo:** `src/Application/Features/Clinical/Serialization/ClinicalJsonContext.cs`

```csharp
// Agregar:
[JsonSerializable(typeof(RequestRecordAccessRequestDTO))]
[JsonSerializable(typeof(PatientAccessLevelResponseDTO))]
[JsonSerializable(typeof(List<ConsultationHistoryDTO>))]
```

---

## 11. Tests

### 11.1 Fakers nuevos

**`FakerCrossOrgAccess.cs`** — `tests/SharedFakers/Fakers/Clinical/`

```csharp
public static class FakerCrossOrgAccess
{
    // Escenario: médico de otra org intenta ver expediente
    public static RequestRecordAccessRequestDTO Create(long patientId, long requestingOrgId) =>
        new RequestRecordAccessRequestDTO
        {
            PatientId      = patientId,
            OrganizationId = requestingOrgId,
            Reason         = "Paciente referido para segunda opinión",
            ExpiresInDays  = 30
        };

    // Escenario: laboratorio intenta ver expediente (debe fallar)
    public static (long OrgId, int OrgType) CreateLabContext() => (99, (int)TipoOrganizacion.LABORATORIO);
}
```

### 11.2 Tests requeridos — `ClinicalPrivacyTests.cs`

```
Escenarios a cubrir (12 tests):

✅ GetPatientRecord_MismaOrg_VeCamposPrivados
✅ GetPatientRecord_OtraOrgClinica_SoloVeDiagnostico
✅ GetPatientRecord_OtraOrgClinica_SubjectiveDataEsNull
✅ GetPatientRecord_OtraOrgClinica_AnalysisDataEsNull
✅ GetPatientRecord_OtraOrgClinica_DiagnosisCIE10Visible
✅ GetPatientRecord_OtraOrgClinica_AlergiasVisibles
✅ GetPatientRecord_Laboratorio_AccesoDenegado                   ← 403
✅ GetPatientRecord_Farmacia_AccesoDenegado                      ← 403
✅ GetPatientRecord_CrossOrg_GeneraAuditoriaEnLog                ← OutputAction
✅ GetPatientRecord_MismaOrg_NoGeneraAuditoriaCrossOrg           ← No polución de logs
✅ RequestRecordAccess_OrgValida_CreaRegistroPendiente
✅ GetPatientRecord_ConNotasMixtas_PrivacidadGranularPorNota     ← Edge case clave
    // Paciente con 3 consultas: 2 de org A, 1 de org B
    // Usuario de org A → ve las 2 de A completas, la de B solo con CIE-10
    // Usuario de org B → ve la 1 de B completa, las 2 de A solo con CIE-10
```

---

## 12. Checklist de entrega para Antigravity

### Fase 1 — Base de datos (sin tocar C#)
- [ ] Ejecutar `030_Granular_Privacy.sql` (Bloques A–E en orden)
- [ ] Verificar: `SELECT id, organization_id FROM care.mst_consultation LIMIT 10;`
- [ ] Verificar: `SELECT * FROM common.mst_catalog WHERE id IN (35, 36);`
- [ ] Verificar: `SELECT * FROM security.mst_action WHERE id BETWEEN 4127 AND 4131;`

### Fase 2 — Dominio (sin cambios en DB)
- [ ] Crear `ClinicalAccessLevel.cs` enum
- [ ] Crear `CrossOrgAccessStatus.cs` enum
- [ ] Crear `ClinicalPrivacyFilterAttribute.cs`
- [ ] Extender `IUserContext` con `OrganizationTypeId`
- [ ] Extender `UserContext.cs` con `OrganizationTypeId`
- [ ] Agregar constantes `4127–4131` en `AppAction.Clinical.cs`
- [ ] Agregar campo `NoteOwnerOrgId` + campos SOAP en `ConsultationRow`

### Fase 3 — Aplicación
- [ ] Extender `ConsultationHistoryDTO` con nuevos campos
- [ ] Crear `IClinicalPrivacyService` + `ClinicalPrivacyService`
- [ ] Reemplazar query SQL en `GetFullPatientRecordAsync` (§5.2)
- [ ] Reemplazar `GetPatientRecordAsync` en `ClinicalService` (§4.2)
- [ ] Inyectar `IClinicalPrivacyService` + `IUserContext` en `ClinicalService`
- [ ] Crear `CrossOrgAccessAuditOutputAction`
- [ ] Crear `RequestRecordAccessRequestDTO` y `PatientAccessLevelResponseDTO`
- [ ] Registrar handlers nuevos en `ClinicalDomain`
- [ ] Registrar DTOs nuevos en `ClinicalJsonContext`

### Fase 4 — Tests
- [ ] Crear `FakerCrossOrgAccess.cs`
- [ ] Crear `ClinicalPrivacyTests.cs` con los 12 tests (§11.2)
- [ ] Verificar que `SecurityDataIsolationTests.Ensure_Organization_Data_Isolation` sigue pasando

---

## 13. Mapa de archivos afectados

| Archivo | Acción | Fase |
|---|---|---|
| `src/Migrations/Scripts/030_Granular_Privacy.sql` | **CREAR** | 1 |
| `src/Domain/Enums/Clinical/ClinicalAccessLevel.cs` | **CREAR** | 2 |
| `src/Domain/Enums/Clinical/CrossOrgAccessStatus.cs` | **CREAR** | 2 |
| `src/Domain/Common/Attributes/ClinicalPrivacyFilterAttribute.cs` | **CREAR** | 2 |
| `src/Domain/Interfaces/Security/IUserContext.cs` | Modificar | 2 |
| `src/Infrastructure/Shared/Security/UserContext.cs` | Modificar | 2 |
| `src/Domain/Const/AppAction.Clinical.cs` | Modificar | 2 |
| `src/Domain/Interfaces/Repositories/Clinical/IPatientRepository.cs` | Modificar — `ConsultationRow` | 2 |
| `src/Application/Features/Clinical/Dtos/Response/PatientRecordResponseDTO.cs` | Modificar | 3 |
| `src/Application/Features/Clinical/Interfaces/IClinicalPrivacyService.cs` | **CREAR** | 3 |
| `src/Application/Features/Clinical/Services/ClinicalPrivacyService.cs` | **CREAR** | 3 |
| `src/Application/Features/Clinical/Services/ClinicalService.cs` | Modificar — `GetPatientRecordAsync` | 3 |
| `src/Infrastructure/Persistence/Repositories/Clinical/PatientRepository.cs` | Modificar — SQL query | 3 |
| `src/Application/Features/Clinical/OutputActions/CrossOrgAccessAuditOutputAction.cs` | **CREAR** | 3 |
| `src/Application/Features/Clinical/Dtos/Request/CrossOrgAccessRequestDTO.cs` | **CREAR** | 3 |
| `src/Application/Features/Clinical/Domain/ClinicalDomain.cs` | Modificar | 3 |
| `src/Application/Features/Clinical/Serialization/ClinicalJsonContext.cs` | Modificar | 3 |
| `tests/SharedFakers/Fakers/Clinical/FakerCrossOrgAccess.cs` | **CREAR** | 4 |
| `tests/IntegrationTests/Service/ClinicalPrivacyTests.cs` | **CREAR** | 4 |

> **⚠️ Nota crítica:** El cambio más urgente es el `BLOQUE A` del SQL — agregar `organization_id` a `care.mst_consultation`. Sin ese campo, toda la lógica de privacidad depende del JOIN con `log_consultation_ledger`, que puede no existir para consultas antiguas o directas. Con `organization_id` en la consulta, el filtro es O(1) por índice.

---

*Fin del documento — MedfarLabs Core · Privacidad Granular por Organización*
