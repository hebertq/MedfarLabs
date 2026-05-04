# MedfarLabs · Core — Instrucciones Antigravity
## Módulo: Plantillas de Exámenes y Resultados Estructurados
> Plantillas globales · Personalización por laboratorio · Ingreso de resultados  
> Mayo 2025

---

## 0. Contexto — arquitectura del sistema de plantillas

El Core ya tiene dos niveles de plantillas implementados a nivel de entidades y tablas. El objetivo es completar los gaps para que el flujo completo funcione: desde consultar plantillas globales hasta ingresar resultados estructurados con validación de rangos.

### Dos niveles de plantillas

**Nivel 1 — Plantilla global** (`mst_lab_exam_template` + `det_lab_exam_template_item`)  
Creada y mantenida por MedfarLabs. Define analitos estándar, unidades y rangos de referencia. Ya existen 4 plantillas sembradas (Hemograma, Perfil Lipídico, Química Básica, Orina).

**Nivel 2 — Configuración por organización** (`cfg_org_lab_exam_config` + `cfg_org_lab_exam_item_config`)  
Clon personalizable por laboratorio. Puede renombrar analitos, cambiar rangos, ocultar ítems o reordenarlos. Si no personaliza, usa los valores globales tal como están.

### Estado actual de implementación

| Estado | Descripción |
|---|---|
| ✅ Existe y funciona | `CloneTemplateAsync`, `UpdateLabConfigItemAsync`, `SaveStructuredResultAsync` en `LaboratoryService` |
| 🔴 **CRÍTICO — no conectado** | Los tres métodos anteriores tienen acción (8012, 8013, 8014) pero **NO están registrados en `LaboratoryDomain`**. El dispatcher los ignora completamente |
| ❌ Falta | `GetTemplateWithConfigAsync` — el técnico necesita cargar la plantilla con valores efectivos antes de ingresar resultados |
| ❌ Falta | `GetOrgTemplatesAsync` — listar qué plantillas tiene configuradas el laboratorio |
| ❌ Falta | `ResetTemplateToDefaultAsync` — volver a valores globales si el lab quiere deshacer personalización |
| ⚠️ Incompleto | `SaveStructuredResultAsync` tiene `orgConfigId = 0` hardcodeado, sin resolución real |
| ⚠️ Incompleto | DTOs de `CloneTemplate` y `UpdateLabConfigItem` usan `class` en vez de `record`, sin `[ActionMapping]` ni `[JsonIgnore]` en `OrganizationId` |

> **Nota:** La migración 016 ya sembró las 4 plantillas clínicas estándar con ítems, rangos demográficos y opciones cualitativas. No se re-siembra; solo se extiende.

---

## 1. Flujo completo — de plantilla a resultado

El flujo que debe quedar funcionando al finalizar estos cambios:

1. Laboratorio consulta sus plantillas disponibles → `GetOrgTemplatesAsync`
2. Si no ha clonado una plantilla global, la clona → `CloneTemplateAsync` *(ya existe, falta conectar)*
3. Técnico abre la plantilla con valores efectivos para ingresar resultados → `GetTemplateWithConfigAsync` *(nuevo)*
4. Técnico puede personalizar rangos o nombres → `UpdateLabConfigItemAsync` *(ya existe, falta conectar)*
5. Técnico ingresa los valores de resultado por analito → `SaveStructuredResultAsync` *(ya existe, falta corregir)*
6. El sistema detecta si cada valor está fuera de rango → `IsOutOfRangeAsync` *(ya existe)*
7. El médico/paciente consulta el resultado con flags de valores críticos → `ObtenerResultadoPorOrdenAsync` *(ya existe)*

> Si el laboratorio nunca clona una plantilla, usa la plantilla global directamente. El sistema debe soportar ambos casos: con y sin personalización.

---

## 2. Constantes C# — `AppAction.Laboratory.cs`

**Archivo:** `src/Domain/Const/AppAction.Laboratory.cs`

| ID | Constante | Descripción | Estado |
|---|---|---|---|
| 8011 | `ViewTemplate` | Ver plantilla global | Existe — falta handler |
| 8012 | `CloneTemplate` | Clonar plantilla para org | Existe — falta handler |
| 8013 | `UpdateConfig` | Actualizar ítem config org | Existe — falta handler |
| 8014 | `SaveStructuredResult` | Guardar resultado estructurado | Existe — falta handler |
| 8015 | `GetOrgTemplates` | Listar plantillas configuradas por org | **AGREGAR NUEVO** |
| 8016 | `GetTemplateWithConfig` | Plantilla con valores efectivos para ingreso | **AGREGAR NUEVO** |
| 8017 | `ResetTemplateToDefault` | Revertir personalización a valores globales | **AGREGAR NUEVO** |
| 8018 | `GetGlobalTemplates` | Listar plantillas globales disponibles | **AGREGAR NUEVO** |

Agregar al final del bloque `Laboratory` (antes del cierre de la clase):

```csharp
public const int GetOrgTemplates        = 8015; // Plantillas configuradas por la organización
public const int GetTemplateWithConfig   = 8016; // Plantilla con valores efectivos para ingreso de resultado
public const int ResetTemplateToDefault  = 8017; // Revertir personalización a valores globales
public const int GetGlobalTemplates      = 8018; // Catálogo de plantillas globales disponibles
```

> Eliminar las constantes `string ManageSampleConfig` y `ViewSampleConfig` — no siguen el patrón `int` del resto del sistema y no están en uso.

---

## 3. Entidades de dominio — ajustes menores

### 3.1 `OrgLabExamConfig.cs` — agregar campos faltantes

**Archivo:** `src/Domain/Entities/Laboratory/OrgLabExamConfig.cs`

La entidad ya existe. Agregar los campos que faltan respecto a la tabla:

```csharp
[DbColumn("updated_at")]
public DateTime? UpdatedAt { get; set; }

[DbColumn("is_active")]
public bool IsActive { get; set; } = true;
```

### 3.2 `OrgLabExamItemConfig.cs` — agregar `is_active`

**Archivo:** `src/Domain/Entities/Laboratory/OrgLabExamItemConfig.cs`

El campo ya existe en la tabla pero no en la entidad:

```csharp
[DbColumn("is_active")]
public bool IsActive { get; set; } = true;
```

### 3.3 `LabExamTemplate.cs` — agregar campo `Category`

**Archivo:** `src/Domain/Entities/Laboratory/LabExamTemplate.cs`

Agregar para agrupar plantillas por área clínica:

```csharp
[DbColumn("category")]
public string? Category { get; set; }
```

---

## 4. DTOs de Request — corregir existentes y crear nuevos

