# MedfarLabs · Análisis del Módulo de Facturación al Paciente
> Factura del médico → paciente · Pago directo en efectivo o tarjeta  
> Mayo 2025

---

## 0. Resumen ejecutivo

El módulo de facturación al paciente **existe y tiene buena base estructural**, pero tiene brechas importantes que lo hacen incompleto para producción. El flujo central (crear factura → registrar pago → imprimir) está trazado, pero con lógica frágil, estados inconsistentes, validadores vacíos y gaps que generarían problemas reales de operación diaria para el médico.

| Capa | Estado |
|---|---|
| Entidades de dominio (`Invoice`, `InvoiceItem`, `Payment`) | ✅ Base sólida, faltan 4 campos clave |
| Flujo crear factura → pagar | ✅ Funciona, con bugs de lógica importantes |
| Estados de factura | ⚠️ Enum tiene 4 estados pero el servicio solo usa 2 |
| Métodos de pago | ⚠️ El código dice "3: Insurance" — irrelevante para el scope actual |
| Validadores | 🔴 Vacíos con TODO en 4 de 6 clases |
| Búsqueda y listado | 🔴 `GetAllInvoices` hace `GetAllAsync()` sin filtro SQL — carga toda la tabla |
| Pago parcial / cambio | 🔴 No existe. El pago siempre marca la factura como pagada al 100% |
| Discount / descuento | 🔴 No existe en entidad ni en flujo |
| Consulta → Factura automática | 🔴 No hay conexión entre cerrar una consulta y generar su factura |
| Impresión / PDF | ✅ Templates A4, Modern y Ticket ya implementados con QuestPDF |
| `BuscarFactura` y `CancelInvoiceRequestDTO` | 🔴 No registrados en `BillingDomain` — el dispatcher los ignora |

---

## 1. Bugs críticos a corregir hoy

### 1.1 `GetAllInvoices` carga toda la tabla en memoria

**Archivo:** `BillingService.cs` — método `GetAllInvoicesAsync`

```csharp
// ACTUAL — carga TODOS los registros de la tabla sin WHERE
var invoices = await _unitOfWork.Invoices.GetAllAsync();
var filteredInvoices = invoices.Where(i => i.OrganizationId == request.OrganizationId)...
```

Esto es un problema de rendimiento grave. Con 10,000 facturas en la tabla de 50 organizaciones, cada llamada carga los 10,000 registros para filtrar en memoria. Hay que mover el filtro a SQL.

**Corrección:** Agregar `GetByOrganizationAsync` en `IInvoiceRepository`:

```csharp
Task<IEnumerable<Invoice>> GetByOrganizationAsync(long organizationId, int page, int pageSize,
    DateTime? from = null, DateTime? to = null, int? statusId = null);
```

### 1.2 `RegistrarPago` marca la factura como Pagada sin verificar el monto

**Archivo:** `BillingService.cs` — método `RegistrarPagoAsync`

```csharp
// ACTUAL — marca como Pagada (status 2) sin importar si AmountPaid == TotalAmount
invoice.StatusId = 2;
await _unitOfWork.Invoices.UpdateAsync(invoice);
```

Si el médico registra un pago de $10 sobre una factura de $100, el sistema la marca como "Pagada". No hay verificación del monto. No existe concepto de pago parcial ni de saldo pendiente.

### 1.3 `ActualizarFactura` borra items con `GetAllAsync()` sin filtro SQL

**Archivo:** `BillingService.cs` — método `ActualizarFacturaAsync`

```csharp
// ACTUAL — carga TODOS los InvoiceItems de la tabla para filtrar en memoria
var allItems = await _unitOfWork.InvoiceItems.GetAllAsync();
var oldItems = allItems.Where(i => i.InvoiceId == request.InvoiceId).ToList();
```

Misma falla que el punto anterior. Necesita `GetByInvoiceIdAsync(long invoiceId)` en `IInvoiceItemRepository`.

### 1.4 `BuscarFactura` y `AnularFactura` no están en `BillingDomain`

**Archivo:** `BillingDomain.cs`

```csharp
// FALTAN estos dos en el Domain — el dispatcher los ignora completamente:
RegisterActionHandler<IBillingService>(AppAction.Billling.BuscarFactura,
    nameof(IBillingService.BuscarFacturaAsync));    // método no existe en servicio
RegisterActionHandler<IBillingService>(AppAction.Billling.AnularFactura,
    nameof(IBillingService.AnularFacturaAsync));    // existe pero no está registrado
```

`AnularFactura` tiene implementación completa pero nunca llega al dispatcher. `BuscarFactura` tiene acción (3005) y DTO pero no tiene método en `IBillingService`.

### 1.5 Estados de factura inconsistentes entre enum y servicio

El enum `InvoiceStatusEnum` define: `Draft=1`, `Unpaid=2`, `Paid=3`, `Void=4`.

El servicio usa: `StatusId = 1` para emitida/pendiente y `StatusId = 2` para pagada — al revés del enum. `GenerarFactura` asigna `StatusId = 1` (Draft) en lugar de `StatusId = 2` (Unpaid). `AnularFactura` asigna `StatusId = 3` (Paid) en lugar de `StatusId = 4` (Void).

Hay que alinear el servicio con el enum o corregir el enum. Lo más limpio es corregir el servicio para usar el enum directamente:

```csharp
// Correcto:
StatusId = (int)InvoiceStatusEnum.Unpaid   // al crear
StatusId = (int)InvoiceStatusEnum.Paid     // al pagar
StatusId = (int)InvoiceStatusEnum.Void     // al anular
```

---

## 2. Campos faltantes en entidades

### 2.1 `Invoice.cs` — le faltan 5 campos que sí existen en la tabla SQL

La tabla `billing.mst_invoice` tiene más columnas que la entidad C#:

```csharp
// Agregar en Invoice.cs:
[DbColumn("branch_id")]
public long BranchId { get; set; }            // ¿En qué sucursal se emitió?

[DbColumn("consultation_id")]
public long? ConsultationId { get; set; }      // Factura ligada a una consulta específica

[DbColumn("discount_amount")]
public decimal DiscountAmount { get; set; }    // Descuento aplicado

[DbColumn("notes")]
public string? Notes { get; set; }             // Notas visibles al paciente en la factura

[DbColumn("paid_at")]
public DateTime? PaidAt { get; set; }          // Fecha real de pago
```

### 2.2 `Payment.cs` — le faltan campos de trazabilidad

```csharp
// Agregar en Payment.cs:
[DbColumn("notes")]
public string? Notes { get; set; }             // Notas del cajero / recepcionista

[DbColumn("change_given")]
public decimal ChangeGiven { get; set; }       // Cambio dado al paciente (efectivo)
```

### 2.3 `InvoiceItem.cs` — le falta el nombre libre del concepto

Actualmente `InvoiceItem` solo referencia `service_id`. Esto obliga a que todo ítem venga del catálogo de servicios. Hay casos donde el médico quiere escribir un concepto libre ("Procedimiento especial", "Material quirúrgico", etc.):

```csharp
// Agregar en InvoiceItem.cs:
[DbColumn("description_override")]
public string? DescriptionOverride { get; set; }  // Nombre libre, usa ServiceName si es NULL

[DbColumn("discount_pct")]
public decimal? DiscountPct { get; set; }           // % de descuento en esta línea
```

---

## 3. Validadores vacíos — implementar

### 3.1 `InvoiceItemRules`

```csharp
public class InvoiceItemRules : AbstractValidator<InvoiceItemRequestDTO>
{
    public InvoiceItemRules()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("Servicio inválido.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
    }
}
```

### 3.2 `PaymentRules`

```csharp
public class PaymentRules : AbstractValidator<PaymentRequestDTO>
{
    public PaymentRules()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("Factura inválida.");
        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("El monto pagado debe ser mayor a cero.");
        RuleFor(x => x.PaymentMethodId)
            .InclusiveBetween(1, 2).WithMessage("Método de pago inválido.")
            // Solo efectivo (1) o tarjeta (2) — el sistema no maneja seguros
            ;
    }
}
```

### 3.3 `CancelInvoiceRules`

```csharp
public class CancelInvoiceRules : AbstractValidator<CancelInvoiceRequestDTO>
{
    public CancelInvoiceRules()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("ID de factura inválido.");
        RuleFor(x => x.CancelReason)
            .NotEmpty().WithMessage("Debe indicar el motivo de anulación.")
            .MaximumLength(300);
    }
}
```