### 4.1 `CloneTemplateRequestDTO.cs` — convertir a record con ActionMapping

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/CloneTemplateRequestDTO.cs`

Actualmente es una `class` sin `[ActionMapping]`. Reemplazar completamente:

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.CloneTemplate)]
public record CloneTemplateRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    [JsonPropertyName("template_id")]
    public long TemplateId { get; init; }
}

public class CloneTemplateRules : AbstractValidator<CloneTemplateRequestDTO>
{
    public CloneTemplateRules()
    {
        RuleFor(x => x.TemplateId)
            .GreaterThan(0).WithMessage("Plantilla inválida.");
    }
}
```

### 4.2 `UpdateLabConfigItemRequestDTO.cs` — convertir a record con ActionMapping

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/UpdateLabConfigItemRequestDTO.cs`

Reemplazar completamente:

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.UpdateConfig)]
public record UpdateLabConfigItemRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    [JsonPropertyName("org_config_id")]    public long OrgConfigId { get; init; }
    [JsonPropertyName("template_item_id")] public long TemplateItemId { get; init; }
    [JsonPropertyName("custom_name")]      public string? CustomName { get; init; }
    [JsonPropertyName("custom_unit")]      public string? CustomUnit { get; init; }
    [JsonPropertyName("custom_ref_min")]   public decimal? CustomRefMin { get; init; }
    [JsonPropertyName("custom_ref_max")]   public decimal? CustomRefMax { get; init; }
    [JsonPropertyName("custom_ref_options")] public string? CustomRefOptions { get; init; }
    [JsonPropertyName("is_hidden")]        public bool IsHidden { get; init; }
    [JsonPropertyName("sort_order")]       public int? SortOrder { get; init; }
}

public class UpdateLabConfigItemRules : AbstractValidator<UpdateLabConfigItemRequestDTO>
{
    public UpdateLabConfigItemRules()
    {
        RuleFor(x => x.OrgConfigId)
            .GreaterThan(0).WithMessage("Config de organización inválida.");
        RuleFor(x => x.TemplateItemId)
            .GreaterThan(0).WithMessage("Ítem inválido.");
        When(x => x.CustomRefMin.HasValue && x.CustomRefMax.HasValue, () =>
        {
            RuleFor(x => x.CustomRefMax)
                .GreaterThan(x => x.CustomRefMin)
                .WithMessage("El rango máximo debe ser mayor al mínimo.");
        });
    }
}
```

### 4.3 `SaveStructuredResultRequestDTO.cs` — convertir a record con ActionMapping

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/SaveStructuredResultRequestDTO.cs`

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.SaveStructuredResult)]
public record SaveStructuredResultRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    [JsonPropertyName("lab_result_id")] public long LabResultId { get; init; }
    [JsonPropertyName("patient_id")]    public long PatientId { get; init; }
    [JsonPropertyName("items")]         public List<StructuredResultItemDTO> Items { get; init; } = new();
}

public record StructuredResultItemDTO
{
    [JsonPropertyName("item_id")]       public long ItemId { get; init; }
    [JsonPropertyName("numeric_value")] public decimal? NumericValue { get; init; }
    [JsonPropertyName("text_value")]    public string? TextValue { get; init; }
}

public class SaveStructuredResultRules : AbstractValidator<SaveStructuredResultRequestDTO>
{
    public SaveStructuredResultRules()
    {
        RuleFor(x => x.LabResultId).GreaterThan(0).WithMessage("Resultado principal inválido.");
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("Paciente requerido.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Debe incluir al menos un analito.");
    }
}
```

### 4.4 `GetOrgTemplatesRequestDTO.cs` — crear nuevo

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/GetOrgTemplatesRequestDTO.cs`

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.GetOrgTemplates)]
public record GetOrgTemplatesRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    // Opcional: filtrar por servicio específico
    [JsonPropertyName("service_id")]
    public long? ServiceId { get; init; }
}
```

### 4.5 `GetTemplateWithConfigRequestDTO.cs` — crear nuevo

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/GetTemplateWithConfigRequestDTO.cs`

Esta es la operación central. Carga la plantilla con valores efectivos (custom si existe, global como fallback):

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.GetTemplateWithConfig)]
public record GetTemplateWithConfigRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    // Pasar template_id (plantilla global) o org_config_id (config ya clonada)
    [JsonPropertyName("template_id")]    public long? TemplateId { get; init; }
    [JsonPropertyName("org_config_id")]  public long? OrgConfigId { get; init; }

    // Opcional: aplicar rangos demográficos en preview
    [JsonPropertyName("patient_id")]     public long? PatientId { get; init; }
}

public class GetTemplateWithConfigRules : AbstractValidator<GetTemplateWithConfigRequestDTO>
{
    public GetTemplateWithConfigRules()
    {
        RuleFor(x => x)
            .Must(x => x.TemplateId.HasValue || x.OrgConfigId.HasValue)
            .WithMessage("Debe proporcionar template_id u org_config_id.");
    }
}
```

### 4.6 `ResetTemplateToDefaultRequestDTO.cs` — crear nuevo

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/ResetTemplateToDefaultRequestDTO.cs`

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.ResetTemplateToDefault)]
public record ResetTemplateToDefaultRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    [JsonPropertyName("org_config_id")] public long OrgConfigId { get; init; }

    // reset_all = true → resetea todos los ítems; false → solo los de item_ids
    [JsonPropertyName("reset_all")]     public bool ResetAll { get; init; } = true;
    [JsonPropertyName("item_ids")]      public List<long> ItemIds { get; init; } = new();
}
```

### 4.7 `GetGlobalTemplatesRequestDTO.cs` — crear nuevo

**Archivo:** `src/Application/Features/Laboratory/Dtos/Request/GetGlobalTemplatesRequestDTO.cs`

```csharp
[ActionMapping(AppModule.Laboratory, AppAction.Laboratory.GetGlobalTemplates)]
public record GetGlobalTemplatesRequestDTO : IHasOrganization
{
    [JsonIgnore] public long OrganizationId { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }
}
```

---

## 5. DTOs de Response — crear archivo nuevo

**Archivo:** `src/Application/Features/Laboratory/Dtos/Response/TemplateResponseDTOs.cs`

Todos los response DTOs del módulo de plantillas en un solo archivo:

```csharp
// Resumen de plantillas configuradas de la organización
public record OrgTemplateResponseDTO
{
    [JsonPropertyName("org_config_id")]  public long OrgConfigId { get; init; }
    [JsonPropertyName("template_id")]    public long TemplateId { get; init; }
    [JsonPropertyName("service_id")]     public long ServiceId { get; init; }
    [JsonPropertyName("name")]           public string Name { get; init; } = string.Empty;
    [JsonPropertyName("custom_name")]    public string? CustomName { get; init; }
    [JsonPropertyName("category")]       public string? Category { get; init; }
    [JsonPropertyName("item_count")]     public int ItemCount { get; init; }
    [JsonPropertyName("is_customized")]  public bool IsCustomized { get; init; }
    [JsonPropertyName("cloned_at")]      public DateTime? ClonedAt { get; init; }
}

// Ítem con valores efectivos (custom o global según corresponda)
public record EffectiveTemplateItemDTO
{
    [JsonPropertyName("item_id")]              public long ItemId { get; init; }
    [JsonPropertyName("org_config_item_id")]   public long? OrgConfigItemId { get; init; }
    // DisplayName = CustomName si existe, AnalyteName global si no
    [JsonPropertyName("display_name")]         public string DisplayName { get; init; } = string.Empty;
    [JsonPropertyName("analyte_name_global")]  public string AnalyteNameGlobal { get; init; } = string.Empty;
    [JsonPropertyName("unit")]                 public string? Unit { get; init; }
    [JsonPropertyName("value_type")]           public string ValueType { get; init; } = string.Empty;
    // Rango efectivo: custom > demográfico > global
    [JsonPropertyName("ref_min")]              public decimal? RefMin { get; init; }
    [JsonPropertyName("ref_max")]              public decimal? RefMax { get; init; }
    [JsonPropertyName("ref_options")]          public List<string> RefOptions { get; init; } = new();
    [JsonPropertyName("is_customized")]        public bool IsCustomized { get; init; }
    [JsonPropertyName("is_hidden")]            public bool IsHidden { get; init; }
    [JsonPropertyName("is_required")]          public bool IsRequired { get; init; }
    [JsonPropertyName("sort_order")]           public int SortOrder { get; init; }
}

// Plantilla completa lista para ingreso de resultado
public record TemplateWithConfigResponseDTO
{
    [JsonPropertyName("template_id")]    public long TemplateId { get; init; }
    [JsonPropertyName("org_config_id")]  public long? OrgConfigId { get; init; }
    [JsonPropertyName("template_name")]  public string TemplateName { get; init; } = string.Empty;
    [JsonPropertyName("display_name")]   public string DisplayName { get; init; } = string.Empty;
    [JsonPropertyName("category")]       public string? Category { get; init; }
    [JsonPropertyName("is_customized")]  public bool IsCustomized { get; init; }
    [JsonPropertyName("items")]          public List<EffectiveTemplateItemDTO> Items { get; init; } = new();
}

// Vista del catálogo global (para que el lab elija qué clonar)
public record GlobalTemplateResponseDTO
{
    [JsonPropertyName("template_id")]           public long TemplateId { get; init; }
    [JsonPropertyName("service_id")]            public long ServiceId { get; init; }
    [JsonPropertyName("name")]                  public string Name { get; init; } = string.Empty;
    [JsonPropertyName("category")]              public string? Category { get; init; }
    [JsonPropertyName("item_count")]            public int ItemCount { get; init; }
    [JsonPropertyName("is_already_configured")] public bool IsAlreadyConfigured { get; init; }
}
```

---

## 6. Repositorios — nuevos métodos

### 6.1 `ILabExamTemplateRepository` — agregar métodos

**Archivo:** `src/Domain/Interfaces/Repositories/Laboratory/ILabExamTemplateRepository.cs`

```csharp
public interface ILabExamTemplateRepository : IBaseRepository<LabExamTemplate>
{
    Task<IEnumerable<LabExamTemplateItem>> GetItemsByTemplateIdAsync(long templateId);
    Task<IEnumerable<LabExamTemplateItemRange>> GetRangesByItemIdAsync(long itemId);
    // NUEVOS:
    Task<IEnumerable<LabExamTemplate>> GetAllPublishedAsync(string? category = null);
    Task<int> GetItemCountAsync(long templateId);
}
```

**Implementación en** `LabExamTemplateRepository.cs` — agregar:

```csharp
public async Task<IEnumerable<LabExamTemplate>> GetAllPublishedAsync(string? category = null)
{
    var sql = @"SELECT * FROM laboratory.mst_lab_exam_template
               WHERE is_published = TRUE AND is_active = TRUE
               AND (@Category IS NULL OR category = @Category)
               ORDER BY category, name;";
    return await _connection.QueryAsync<LabExamTemplate>(sql,
        new { Category = category }, _transaction);
}

public async Task<int> GetItemCountAsync(long templateId)
{
    var sql = "SELECT COUNT(*) FROM laboratory.det_lab_exam_template_item WHERE template_id = @TemplateId AND is_active = TRUE;";
    return await _connection.ExecuteScalarAsync<int>(sql, new { TemplateId = templateId }, _transaction);
}
```

### 6.2 `IOrgLabConfigRepository` — agregar métodos

**Archivo:** `src/Domain/Interfaces/Repositories/Laboratory/IOrgLabConfigRepository.cs`

```csharp
public interface IOrgLabConfigRepository : IBaseRepository<OrgLabExamConfig>
{
    // Existentes (no modificar):
    Task<OrgLabExamConfig?> GetConfigByOrgAndTemplateAsync(long organizationId, long templateId);
    Task<IEnumerable<OrgLabExamItemConfig>> GetItemConfigsAsync(long orgConfigId);
    Task<OrgLabExamItemConfig?> GetItemConfigAsync(long orgConfigId, long templateItemId);
    Task<long> AddItemConfigAsync(OrgLabExamItemConfig itemConfig);
    Task UpdateItemConfigAsync(OrgLabExamItemConfig itemConfig);
    // NUEVOS:
    Task<IEnumerable<OrgLabExamConfig>> GetByOrganizationAsync(long organizationId, long? serviceId = null);
    Task<OrgLabExamConfig?> GetByIdAndOrgAsync(long configId, long organizationId);
    Task ResetItemsToDefaultAsync(long orgConfigId, IEnumerable<long>? templateItemIds = null);
    Task<bool> HasCustomizationsAsync(long orgConfigId);
}
```

**Implementación en** `OrgLabConfigRepository.cs` — agregar:

```csharp
public async Task<IEnumerable<OrgLabExamConfig>> GetByOrganizationAsync(long organizationId, long? serviceId = null)
{
    var sql = @"SELECT * FROM laboratory.cfg_org_lab_exam_config
               WHERE organization_id = @OrgId AND is_active = TRUE
               AND (@ServiceId IS NULL OR service_id = @ServiceId)
               ORDER BY cloned_at DESC;";
    return await _connection.QueryAsync<OrgLabExamConfig>(sql,
        new { OrgId = organizationId, ServiceId = serviceId }, _transaction);
}

public async Task<OrgLabExamConfig?> GetByIdAndOrgAsync(long configId, long organizationId)
{
    var sql = "SELECT * FROM laboratory.cfg_org_lab_exam_config WHERE id = @Id AND organization_id = @OrgId AND is_active = TRUE LIMIT 1;";
    return await _connection.QueryFirstOrDefaultAsync<OrgLabExamConfig>(sql,
        new { Id = configId, OrgId = organizationId }, _transaction);
}

public async Task ResetItemsToDefaultAsync(long orgConfigId, IEnumerable<long>? templateItemIds = null)
{
    if (templateItemIds != null && templateItemIds.Any())
    {
        // Resetear solo ítems especificados
        var sql = @"UPDATE laboratory.cfg_org_lab_exam_item_config SET
            custom_name = NULL, custom_unit = NULL,
            custom_ref_min = NULL, custom_ref_max = NULL,
            custom_ref_options = NULL, is_hidden = FALSE
            WHERE org_config_id = @ConfigId
            AND template_item_id = ANY(@ItemIds::bigint[]);";
        await _connection.ExecuteAsync(sql,
            new { ConfigId = orgConfigId, ItemIds = templateItemIds.ToList() }, _transaction);
    }
    else
    {
        // Resetear todos
        var sql = @"UPDATE laboratory.cfg_org_lab_exam_item_config SET
            custom_name = NULL, custom_unit = NULL,
            custom_ref_min = NULL, custom_ref_max = NULL,
            custom_ref_options = NULL, is_hidden = FALSE
            WHERE org_config_id = @ConfigId;";
        await _connection.ExecuteAsync(sql, new { ConfigId = orgConfigId }, _transaction);
    }
}

public async Task<bool> HasCustomizationsAsync(long orgConfigId)
{
    var sql = @"SELECT EXISTS (
        SELECT 1 FROM laboratory.cfg_org_lab_exam_item_config
        WHERE org_config_id = @ConfigId
          AND is_active = TRUE
          AND (custom_name IS NOT NULL OR custom_unit IS NOT NULL
           OR custom_ref_min IS NOT NULL OR custom_ref_max IS NOT NULL
           OR is_hidden = TRUE));";
    return await _connection.ExecuteScalarAsync<bool>(sql, new { ConfigId = orgConfigId }, _transaction);
}
```

---

## 7. `ILaboratoryService` e implementación

### 7.1 Agregar firmas nuevas

**Archivo:** `src/Application/Features/Laboratory/Interfaces/ILaboratoryService.cs`

Agregar al final de la interfaz:

```csharp
// Catálogo y configuración de plantillas
Task<BaseResponse<List<GlobalTemplateResponseDTO>>> GetGlobalTemplatesAsync(GetGlobalTemplatesRequestDTO request);
Task<BaseResponse<List<OrgTemplateResponseDTO>>> GetOrgTemplatesAsync(GetOrgTemplatesRequestDTO request);
Task<BaseResponse<TemplateWithConfigResponseDTO?>> GetTemplateWithConfigAsync(GetTemplateWithConfigRequestDTO request);
Task<BaseResponse<bool>> ResetTemplateToDefaultAsync(ResetTemplateToDefaultRequestDTO request);
```

### 7.2 Implementar `GetGlobalTemplatesAsync`

```csharp
public async Task<BaseResponse<List<GlobalTemplateResponseDTO>>> GetGlobalTemplatesAsync(GetGlobalTemplatesRequestDTO request)
{
    var templates = await _unitOfWork.LabExamTemplates.GetAllPublishedAsync(request.Category);
    var orgConfigs = await _unitOfWork.OrgLabConfigs.GetByOrganizationAsync(request.OrganizationId);
    var configuredTemplateIds = orgConfigs.Select(c => c.TemplateId).ToHashSet();

    var dtos = new List<GlobalTemplateResponseDTO>();
    foreach (var t in templates)
    {
        var count = await _unitOfWork.LabExamTemplates.GetItemCountAsync(t.Id);
        dtos.Add(new GlobalTemplateResponseDTO
        {
            TemplateId = t.Id,
            ServiceId = t.ServiceId,
            Name = t.Name,
            Category = t.Category,
            ItemCount = count,
            IsAlreadyConfigured = configuredTemplateIds.Contains(t.Id)
        });
    }
    return BaseResponse<List<GlobalTemplateResponseDTO>>.Success(dtos);
}
```

### 7.3 Implementar `GetOrgTemplatesAsync`

```csharp
public async Task<BaseResponse<List<OrgTemplateResponseDTO>>> GetOrgTemplatesAsync(GetOrgTemplatesRequestDTO request)
{
    var configs = await _unitOfWork.OrgLabConfigs.GetByOrganizationAsync(request.OrganizationId, request.ServiceId);
    var dtos = new List<OrgTemplateResponseDTO>();

    foreach (var c in configs)
    {
        var template = await _unitOfWork.LabExamTemplates.GetByIdAsync(c.TemplateId);
        var itemCount = await _unitOfWork.LabExamTemplates.GetItemCountAsync(c.TemplateId);
        var hasCustom = await _unitOfWork.OrgLabConfigs.HasCustomizationsAsync(c.Id);
        dtos.Add(new OrgTemplateResponseDTO
        {
            OrgConfigId = c.Id,
            TemplateId = c.TemplateId,
            ServiceId = c.ServiceId,
            Name = template?.Name ?? string.Empty,
            CustomName = c.CustomName,
            Category = template?.Category,
            ItemCount = itemCount,
            IsCustomized = hasCustom,
            ClonedAt = c.ClonedAt
        });
    }
    return BaseResponse<List<OrgTemplateResponseDTO>>.Success(dtos);
}
```

### 7.4 Implementar `GetTemplateWithConfigAsync` *(método central)*

Lógica de resolución de valores: **custom del lab → rango demográfico → valor global**.