Esto implica agregar `cancel_reason` a `CancelInvoiceRequestDTO` y a la entidad `Invoice`.

### 3.4 `GetAllInvoicesRules` y `GetInvoiceByIdRules`

```csharp
public class GetAllInvoicesRules : AbstractValidator<GetAllInvoicesRequestDTO>
{
    public GetAllInvoicesRules()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetInvoiceByIdRules : AbstractValidator<GetInvoiceByIdRequestDTO>
{
    public GetInvoiceByIdRules()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("ID de factura inválido.");
    }
}
```

---

## 4. Mejoras funcionales necesarias

### 4.1 Pago parcial y saldo pendiente

El flujo real de una clínica privada en Nicaragua incluye frecuentemente abonos: el paciente paga la mitad hoy y el resto la próxima semana. Actualmente el sistema no lo soporta.

**Cambios necesarios:**

En `Invoice` agregar:
```csharp
[DbColumn("amount_paid")]   public decimal AmountPaid { get; set; }   // Suma de pagos recibidos
[DbColumn("balance_due")]   public decimal BalanceDue { get; set; }   // TotalAmount - AmountPaid
```

En `RegistrarPagoAsync` corregir la lógica:
```csharp
// Sumar el pago al acumulado
invoice.AmountPaid += request.AmountPaid;
invoice.BalanceDue  = invoice.TotalAmount - invoice.AmountPaid;

// Solo marcar como Pagada si el saldo queda en cero
if (invoice.BalanceDue <= 0)
{
    invoice.StatusId = (int)InvoiceStatusEnum.Paid;
    invoice.PaidAt = DateTime.UtcNow;
}
// Si hay saldo pendiente, puede quedar en estado PartiallyPaid (nuevo estado)
```

Agregar al enum `InvoiceStatusEnum`:
```csharp
[Display(Name = "Pago Parcial")]
PartiallyPaid = 5
```

### 4.2 Descuento a nivel de factura

Muchos médicos privados aplican descuentos por cortesía, convenio con empresa, o paciente frecuente.

En `InvoiceRequestDTO` agregar:
```csharp
[JsonPropertyName("discount_amount")]
public decimal DiscountAmount { get; init; }        // Descuento fijo en $

[JsonPropertyName("discount_pct")]
public decimal? DiscountPct { get; init; }           // O bien un % sobre el subtotal

[JsonPropertyName("discount_reason")]
public string? DiscountReason { get; init; }         // Motivo (cortesía, convenio, etc.)
```

En `GenerarFacturaAsync` ajustar el cálculo:
```csharp
var subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
var discount = request.DiscountAmount > 0
    ? request.DiscountAmount
    : subtotal * (request.DiscountPct ?? 0) / 100;
var totalBeforeTax = subtotal - discount;
var tax = totalBeforeTax * (request.Tax / 100);
var total = totalBeforeTax + tax;
```

### 4.3 Conexión directa consulta → factura

Hoy la factura no tiene referencia a la consulta que originó el cobro. Esto impide saber qué consulta generó qué factura, duplicar cobros por la misma consulta, o pre-llenar la factura automáticamente desde el cierre de la consulta.

**Cambio mínimo:** Agregar `consultation_id` en `billing.mst_invoice` (ya propuesto en §2.1) y en el `InvoiceRequestDTO`:

```csharp
// En InvoiceRequestDTO agregar:
[JsonPropertyName("consultation_id")]
public long? ConsultationId { get; init; }
```

**Flujo mejorado:** Al cerrar una consulta (`Care` module), lanzar un evento que pre-genere un borrador de factura con el servicio "Consulta médica" ya como ítem, listo para que el recepcionista confirme y cobre.

### 4.4 `BuscarFactura` — agregar implementación en servicio

El action `3005` existe, el DTO existe, pero no hay método en `IBillingService`. Agregar:

En `IBillingService`:
```csharp
Task<BaseResponse<IEnumerable<SearchInvoiceResponseDTO>>> BuscarFacturaAsync(SearchInvoiceRequestDTO request);
```

En `BillingService`:
```csharp
public async Task<BaseResponse<IEnumerable<SearchInvoiceResponseDTO>>> BuscarFacturaAsync(SearchInvoiceRequestDTO request)
{
    var invoices = await _unitOfWork.Invoices.SearchAsync(
        request.OrganizationId,
        request.BranchId,
        request.PatientId,
        request.StartDate,
        request.EndDate);

    var dtos = invoices.Select(i => new SearchInvoiceResponseDTO
    {
        Id            = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        PatientName   = i.PatientName,   // Resuelto por el repo con JOIN
        TotalAmount   = i.TotalAmount,
        StatusName    = i.StatusName,    // Resuelto por el repo con JOIN
        CreatedAt     = i.CreatedAt
    });

    return BaseResponse<IEnumerable<SearchInvoiceResponseDTO>>.Success(dtos);
}
```

En `IInvoiceRepository` agregar el método de búsqueda con JOIN:
```csharp
Task<IEnumerable<InvoiceSearchResult>> SearchAsync(
    long organizationId, long? branchId = null, long? patientId = null,
    DateTime? from = null, DateTime? to = null, int? statusId = null,
    int page = 1, int pageSize = 20);
```

### 4.5 `SearchInvoiceRequestDTO` — extender filtros

Actualmente solo filtra por fecha y `patient_id`. Para una clínica, hace falta también filtrar por estado y por número de factura:

```csharp
// Agregar en SearchInvoiceRequestDTO:
[JsonPropertyName("status_id")]
public int? StatusId { get; init; }

[JsonPropertyName("invoice_number")]
public string? InvoiceNumber { get; init; }

[JsonPropertyName("page")]
public int Page { get; init; } = 1;

[JsonPropertyName("page_size")]
public int PageSize { get; init; } = 20;
```

### 4.6 Cambio de moneda al pagar en efectivo

En Nicaragua, muchos médicos cobran en dólares pero el paciente paga en córdobas. Falta registrar:

```csharp
// En PaymentRequestDTO agregar:
[JsonPropertyName("currency")]
public string Currency { get; init; } = "USD";    // "USD" o "NIO"

[JsonPropertyName("exchange_rate")]
public decimal ExchangeRate { get; init; } = 1;   // Tasa al momento del pago

[JsonPropertyName("amount_in_local_currency")]
public decimal? AmountInLocalCurrency { get; init; }  // Monto equivalente en NIO
```

Esto permite al médico registrar "El paciente pagó C$3,640 equivalente a $100 dólares a una tasa de 36.40".

---

## 5. Mejoras en el módulo de impresión

### 5.1 Campos faltantes en `InvoiceReportModel`

El modelo de reporte ya existe pero le faltan campos para imprimir una factura completa:

```csharp
// Agregar en InvoiceReportModel:
public string? DoctorName { get; set; }        // "Dr. Juan Pérez"
public string? DoctorSpecialty { get; set; }   // "Médico General"
public string? ConsultationDate { get; set; }  // Fecha de la consulta
public decimal DiscountAmount { get; set; }    // Descuento aplicado
public decimal AmountPaid { get; set; }        // Lo que pagó
public decimal BalanceDue { get; set; }        // Lo que falta
public string? PaymentMethodName { get; set; } // "Efectivo" / "Tarjeta"
public string? Notes { get; set; }             // Notas visibles al paciente
public bool IsPartialPayment { get; set; }     // Para mostrar "ABONO" en vez de "PAGADA"
```

### 5.2 `InvoiceReportRules` — implementar validador vacío

```csharp
public class InvoiceReportRules : AbstractValidator<InvoiceReportRequestDTO>
{
    public InvoiceReportRules()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("ID de factura inválido.");
    }
}
```

---

## 6. Acciones faltantes en `BillingDomain`

**Archivo:** `src/Application/Features/Billing/Domain/BillingDomain.cs`

Registrar los que faltan:

```csharp
// Existente pero no registrado:
RegisterActionHandler<IBillingService>(AppAction.Billling.AnularFactura,
    nameof(IBillingService.AnularFacturaAsync));

// Existente sin implementación en servicio (agregar en §4.4):
RegisterActionHandler<IBillingService>(AppAction.Billling.BuscarFactura,
    nameof(IBillingService.BuscarFacturaAsync));
```

---

## 7. Nuevas constantes necesarias en `AppAction.Billling.cs`

```csharp
public const int GetInvoicePayments  = 3020; // Historial de pagos de una factura
public const int GetPatientBalance   = 3021; // Saldo total pendiente de un paciente
public const int GetDailyClosing     = 3022; // Cierre de caja del día (suma por método de pago)
```