```csharp
public async Task<BaseResponse<TemplateWithConfigResponseDTO?>> GetTemplateWithConfigAsync(GetTemplateWithConfigRequestDTO request)
{
    LabExamTemplate? template = null;
    OrgLabExamConfig? orgConfig = null;

    // Resolver template y config
    if (request.OrgConfigId.HasValue)
    {
        orgConfig = await _unitOfWork.OrgLabConfigs.GetByIdAndOrgAsync(request.OrgConfigId.Value, request.OrganizationId);
        if (orgConfig == null) return BaseResponse<TemplateWithConfigResponseDTO?>.Failure("Configuración no encontrada.");
        template = await _unitOfWork.LabExamTemplates.GetByIdAsync(orgConfig.TemplateId);
    }
    else if (request.TemplateId.HasValue)
    {
        template = await _unitOfWork.LabExamTemplates.GetByIdAsync(request.TemplateId.Value);
        // Intentar encontrar config de la org (puede no existir si nunca clonó)
        orgConfig = await _unitOfWork.OrgLabConfigs.GetConfigByOrgAndTemplateAsync(request.OrganizationId, request.TemplateId.Value);
    }

    if (template == null) return BaseResponse<TemplateWithConfigResponseDTO?>.Failure("Plantilla no encontrada.");

    // Ítems globales
    var globalItems = await _unitOfWork.LabExamTemplates.GetItemsByTemplateIdAsync(template.Id);

    // Config de ítems de la org (si hay config clonada)
    var itemConfigs = orgConfig != null
        ? (await _unitOfWork.OrgLabConfigs.GetItemConfigsAsync(orgConfig.Id)).ToDictionary(i => i.TemplateItemId)
        : new Dictionary<long, OrgLabExamItemConfig>();

    // Datos demográficos del paciente (si se proporcionó)
    string? patientGender = null;
    int? patientAge = null;
    if (request.PatientId.HasValue)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId.Value);
        if (patient != null)
        {
            var person = await _unitOfWork.Persons.GetByIdAsync(patient.PersonId);
            if (person != null)
            {
                patientAge = DateTime.Today.Year - person.BirthDate.Year;
                if (person.BirthDate.Date > DateTime.Today.AddYears(-patientAge.Value)) patientAge--;
                patientGender = person.GenderId == 1 ? "M" : "F";
            }
        }
    }

    // Construir lista de ítems efectivos
    var effectiveItems = new List<EffectiveTemplateItemDTO>();
    foreach (var g in globalItems.OrderBy(i => i.SortOrder))
    {
        itemConfigs.TryGetValue(g.Id, out var customCfg);
        bool hasCustom = customCfg != null;

        // Resolver rango: custom > demográfico > global
        decimal? refMin = hasCustom && customCfg!.CustomRefMin.HasValue ? customCfg.CustomRefMin : null;
        decimal? refMax = hasCustom && customCfg!.CustomRefMax.HasValue ? customCfg.CustomRefMax : null;

        if (refMin == null && refMax == null && g.ValueType == "numeric" && patientAge.HasValue)
        {
            var ranges = await _unitOfWork.LabExamTemplates.GetRangesByItemIdAsync(g.Id);
            var match = ranges.FirstOrDefault(r =>
                (string.IsNullOrEmpty(r.Gender) || r.Gender == patientGender) &&
                (!r.AgeMinYears.HasValue || patientAge >= r.AgeMinYears) &&
                (!r.AgeMaxYears.HasValue || patientAge <= r.AgeMaxYears));
            if (match != null) { refMin = match.RefMin; refMax = match.RefMax; }
        }

        // Fallback a rango global
        refMin ??= g.RefMin;
        refMax ??= g.RefMax;

        // Resolver opciones cualitativas
        var refOptionsRaw = (hasCustom ? customCfg!.CustomRefOptions : null) ?? g.RefOptions;
        var refOptions = new List<string>();
        if (!string.IsNullOrEmpty(refOptionsRaw))
        {
            try { refOptions = JsonSerializer.Deserialize<List<string>>(refOptionsRaw) ?? new(); }
            catch { }
        }

        effectiveItems.Add(new EffectiveTemplateItemDTO
        {
            ItemId = g.Id,
            OrgConfigItemId = customCfg?.Id,
            DisplayName = (hasCustom ? customCfg!.CustomName : null) ?? g.AnalyteName,
            AnalyteNameGlobal = g.AnalyteName,
            Unit = (hasCustom ? customCfg!.CustomUnit : null) ?? g.Unit,
            ValueType = g.ValueType,
            RefMin = refMin,
            RefMax = refMax,
            RefOptions = refOptions,
            IsCustomized = hasCustom && (customCfg!.CustomRefMin.HasValue || customCfg.CustomRefMax.HasValue || customCfg.CustomName != null),
            IsHidden = hasCustom && customCfg!.IsHidden,
            IsRequired = g.IsRequired,
            SortOrder = customCfg?.SortOrder ?? g.SortOrder
        });
    }

    var dto = new TemplateWithConfigResponseDTO
    {
        TemplateId = template.Id,
        OrgConfigId = orgConfig?.Id,
        TemplateName = template.Name,
        DisplayName = orgConfig?.CustomName ?? template.Name,
        Category = template.Category,
        IsCustomized = orgConfig != null && await _unitOfWork.OrgLabConfigs.HasCustomizationsAsync(orgConfig.Id),
        Items = effectiveItems.Where(i => !i.IsHidden).OrderBy(i => i.SortOrder).ToList()
    };

    return BaseResponse<TemplateWithConfigResponseDTO?>.Success(dto);
}
```

### 7.5 Implementar `ResetTemplateToDefaultAsync`

```csharp
public async Task<BaseResponse<bool>> ResetTemplateToDefaultAsync(ResetTemplateToDefaultRequestDTO request)
{
    return await ExecuteInTransactionAsync(async () =>
    {
        var config = await _unitOfWork.OrgLabConfigs.GetByIdAndOrgAsync(request.OrgConfigId, request.OrganizationId);
        if (config == null) return BaseResponse<bool>.Failure("Configuración no encontrada.");

        IEnumerable<long>? idsToReset = request.ResetAll ? null : request.ItemIds;
        await _unitOfWork.OrgLabConfigs.ResetItemsToDefaultAsync(request.OrgConfigId, idsToReset);

        return BaseResponse<bool>.Success(true, request.ResetAll
            ? "Plantilla revertida a valores globales."
            : $"{request.ItemIds.Count} ítem(s) revertidos a valores globales.");
    });
}
```

### 7.6 Corregir `SaveStructuredResultAsync` — resolver `orgConfigId` real

Localizar en `LaboratoryService.cs` la línea `long orgConfigId = 0;` y reemplazar el bloque:

```csharp
// ANTES (incorrecto — hardcodeado):
long orgConfigId = 0;
// En un escenario real: buscar la config de la org para el serviceId de la orden

// DESPUÉS (correcto):
long orgConfigId = 0;
var orgConfig = await _unitOfWork.OrgLabConfigs
    .GetConfigByOrgAndTemplateAsync(labOrder.OrganizationId, labOrder.ServiceId);
if (orgConfig != null) orgConfigId = orgConfig.Id;
// Si orgConfigId = 0, IsOutOfRangeAsync usa rangos demográficos o globales como fallback
```

> `GetConfigByOrgAndTemplateAsync` recibe `(organizationId, templateId)`. Como aquí se tiene `serviceId`, agregar en `IOrgLabConfigRepository` un método `GetConfigByOrgAndServiceAsync(long organizationId, long serviceId)` con la consulta `WHERE service_id = @ServiceId` para resolver correctamente.

---

## 8. `LaboratoryDomain` — registrar todos los handlers faltantes

**Archivo:** `src/Application/Features/Laboratory/Domain/LaboratoryDomain.cs`

Estado actual: 8 handlers registrados. Agregar los **7 faltantes** después de los existentes:

```csharp
// Los tres que EXISTEN en servicio pero nunca se conectaron al dispatcher:
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.CloneTemplate,
    nameof(ILaboratoryService.CloneTemplateAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.UpdateConfig,
    nameof(ILaboratoryService.UpdateLabConfigItemAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.SaveStructuredResult,
    nameof(ILaboratoryService.SaveStructuredResultAsync));

// Los cuatro nuevos:
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.GetGlobalTemplates,
    nameof(ILaboratoryService.GetGlobalTemplatesAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.GetOrgTemplates,
    nameof(ILaboratoryService.GetOrgTemplatesAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.GetTemplateWithConfig,
    nameof(ILaboratoryService.GetTemplateWithConfigAsync));
RegisterActionHandler<ILaboratoryService>(AppAction.Laboratory.ResetTemplateToDefault,
    nameof(ILaboratoryService.ResetTemplateToDefaultAsync));
```

---

## 9. `LaboratoryJsonContext` — registrar DTOs nuevos

**Archivo:** `src/Application/Features/Laboratory/Serialization/LaboratoryJsonContext.cs`

Agregar los siguientes atributos (mantener todos los existentes):

```csharp
// Request — nuevos
[JsonSerializable(typeof(GetOrgTemplatesRequestDTO))]
[JsonSerializable(typeof(GetTemplateWithConfigRequestDTO))]
[JsonSerializable(typeof(ResetTemplateToDefaultRequestDTO))]
[JsonSerializable(typeof(GetGlobalTemplatesRequestDTO))]

// Response — nuevos
[JsonSerializable(typeof(TemplateWithConfigResponseDTO))]
[JsonSerializable(typeof(GlobalTemplateResponseDTO))]
[JsonSerializable(typeof(List<GlobalTemplateResponseDTO>))]
[JsonSerializable(typeof(OrgTemplateResponseDTO))]
[JsonSerializable(typeof(List<OrgTemplateResponseDTO>))]
[JsonSerializable(typeof(EffectiveTemplateItemDTO))]
[JsonSerializable(typeof(List<EffectiveTemplateItemDTO>))]

// Verificar que estos existan (agregar si faltan):
[JsonSerializable(typeof(LabOrderRequestDTO))]
[JsonSerializable(typeof(CreateLabSampleRequestDTO))]
```

---

## 10. Script SQL — `018_Lab_Templates_Complete.sql`

**Archivo:** `src/Migrations/Scripts/018_Lab_Templates_Complete.sql`

Script idempotente. Todos los bloques deben ejecutarse en orden.

### BLOQUE A — Columna `category` en `mst_lab_exam_template`

```sql
ALTER TABLE laboratory.mst_lab_exam_template
    ADD COLUMN IF NOT EXISTS category VARCHAR(60) NULL;

COMMENT ON COLUMN laboratory.mst_lab_exam_template.category IS
    'Categoría clínica: Hematología, Química, Orina, Inmunología, Microbiología, etc.';

-- Asignar categorías a plantillas del seed 016
UPDATE laboratory.mst_lab_exam_template SET category = 'Hematología'  WHERE id = 1;
UPDATE laboratory.mst_lab_exam_template SET category = 'Química'      WHERE id = 2;
UPDATE laboratory.mst_lab_exam_template SET category = 'Química'      WHERE id = 3;
UPDATE laboratory.mst_lab_exam_template SET category = 'Orina'        WHERE id = 4;
```

### BLOQUE B — Servicios y plantillas nuevas

```sql
-- Servicios globales adicionales
INSERT INTO inventory.mst_service (organization_id, category_id, sku_code, name, description, base_price, is_active)
VALUES
(1, 2, 'LAB-TSH',  'TSH (Tirotropina)',              'Función tiroidea',                    18.00, true),
(1, 2, 'LAB-PCR',  'Proteína C Reactiva',            'Marcador de inflamación',             12.00, true),
(1, 2, 'LAB-HBA1', 'Hemoglobina Glicosilada HbA1c',  'Control glucémico diabetes',          20.00, true),
(1, 2, 'LAB-TPT',  'Tiempos de Coagulación PT/PTT',  'Tiempo protrombina y tromboplastina', 22.00, true),
(1, 2, 'LAB-EGH',  'Examen Coproparasitoscópico',    'Análisis de heces',                   10.00, true)
ON CONFLICT (sku_code) DO NOTHING;

-- Plantillas para los nuevos servicios
INSERT INTO laboratory.mst_lab_exam_template (service_id, name, version, is_published, is_active, category)
SELECT id, 'TSH Estándar',          1, TRUE, TRUE, 'Endocrinología'
FROM inventory.mst_service WHERE sku_code = 'LAB-TSH' ON CONFLICT DO NOTHING;

INSERT INTO laboratory.mst_lab_exam_template (service_id, name, version, is_published, is_active, category)
SELECT id, 'PCR Estándar',          1, TRUE, TRUE, 'Química'
FROM inventory.mst_service WHERE sku_code = 'LAB-PCR' ON CONFLICT DO NOTHING;

INSERT INTO laboratory.mst_lab_exam_template (service_id, name, version, is_published, is_active, category)
SELECT id, 'HbA1c Estándar',        1, TRUE, TRUE, 'Endocrinología'
FROM inventory.mst_service WHERE sku_code = 'LAB-HBA1' ON CONFLICT DO NOTHING;

INSERT INTO laboratory.mst_lab_exam_template (service_id, name, version, is_published, is_active, category)
SELECT id, 'Coagulación PT/PTT',    1, TRUE, TRUE, 'Hematología'
FROM inventory.mst_service WHERE sku_code = 'LAB-TPT' ON CONFLICT DO NOTHING;

INSERT INTO laboratory.mst_lab_exam_template (service_id, name, version, is_published, is_active, category)
SELECT id, 'Coproparasitoscópico',  1, TRUE, TRUE, 'Microbiología'
FROM inventory.mst_service WHERE sku_code = 'LAB-EGH' ON CONFLICT DO NOTHING;
```

### BLOQUE C — Ítems de plantillas nuevas