Estas tres operaciones son las que cualquier recepcionista necesita al final del día para cuadrar caja.

---

## 8. Script SQL — `019_Invoice_Improvements.sql`

```sql
-- BLOQUE A: Columnas faltantes en billing.mst_invoice
ALTER TABLE billing.mst_invoice
    ADD COLUMN IF NOT EXISTS branch_id          BIGINT REFERENCES identity.mst_branch(id),
    ADD COLUMN IF NOT EXISTS consultation_id    BIGINT REFERENCES care.mst_consultation(id),
    ADD COLUMN IF NOT EXISTS discount_amount    DECIMAL(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_reason    VARCHAR(300) NULL,
    ADD COLUMN IF NOT EXISTS amount_paid        DECIMAL(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS balance_due        DECIMAL(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS notes              TEXT NULL,
    ADD COLUMN IF NOT EXISTS paid_at            TIMESTAMP NULL,
    ADD COLUMN IF NOT EXISTS cancel_reason      VARCHAR(300) NULL;

-- BLOQUE B: Columnas faltantes en billing.det_invoice_item
ALTER TABLE billing.det_invoice_item
    ADD COLUMN IF NOT EXISTS description_override VARCHAR(200) NULL,
    ADD COLUMN IF NOT EXISTS discount_pct         DECIMAL(5,2) NULL;

-- BLOQUE C: Columnas faltantes en billing.mst_payments
ALTER TABLE billing.mst_payments
    ADD COLUMN IF NOT EXISTS notes                   TEXT NULL,
    ADD COLUMN IF NOT EXISTS change_given            DECIMAL(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS currency                CHAR(3) NOT NULL DEFAULT 'USD',
    ADD COLUMN IF NOT EXISTS exchange_rate           DECIMAL(10,4) NOT NULL DEFAULT 1,
    ADD COLUMN IF NOT EXISTS amount_in_local_currency DECIMAL(18,2) NULL;

-- BLOQUE D: Estado PartiallyPaid si usas catálogo común
-- Si status_id viene de common.mst_catalog_detail, insertar ahí.
-- Si usas el enum directamente, no requiere script SQL.

-- BLOQUE E: Nuevas acciones
INSERT INTO security.mst_action (id, module_id, name) VALUES
(3020, 3, 'Billing.Invoice.GetPayments'),
(3021, 3, 'Billing.Invoice.GetPatientBalance'),
(3022, 3, 'Billing.Invoice.GetDailyClosing')
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- BLOQUE F: Índices de rendimiento
CREATE INDEX IF NOT EXISTS idx_invoice_org_status_date
    ON billing.mst_invoice(organization_id, status_id, created_at DESC)
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_invoice_patient
    ON billing.mst_invoice(patient_id, organization_id)
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_invoice_consultation
    ON billing.mst_invoice(consultation_id)
    WHERE consultation_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_invoice
    ON billing.mst_payments(invoice_id, organization_id);
```

---

## 9. Checklist priorizado para Antigravity

### 🔴 Prioridad CRÍTICA — bugs activos
- [ ] Registrar `AnularFactura` en `BillingDomain` (1 línea, ya tiene implementación)
- [ ] Corregir `GetAllInvoices` — reemplazar `GetAllAsync()` por query SQL con filtro `WHERE organization_id`
- [ ] Corregir `ActualizarFactura` — reemplazar `GetAllAsync()` por `GetByInvoiceIdAsync`
- [ ] Alinear estados del servicio con el enum `InvoiceStatusEnum` (Draft/Unpaid/Paid/Void)
- [ ] Eliminar referencia a método de pago "3: Insurance" del comentario en `PaymentRequestDTO`

### 🟡 Prioridad ALTA — funcionalidad incompleta
- [ ] Implementar validadores: `InvoiceItemRules`, `PaymentRules`, `CancelInvoiceRules`, `GetInvoiceByIdRules`
- [ ] Agregar `cancel_reason` a `CancelInvoiceRequestDTO` y a `Invoice.cs`
- [ ] Agregar `consultation_id`, `discount_amount`, `amount_paid`, `balance_due`, `paid_at`, `notes` a `Invoice.cs`
- [ ] Implementar pago parcial en `RegistrarPagoAsync` con cálculo de `BalanceDue`
- [ ] Agregar `BuscarFacturaAsync` en `IBillingService` + implementación + registro en Domain
- [ ] Agregar `description_override` a `InvoiceItem.cs` para conceptos libres
- [ ] Ejecutar `019_Invoice_Improvements.sql`