```sql
-- TSH
DO $$ DECLARE v_tid BIGINT;
BEGIN
    SELECT t.id INTO v_tid FROM laboratory.mst_lab_exam_template t
    JOIN inventory.mst_service s ON t.service_id = s.id WHERE s.sku_code = 'LAB-TSH' LIMIT 1;
    IF v_tid IS NOT NULL THEN
        INSERT INTO laboratory.det_lab_exam_template_item
            (template_id, analyte_name, unit, value_type, ref_min, ref_max, sort_order, is_required)
        VALUES
            (v_tid, 'TSH',      'mUI/L', 'numeric', 0.27, 4.20, 1, true),
            (v_tid, 'T4 Libre', 'ng/dL', 'numeric', 0.90, 1.70, 2, false),
            (v_tid, 'T3 Total', 'ng/dL', 'numeric', 0.80, 2.00, 3, false)
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- PCR
DO $$ DECLARE v_tid BIGINT;
BEGIN
    SELECT t.id INTO v_tid FROM laboratory.mst_lab_exam_template t
    JOIN inventory.mst_service s ON t.service_id = s.id WHERE s.sku_code = 'LAB-PCR' LIMIT 1;
    IF v_tid IS NOT NULL THEN
        INSERT INTO laboratory.det_lab_exam_template_item
            (template_id, analyte_name, unit, value_type, ref_min, ref_max, sort_order, is_required)
        VALUES (v_tid, 'PCR', 'mg/L', 'numeric', 0.0, 5.0, 1, true)
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- HbA1c
DO $$ DECLARE v_tid BIGINT;
BEGIN
    SELECT t.id INTO v_tid FROM laboratory.mst_lab_exam_template t
    JOIN inventory.mst_service s ON t.service_id = s.id WHERE s.sku_code = 'LAB-HBA1' LIMIT 1;
    IF v_tid IS NOT NULL THEN
        INSERT INTO laboratory.det_lab_exam_template_item
            (template_id, analyte_name, unit, value_type, ref_min, ref_max, sort_order, is_required)
        VALUES
            (v_tid, 'HbA1c',                    '%',    'numeric', NULL, 5.7, 1, true),
            (v_tid, 'Glucosa estimada promedio', 'mg/dL','numeric', NULL, NULL,2, false)
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- Coagulación
DO $$ DECLARE v_tid BIGINT;
BEGIN
    SELECT t.id INTO v_tid FROM laboratory.mst_lab_exam_template t
    JOIN inventory.mst_service s ON t.service_id = s.id WHERE s.sku_code = 'LAB-TPT' LIMIT 1;
    IF v_tid IS NOT NULL THEN
        INSERT INTO laboratory.det_lab_exam_template_item
            (template_id, analyte_name, unit, value_type, ref_min, ref_max, sort_order, is_required)
        VALUES
            (v_tid, 'TP (Tiempo de Protrombina)', 'segundos', 'numeric', 11.0, 14.0, 1, true),
            (v_tid, 'INR',                         'ratio',    'numeric',  0.8,  1.2, 2, true),
            (v_tid, 'TPTa (KPTT)',                'segundos', 'numeric', 25.0, 40.0, 3, true)
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- Coproparasitoscópico
DO $$ DECLARE v_tid BIGINT;
BEGIN
    SELECT t.id INTO v_tid FROM laboratory.mst_lab_exam_template t
    JOIN inventory.mst_service s ON t.service_id = s.id WHERE s.sku_code = 'LAB-EGH' LIMIT 1;
    IF v_tid IS NOT NULL THEN
        INSERT INTO laboratory.det_lab_exam_template_item
            (template_id, analyte_name, unit, value_type, ref_options, sort_order, is_required)
        VALUES
            (v_tid, 'Color',               NULL, 'qualitative', '["Café","Verde","Amarillo","Negro","Rojo"]'::jsonb,                  1, true),
            (v_tid, 'Consistencia',        NULL, 'qualitative', '["Sólido","Blando","Líquido","Pastoso"]'::jsonb,                     2, true),
            (v_tid, 'Leucocitos',          NULL, 'qualitative', '["Negativo","Escasos","Moderados","Abundantes"]'::jsonb,             3, true),
            (v_tid, 'Eritrocitos',         NULL, 'qualitative', '["Negativo","Escasos","Moderados","Abundantes"]'::jsonb,             4, true),
            (v_tid, 'Quistes / Trofozoítos', NULL, 'text',      NULL,                                                                5, true),
            (v_tid, 'Huevos de helmintos', NULL, 'text',        NULL,                                                                6, true)
        ON CONFLICT DO NOTHING;
    END IF;
END $$;
```

### BLOQUE D — Acciones nuevas en `security.mst_action`

```sql
INSERT INTO security.mst_action (id, module_id, name) VALUES
(8015, 8, 'Lab.Template.GetOrg'),
(8016, 8, 'Lab.Template.GetWithConfig'),
(8017, 8, 'Lab.Template.ResetToDefault'),
(8018, 8, 'Lab.Template.GetGlobal')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name, module_id = EXCLUDED.module_id;

-- Asignar a Admin-Laboratory automáticamente
INSERT INTO security.map_role_action (role_id, action_id)
SELECT r.id, a.id
FROM security.mst_role r
JOIN security.mst_action a ON a.id IN (8015, 8016, 8017, 8018)
WHERE r.name = 'Admin-Laboratory'
ON CONFLICT DO NOTHING;
```

### BLOQUE E — Índices

```sql
CREATE INDEX IF NOT EXISTS idx_org_lab_config_org_service
    ON laboratory.cfg_org_lab_exam_config(organization_id, service_id)
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_org_lab_item_config_custom
    ON laboratory.cfg_org_lab_exam_item_config(org_config_id)
    WHERE is_active = TRUE
      AND (custom_name IS NOT NULL OR custom_ref_min IS NOT NULL OR is_hidden = TRUE);

CREATE INDEX IF NOT EXISTS idx_lab_template_category
    ON laboratory.mst_lab_exam_template(category)
    WHERE is_published = TRUE AND is_active = TRUE;
```

---

## 11. Catálogo de plantillas globales — estado final

| ID | Plantilla | Categoría | SKU | Analitos | Estado |
|---|---|---|---|---|---|
| 1 | Hemograma Completo Estándar | Hematología | LAB-HEM | 5 + rangos M/F | Sembrado 016 |
| 2 | Perfil Lipídico Estándar | Química | LAB-LIP | 4 | Sembrado 016 |
| 3 | Química Básica Estándar | Química | LAB-QUI | 4 + rangos M/F | Sembrado 016 |
| 4 | Examen General de Orina | Orina | LAB-URI | 6 mix num+cualit | Sembrado 016 |
| auto | TSH Estándar | Endocrinología | LAB-TSH | 3 | **NUEVO 018** |
| auto | PCR Estándar | Química | LAB-PCR | 1 | **NUEVO 018** |
| auto | HbA1c Estándar | Endocrinología | LAB-HBA1 | 2 | **NUEVO 018** |
| auto | Coagulación PT/PTT | Hematología | LAB-TPT | 3 | **NUEVO 018** |
| auto | Coproparasitoscópico | Microbiología | LAB-EGH | 6 cualit+texto | **NUEVO 018** |

> Los laboratorios pueden personalizar cualquiera sin afectar a otros. Si no personalizan, `GetTemplateWithConfigAsync` usa los valores globales automáticamente.

---

## 12. Checklist para Antigravity

### Fase 1 — Constantes
- [ ] Agregar constantes `8015–8018` en `AppAction.Laboratory.cs`
- [ ] Eliminar las constantes `string` `ManageSampleConfig` y `ViewSampleConfig`

### Fase 2 — Entidades
- [ ] Agregar `is_active` y `updated_at` en `OrgLabExamConfig.cs`
- [ ] Agregar `is_active` en `OrgLabExamItemConfig.cs`
- [ ] Agregar propiedad `Category` en `LabExamTemplate.cs`

### Fase 3 — DTOs Request (corregir existentes + crear nuevos)
- [ ] Reemplazar `CloneTemplateRequestDTO.cs` → record + `[ActionMapping]` + validador
- [ ] Reemplazar `UpdateLabConfigItemRequestDTO.cs` → record + `[ActionMapping]` + validador
- [ ] Reemplazar `SaveStructuredResultRequestDTO.cs` → record + `[ActionMapping]` + validador
- [ ] Crear `GetOrgTemplatesRequestDTO.cs`
- [ ] Crear `GetTemplateWithConfigRequestDTO.cs` con validador
- [ ] Crear `ResetTemplateToDefaultRequestDTO.cs`
- [ ] Crear `GetGlobalTemplatesRequestDTO.cs`

### Fase 4 — DTOs Response
- [ ] Crear `TemplateResponseDTOs.cs` con los 4 records (§5)

### Fase 5 — Repositorios
- [ ] Agregar `GetAllPublishedAsync` y `GetItemCountAsync` en interfaz e implementación de `ILabExamTemplateRepository`
- [ ] Agregar `GetByOrganizationAsync`, `GetByIdAndOrgAsync`, `ResetItemsToDefaultAsync`, `HasCustomizationsAsync` en interfaz e implementación de `IOrgLabConfigRepository`
- [ ] Agregar `GetConfigByOrgAndServiceAsync` en `IOrgLabConfigRepository` para corregir `SaveStructuredResultAsync`

### Fase 6 — Servicio
- [ ] Agregar 4 firmas nuevas en `ILaboratoryService`
- [ ] Implementar `GetGlobalTemplatesAsync`
- [ ] Implementar `GetOrgTemplatesAsync`
- [ ] Implementar `GetTemplateWithConfigAsync` *(método central — ver §7.4)*
- [ ] Implementar `ResetTemplateToDefaultAsync`
- [ ] Corregir `orgConfigId` en `SaveStructuredResultAsync` *(ver §7.6)*

### Fase 7 — Domain y serialización
- [ ] Registrar los **7 handlers** en `LaboratoryDomain` (3 existentes + 4 nuevos)
- [ ] Agregar atributos `[JsonSerializable]` en `LaboratoryJsonContext`

### Fase 8 — Base de datos
- [ ] Ejecutar `018_Lab_Templates_Complete.sql` (Bloques A–E en orden)
- [ ] Verificar: `SELECT id, name, category FROM laboratory.mst_lab_exam_template ORDER BY id;`
- [ ] Verificar: `SELECT * FROM security.mst_action WHERE id BETWEEN 8015 AND 8018;`

### Fase 9 — Tests
- [ ] Test `GetTemplateWithConfigAsync` con org sin clon (usa valores globales)
- [ ] Test `GetTemplateWithConfigAsync` con config personalizada
- [ ] Test `GetTemplateWithConfigAsync` con `patient_id` para rangos demográficos
- [ ] Test `ResetTemplateToDefaultAsync` → valores custom quedan en `NULL`
- [ ] Test `CloneTemplateAsync` → error si ya existe config para esa plantilla

---

## 13. Mapa de archivos afectados

| Archivo | Acción | Sección |
|---|---|---|
| `src/Domain/Const/AppAction.Laboratory.cs` | Modificar | §2 |
| `src/Domain/Entities/Laboratory/LabExamTemplate.cs` | Modificar | §3.3 |
| `src/Domain/Entities/Laboratory/OrgLabExamConfig.cs` | Modificar | §3.1 |
| `src/Domain/Entities/Laboratory/OrgLabExamItemConfig.cs` | Modificar | §3.2 |
| `src/Domain/Interfaces/Repositories/Laboratory/ILabExamTemplateRepository.cs` | Modificar | §6.1 |
| `src/Domain/Interfaces/Repositories/Laboratory/IOrgLabConfigRepository.cs` | Modificar | §6.2 |
| `src/Infrastructure/Persistence/Repositories/Laboratory/LabExamTemplateRepository.cs` | Modificar | §6.1 |
| `src/Infrastructure/Persistence/Repositories/Laboratory/OrgLabConfigRepository.cs` | Modificar | §6.2 |
| `src/Application/Features/Laboratory/Dtos/Request/CloneTemplateRequestDTO.cs` | Modificar | §4.1 |
| `src/Application/Features/Laboratory/Dtos/Request/UpdateLabConfigItemRequestDTO.cs` | Modificar | §4.2 |
| `src/Application/Features/Laboratory/Dtos/Request/SaveStructuredResultRequestDTO.cs` | Modificar | §4.3 |
| `src/Application/Features/Laboratory/Dtos/Request/GetOrgTemplatesRequestDTO.cs` | **CREAR** | §4.4 |
| `src/Application/Features/Laboratory/Dtos/Request/GetTemplateWithConfigRequestDTO.cs` | **CREAR** | §4.5 |
| `src/Application/Features/Laboratory/Dtos/Request/ResetTemplateToDefaultRequestDTO.cs` | **CREAR** | §4.6 |
| `src/Application/Features/Laboratory/Dtos/Request/GetGlobalTemplatesRequestDTO.cs` | **CREAR** | §4.7 |
| `src/Application/Features/Laboratory/Dtos/Response/TemplateResponseDTOs.cs` | **CREAR** | §5 |
| `src/Application/Features/Laboratory/Interfaces/ILaboratoryService.cs` | Modificar | §7.1 |
| `src/Application/Features/Laboratory/Services/LaboratoryService.cs` | Modificar | §7.2–7.6 |
| `src/Application/Features/Laboratory/Domain/LaboratoryDomain.cs` | Modificar | §8 |
| `src/Application/Features/Laboratory/Serialization/LaboratoryJsonContext.cs` | Modificar | §9 |
| `src/Migrations/Scripts/018_Lab_Templates_Complete.sql` | **CREAR** | §10 |

> **⚠️ Prioridad crítica:** Registrar los 3 handlers en `LaboratoryDomain` (§8) para `CloneTemplate`, `UpdateConfig` y `SaveStructuredResult`. Estos ya tienen implementación completa — solo falta la conexión al dispatcher.

---

*Fin del documento — MedfarLabs Core · Plantillas y Resultados de Laboratorio*