### 🟢 Prioridad NORMAL — mejora de experiencia
- [ ] Agregar descuento a `InvoiceRequestDTO`, entidad y lógica de cálculo
- [ ] Agregar campos de moneda/tasa de cambio a `PaymentRequestDTO`
- [ ] Extender `SearchInvoiceRequestDTO` con filtros de estado, número y paginación
- [ ] Agregar `GetInvoicePayments`, `GetPatientBalance`, `GetDailyClosing` (constantes + métodos)
- [ ] Agregar campos faltantes en `InvoiceReportModel` (doctor, descuento, saldo)
- [ ] Agregar `consultation_id` a `InvoiceRequestDTO` para ligar factura a consulta
- [ ] Implementar `InvoiceReportRules`

---

## 10. Mapa de archivos afectados

| Archivo | Acción | Prioridad |
|---|---|---|
| `src/Domain/Entities/Billing/Invoice.cs` | Modificar — 5 campos nuevos | Alta |
| `src/Domain/Entities/Billing/InvoiceItem.cs` | Modificar — 2 campos nuevos | Normal |
| `src/Domain/Entities/Billing/Payment.cs` | Modificar — 4 campos nuevos | Normal |
| `src/Domain/Enums/Billing/InvoiceStatusEnum.cs` | Modificar — agregar `PartiallyPaid = 5` | Alta |
| `src/Domain/Const/AppAction.Billling.cs` | Modificar — agregar 3020, 3021, 3022 | Normal |
| `src/Domain/Interfaces/Repositories/Billing/IInvoiceRepository.cs` | Modificar — `GetByOrganizationAsync`, `SearchAsync` | Crítica |
| `src/Domain/Interfaces/Repositories/Billing/IInvoiceItemRepository.cs` | Modificar — `GetByInvoiceIdAsync` | Crítica |
| `src/Domain/Interfaces/Repositories/Billing/IPaymentRepository.cs` | Sin cambio | — |
| `src/Infrastructure/Persistence/Repositories/Billing/InvoiceRepository.cs` | Implementar métodos nuevos | Crítica |
| `src/Infrastructure/Persistence/Repositories/Billing/InvoiceItemRepository.cs` | Implementar `GetByInvoiceIdAsync` | Crítica |
| `src/Application/Features/Billing/Dtos/Request/InvoiceRequestDTO.cs` | Modificar — discount, consultation_id | Alta |
| `src/Application/Features/Billing/Dtos/Request/InvoiceItemRequestDTO.cs` | Modificar — description_override | Normal |
| `src/Application/Features/Billing/Dtos/Request/PaymentRequestDTO.cs` | Modificar — currency, change_given | Normal |
| `src/Application/Features/Billing/Dtos/Request/CancelInvoiceRequestDTO.cs` | Modificar — cancel_reason, record | Alta |
| `src/Application/Features/Billing/Dtos/Request/GetAllInvoicesRequestDTO.cs` | Modificar — page, page_size, filtros | Alta |
| `src/Application/Features/Billing/Dtos/Request/SearchInvoiceRequestDTO.cs` | Modificar — statusId, invoiceNumber, paginación | Alta |
| `src/Application/Features/Billing/Interfaces/IBillingService.cs` | Modificar — agregar `BuscarFacturaAsync` | Alta |
| `src/Application/Features/Billing/Services/BillingService.cs` | Modificar — 4 correcciones + nuevos métodos | Crítica |
| `src/Application/Features/Billing/Domain/BillingDomain.cs` | Modificar — registrar 2 handlers | Crítica |
| `src/Domain/Models/Reporting/InvoiceReportModel.cs` | Modificar — 8 campos nuevos | Normal |
| `src/Reporting/Templates/Invoice/InvoiceA4Template.cs` | Modificar — mostrar descuento y saldo | Normal |
| `src/Migrations/Scripts/019_Invoice_Improvements.sql` | CREAR | Alta |

---

*Fin del análisis — MedfarLabs Core · Módulo de Facturación al Paciente*
