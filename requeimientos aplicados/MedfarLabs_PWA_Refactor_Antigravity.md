# MedfarLabs · PWA — Refactorización Total UI/UX
## Propuesta completa para Antigravity · Estándar Elite
> Blazor WebAssembly + MudBlazor · Mayo 2025

---

## 0. Resumen ejecutivo

Este documento define la refactorización completa de la PWA de MedfarLabs. El objetivo es transformarla de una interfaz funcional a un sistema de grado médico premium, aplicando los 6 estándares definidos en los skills:

| Estándar | Estado actual | Estado objetivo |
|---|---|---|
| **Componentes base** | `MudTable` suelto por página | `MedFarTable` centralizado |
| **Estados de carga** | `MudProgressCircular` genérico | `MedFarSkeleton` por layout |
| **Datos vacíos** | Mensaje por defecto de Mud | `MedFarEmptyState` con acción |
| **Arquitectura** | Lógica mezclada en `.razor` | Triple archivo obligatorio |
| **Estilo visual** | Superficies planas | Glassmorphism + gradientes |
| **Seguridad clínica** | Sin Poka-Yoke implementado | Sticky alerts + bloqueos |

**Alcance:** 38 páginas a refactorizar, 17 componentes nuevos a crear, 4 fases de 2 semanas cada una.

---

## 1. Sistema de diseño — tokens y variables CSS globales

**Archivo:** `wwwroot/css/medfar-tokens.css`

```css
/* ═══════════════════════════════════════════════
   TOKENS GLOBALES — MedFarLab Design System
   Importar en app.css antes de cualquier otro css
═══════════════════════════════════════════════ */

:root {
  /* ── Colores primarios ── */
  --mf-primary:        #10B981;   /* Emerald 500 */
  --mf-primary-dark:   #059669;   /* Emerald 600 */
  --mf-primary-light:  #D1FAE5;   /* Emerald 100 */
  --mf-accent:         #6366F1;   /* Indigo 500 */

  /* ── Semánticos clínicos ── */
  --mf-danger:         #EF4444;   /* Red 500 — alergias, crítico */
  --mf-warning:        #F97316;   /* Orange 500 — precaución */
  --mf-success:        #22C55E;   /* Green 500 — normal */
  --mf-info:           #3B82F6;   /* Blue 500 — informativo */

  /* ── Superficies Light Mode ── */
  --mf-surface:        rgba(255, 255, 255, 0.75);
  --mf-surface-solid:  #FFFFFF;
  --mf-bg:             #F0FDF4;
  --mf-border:         rgba(0, 0, 0, 0.08);

  /* ── Glassmorphism ── */
  --mf-glass-bg:       rgba(255, 255, 255, 0.70);
  --mf-glass-blur:     12px;
  --mf-glass-border:   1px solid rgba(255, 255, 255, 0.50);

  /* ── Sombras flotantes ── */
  --mf-shadow-sm:      0 2px 8px rgba(16, 185, 129, 0.08);
  --mf-shadow-float:   0 8px 32px rgba(16, 185, 129, 0.15);
  --mf-shadow-modal:   0 24px 64px rgba(0, 0, 0, 0.20);

  /* ── Tipografía ── */
  --mf-font:           'Inter', -apple-system, sans-serif;
  --mf-font-mono:      'JetBrains Mono', monospace;

  /* ── Radios ── */
  --mf-radius-sm:      8px;
  --mf-radius-md:      12px;
  --mf-radius-lg:      16px;
  --mf-radius-xl:      24px;

  /* ── Transiciones ── */
  --mf-transition:     0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Dark Mode — detección automática + manual */
[data-theme="dark"], .dark-mode {
  --mf-surface:        rgba(30, 30, 46, 0.85);
  --mf-surface-solid:  #1E1E2E;
  --mf-bg:             #121218;
  --mf-border:         rgba(255, 255, 255, 0.08);
  --mf-glass-bg:       rgba(30, 30, 46, 0.80);
  --mf-glass-border:   1px solid rgba(255, 255, 255, 0.10);

  /* Primarios más brillantes en dark para accesibilidad */
  --mf-primary:        #4ADE80;
  --mf-primary-light:  rgba(74, 222, 128, 0.15);
}

/* ── Clases utilitarias globales ── */
.surface-glass {
  background:    var(--mf-glass-bg) !important;
  backdrop-filter: blur(var(--mf-glass-blur));
  -webkit-backdrop-filter: blur(var(--mf-glass-blur));
  border:        var(--mf-glass-border) !important;
  border-radius: var(--mf-radius-lg) !important;
}

.shadow-float {
  box-shadow: var(--mf-shadow-float) !important;
}

.btn-emerald-gradient {
  background:    linear-gradient(135deg, var(--mf-primary), var(--mf-accent)) !important;
  color:         white !important;
  border:        none !important;
  box-shadow:    var(--mf-shadow-sm) !important;
  transition:    var(--mf-transition) !important;
}

.btn-emerald-gradient:hover {
  transform:     translateY(-1px);
  box-shadow:    var(--mf-shadow-float) !important;
}

/* ── Clínico: colores de riesgo ── */
.clinical-critical { color: var(--mf-danger); font-weight: 700; }
.clinical-warning  { color: var(--mf-warning); font-weight: 600; }
.clinical-normal   { color: var(--mf-success); }
.lab-out-of-range  { color: var(--mf-danger); font-weight: 700; }
.lab-out-of-range::after { content: ' ↑'; }
.lab-below-range::after  { content: ' ↓'; color: var(--mf-info); font-weight: 700; }
```

---

## 2. Tema MudBlazor — configuración global

**Archivo:** `Services/MedFarThemeService.cs`

```csharp
using MudBlazor;

public static class MedFarTheme
{
    public static MudTheme Build() => new MudTheme
    {
        PaletteLight = new PaletteLight
        {
            Primary         = "#10B981",
            PrimaryDarken   = "#059669",
            PrimaryLighten  = "#D1FAE5",
            Secondary       = "#6366F1",
            Tertiary        = "#F97316",
            Background      = "#F0FDF4",
            Surface         = "#FFFFFF",
            AppbarBackground= "rgba(255,255,255,0.80)",
            DrawerBackground= "rgba(255,255,255,0.75)",
            Success         = "#22C55E",
            Warning         = "#F97316",
            Error           = "#EF4444",
            Info            = "#3B82F6",
            TextPrimary     = "#111827",
            TextSecondary   = "#6B7280"
        },
        PaletteDark = new PaletteDark
        {
            Primary         = "#4ADE80",
            PrimaryDarken   = "#16A34A",
            PrimaryLighten  = "rgba(74,222,128,0.15)",
            Secondary       = "#818CF8",
            Background      = "#121218",
            Surface         = "#1E1E2E",
            AppbarBackground= "rgba(30,30,46,0.85)",
            DrawerBackground= "rgba(30,30,46,0.80)",
            TextPrimary     = "#F9FAFB",
            TextSecondary   = "#9CA3AF"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "-apple-system", "sans-serif" },
                FontSize   = "0.875rem",
                LineHeight = "1.5"
            },
            H5 = new H5Typography { FontWeight = "700", FontSize = "1.125rem" },
            H6 = new H6Typography { FontWeight = "600", FontSize = "1rem" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft     = "260px",
            AppbarHeight        = "64px"
        }
    };
}
```

**Archivo:** `Program.cs` — aplicar el tema:

```csharp
// Reemplazar MudTheme genérico por:
builder.Services.AddSingleton(MedFarTheme.Build());
```

---

## 3. Componentes base — los 4 pilares

### 3.1 `MedFarSkeleton.razor` — el que faltaba implementar

**Archivo:** `Shared/Components/MedFarSkeleton.razor`

```razor
@using MudBlazor

@switch (Type)
{
    case SkeletonType.List:
        <div class="mf-skeleton-list">
            @for (int i = 0; i < Rows; i++)
            {
                <div class="mf-skeleton-row">
                    @if (ShowAvatar)
                    {
                        <MudSkeleton SkeletonType="SkeletonType.Circle"
                                     Width="40px" Height="40px" Class="mr-3" Animation="Animation.Wave"/>
                    }
                    <div class="flex-grow-1">
                        <MudSkeleton Width="@GetRowWidth(i)" Height="16px"
                                     Animation="Animation.Wave" Class="mb-1"/>
                        <MudSkeleton Width="60%" Height="12px"
                                     Animation="Animation.Wave"/>
                    </div>
                </div>
            }
        </div>
        break;

    case SkeletonType.Card:
        <MudCard Class="surface-glass shadow-float">
            <MudCardContent>
                <MudSkeleton Width="40%" Height="24px" Animation="Animation.Wave" Class="mb-3"/>
                <MudSkeleton Width="100%" Height="14px" Animation="Animation.Wave" Class="mb-2"/>
                <MudSkeleton Width="80%" Height="14px" Animation="Animation.Wave" Class="mb-2"/>
                <MudSkeleton Width="60%" Height="14px" Animation="Animation.Wave"/>
            </MudCardContent>
        </MudCard>
        break;

    case SkeletonType.Form:
        <div class="mf-skeleton-form">
            @for (int i = 0; i < Rows; i++)
            {
                <div class="mb-4">
                    <MudSkeleton Width="30%" Height="12px" Animation="Animation.Wave" Class="mb-2"/>
                    <MudSkeleton Width="100%" Height="40px" Animation="Animation.Wave"
                                 Style="border-radius: var(--mf-radius-sm)"/>
                </div>
            }
        </div>
        break;

    case SkeletonType.PatientHeader:
        <!-- Skeleton específico para la cabecera del expediente -->
        <div class="d-flex align-center gap-4 pa-4">
            <MudSkeleton SkeletonType="SkeletonType.Circle" Width="72px" Height="72px" Animation="Animation.Wave"/>
            <div class="flex-grow-1">
                <MudSkeleton Width="45%" Height="24px" Animation="Animation.Wave" Class="mb-2"/>
                <MudSkeleton Width="30%" Height="16px" Animation="Animation.Wave" Class="mb-1"/>
                <MudSkeleton Width="25%" Height="14px" Animation="Animation.Wave"/>
            </div>
            <div class="d-flex gap-2">
                <MudSkeleton Width="80px" Height="32px" Style="border-radius:16px" Animation="Animation.Wave"/>
                <MudSkeleton Width="80px" Height="32px" Style="border-radius:16px" Animation="Animation.Wave"/>
            </div>
        </div>
        break;

    default:
        <MudSkeleton Width="@Width" Height="@Height" Animation="Animation.Wave"/>
        break;
}

@code {
    [Parameter] public SkeletonType Type { get; set; } = SkeletonType.List;
    [Parameter] public int Rows { get; set; } = 5;
    [Parameter] public bool ShowAvatar { get; set; } = false;
    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "20px";

    private string GetRowWidth(int index)
    {
        var widths = new[] { "100%", "85%", "92%", "78%", "95%" };
        return widths[index % widths.Length];
    }
}

public enum SkeletonType { List, Card, Form, Text, PatientHeader }
```

**Archivo:** `Shared/Components/MedFarSkeleton.razor.css`

```css
.mf-skeleton-list { display: flex; flex-direction: column; gap: 12px; }
.mf-skeleton-row  { display: flex; align-items: center; padding: 8px 0; }
.mf-skeleton-form { display: flex; flex-direction: column; }
```

### 3.2 `MedFarEmptyState.razor` — ya definido, completar con variantes

**Archivo:** `Shared/Components/MedFarEmptyState.razor`

```razor
@using MudBlazor

<div class="mf-empty-state">
    <div class="mf-empty-icon-wrap">
        <MudIcon Icon="@Icon" Style="font-size: 4rem; opacity: 0.25;"/>
    </div>
    <MudText Typo="Typo.h6" Class="mf-empty-title">@Title</MudText>
    <MudText Typo="Typo.body2" Class="mf-empty-message">@Message</MudText>
    @if (ActionContent != null)
    {
        <div class="mt-6">@ActionContent</div>
    }
</div>

@code {
    [Parameter] public string Icon { get; set; } = Icons.Material.Outlined.Inbox;
    [Parameter] public string Title { get; set; } = "Sin registros";
    [Parameter] public string Message { get; set; } = "No hay datos disponibles en este momento.";
    [Parameter] public RenderFragment? ActionContent { get; set; }

    // Variantes predefinidas para los módulos más comunes
    public static (string Icon, string Title, string Message) Patients =>
        (Icons.Material.Outlined.PersonSearch,
         "Directorio vacío",
         "No has registrado pacientes en esta sucursal aún.");

    public static (string Icon, string Title, string Message) LabOrders =>
        (Icons.Material.Outlined.Biotech,
         "Sin órdenes de laboratorio",
         "Las órdenes aparecerán aquí cuando el médico las emita.");

    public static (string Icon, string Title, string Message) Invoices =>
        (Icons.Material.Outlined.ReceiptLong,
         "Sin facturas registradas",
         "Las facturas generadas aparecerán en este listado.");

    public static (string Icon, string Title, string Message) Appointments =>
        (Icons.Material.Outlined.CalendarMonth,
         "Agenda vacía",
         "No hay citas programadas para este período.");
}
```

**Archivo:** `Shared/Components/MedFarEmptyState.razor.css`

```css
.mf-empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 24px;
    text-align: center;
}
.mf-empty-icon-wrap {
    width: 96px; height: 96px;
    border-radius: 50%;
    background: var(--mf-primary-light);
    display: flex; align-items: center; justify-content: center;
    margin-bottom: 20px;
}
.mf-empty-title   { font-weight: 700; margin-bottom: 8px; }
.mf-empty-message { color: var(--mud-palette-text-secondary); max-width: 360px; }
```

### 3.3 `MedFarTable.razor` — versión completa con todas las variantes

**Archivo:** `Shared/Components/MedFarTable.razor`

```razor
@typeparam TItem
@using MudBlazor

<div class="mf-table-wrapper @(Dense ? "mf-table-dense" : "")">

    @* Toolbar opcional con búsqueda y acciones *@
    @if (ShowToolbar)
    {
        <div class="mf-table-toolbar surface-glass">
            @if (ShowSearch)
            {
                <MudTextField @bind-Value="_searchTerm"
                              Placeholder="@SearchPlaceholder"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Filled.Search"
                              Immediate="true" Clearable="true"
                              Variant="Variant.Outlined"
                              Class="mf-table-search"
                              OnClearButtonClick="@(() => _searchTerm = string.Empty)"/>
            }
            <div class="mf-table-actions">
                @ToolbarContent
            </div>
        </div>
    }

    <MudTable T="TItem"
              Items="@Items"
              Filter="@(ShowSearch ? new Func<TItem, bool>(FilterFunc) : null)"
              Loading="@IsLoading"
              Hover="true"
              Striped="@Striped"
              Elevation="0"
              Class="mf-table surface-glass shadow-float"
              Breakpoint="Breakpoint.Sm"
              FixedHeader="@FixedHeader"
              Height="@(FixedHeader ? TableHeight : null)"
              @ref="_table">

        <ToolBarContent>
            @* Espacio reservado — toolbar manejada arriba *@
        </ToolBarContent>

        <HeaderContent>
            @HeaderContent
        </HeaderContent>

        <RowTemplate>
            @RowTemplate(context)
        </RowTemplate>

        <LoadingContent>
            <MudTd colspan="@ColumnCount">
                <div class="pa-4">
                    <MedFarSkeleton Type="SkeletonType.List" Rows="@SkeletonRows"/>
                </div>
            </MudTd>
        </LoadingContent>

        <NoRecordsContent>
            <MudTd colspan="@ColumnCount">
                <MedFarEmptyState Title="@EmptyTitle"
                                  Message="@EmptyMessage"
                                  Icon="@EmptyIcon"
                                  ActionContent="@EmptyActionContent"/>
            </MudTd>
        </NoRecordsContent>

        <PagerContent>
            <MudTablePager InfoFormat="{first_item}-{last_item} de {all_items}"
                           RowsPerPageString="Filas:"
                           PageSizeOptions="new[] {10, 25, 50, 100}"/>
        </PagerContent>

    </MudTable>
</div>

@code {
    private MudTable<TItem>? _table;
    private string _searchTerm = string.Empty;

    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment<TItem> RowTemplate { get; set; } = null!;
    [Parameter] public RenderFragment? ToolbarContent { get; set; }

    // Búsqueda
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public string SearchPlaceholder { get; set; } = "Buscar...";
    [Parameter] public Func<TItem, string, bool>? SearchFilter { get; set; }

    // Layout
    [Parameter] public bool Dense { get; set; } = false;
    [Parameter] public bool Striped { get; set; } = true;
    [Parameter] public bool FixedHeader { get; set; } = false;
    [Parameter] public string TableHeight { get; set; } = "500px";
    [Parameter] public int ColumnCount { get; set; } = 5;
    [Parameter] public int SkeletonRows { get; set; } = 5;

    // Empty State
    [Parameter] public string EmptyTitle { get; set; } = "Sin registros";
    [Parameter] public string EmptyMessage { get; set; } = "No hay datos disponibles.";
    [Parameter] public string EmptyIcon { get; set; } = Icons.Material.Outlined.Inbox;
    [Parameter] public RenderFragment? EmptyActionContent { get; set; }

    private bool FilterFunc(TItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchTerm)) return true;
        if (SearchFilter != null) return SearchFilter(item, _searchTerm);
        // Fallback: buscar en ToString()
        return item?.ToString()?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
```

**Archivo:** `Shared/Components/MedFarTable.razor.css`

```css
.mf-table-wrapper  { display: flex; flex-direction: column; gap: 0; border-radius: var(--mf-radius-lg); overflow: hidden; }
.mf-table-toolbar  { display: flex; align-items: center; gap: 12px; padding: 12px 16px; border-radius: var(--mf-radius-lg) var(--mf-radius-lg) 0 0; border-bottom: 1px solid var(--mf-border); }
.mf-table-search   { flex: 1; max-width: 360px; }
.mf-table-actions  { display: flex; align-items: center; gap: 8px; margin-left: auto; }
.mf-table          { border-radius: 0 !important; }
.mf-table-dense td { padding: 6px 12px !important; font-size: 0.8125rem !important; }
```

### 3.4 `MedFarPageHeader.razor` — header estándar para todas las páginas

**Archivo:** `Shared/Components/MedFarPageHeader.razor`

```razor
@using MudBlazor

<div class="mf-page-header surface-glass shadow-float">
    <div class="mf-page-header-left">
        @if (ShowBack)
        {
            <MudIconButton Icon="@Icons.Material.Filled.ArrowBack"
                           OnClick="@OnBackClick"
                           Class="mf-back-btn" Size="Size.Medium"/>
        }
        <div class="mf-page-header-title">
            @if (!string.IsNullOrEmpty(Icon))
            {
                <MudIcon Icon="@Icon" Color="Color.Primary" Class="mr-2"/>
            }
            <div>
                <MudText Typo="Typo.h6" Class="mf-title-text">@Title</MudText>
                @if (!string.IsNullOrEmpty(Subtitle))
                {
                    <MudText Typo="Typo.caption" Color="Color.Secondary">@Subtitle</MudText>
                }
            </div>
        </div>
    </div>

    <div class="mf-page-header-actions">
        @* Acciones principales *@
        @PrimaryActions

        @* Menú kebab para acciones secundarias *@
        @if (SecondaryActions != null)
        {
            <MudMenu Icon="@Icons.Material.Filled.MoreVert"
                     AnchorOrigin="Origin.BottomRight"
                     TransformOrigin="Origin.TopRight"
                     Dense="true">
                @SecondaryActions
            </MudMenu>
        }
    </div>
</div>

@code {
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool ShowBack { get; set; } = false;
    [Parameter] public EventCallback OnBackClick { get; set; }
    [Parameter] public RenderFragment? PrimaryActions { get; set; }
    [Parameter] public RenderFragment? SecondaryActions { get; set; }
}
```

**Archivo:** `Shared/Components/MedFarPageHeader.razor.css`

```css
.mf-page-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 12px 20px; margin-bottom: 16px;
    position: sticky; top: 0; z-index: 10;
}
.mf-page-header-left   { display: flex; align-items: center; gap: 8px; }
.mf-page-header-title  { display: flex; align-items: center; }
.mf-title-text         { font-weight: 700; line-height: 1.2; }
.mf-page-header-actions { display: flex; align-items: center; gap: 8px; }
.mf-back-btn           { margin-right: 4px; }
```

---

## 4. Seguridad clínica — componentes Poka-Yoke

### 4.1 `StickyPatientAlerts.razor` — alergias y riesgos siempre visibles

**Archivo:** `Shared/Clinical/StickyPatientAlerts.razor`

```razor
@using MudBlazor
@if (Allergies?.Any() == true || CriticalAlerts?.Any() == true)
{
    <div class="sticky-clinical-alerts">
        @if (Allergies?.Any() == true)
        {
            <div class="sticky-alert-group sticky-allergies">
                <MudIcon Icon="@Icons.Material.Filled.Warning" Size="Size.Small"/>
                <span class="sticky-alert-label">ALERGIA:</span>
                @foreach (var a in Allergies.Take(3))
                {
                    <MudChip T="string" Size="Size.Small" Color="Color.Error"
                             Class="sticky-chip">@a</MudChip>
                }
                @if (Allergies.Count > 3)
                {
                    <MudChip T="string" Size="Size.Small" Color="Color.Error"
                             Variant="Variant.Outlined"
                             Class="sticky-chip">+@(Allergies.Count - 3)</MudChip>
                }
            </div>
        }
        @if (CriticalAlerts?.Any() == true)
        {
            <div class="sticky-alert-group sticky-risks">
                <MudIcon Icon="@Icons.Material.Filled.ErrorOutline" Size="Size.Small"/>
                <span class="sticky-alert-label">RIESGO:</span>
                @foreach (var r in CriticalAlerts.Take(2))
                {
                    <MudChip T="string" Size="Size.Small" Color="Color.Warning"
                             Class="sticky-chip">@r</MudChip>
                }
            </div>
        }
    </div>
}

@code {
    [Parameter] public List<string>? Allergies { get; set; }
    [Parameter] public List<string>? CriticalAlerts { get; set; }
}
```

**Archivo:** `Shared/Clinical/StickyPatientAlerts.razor.css`

```css
.sticky-clinical-alerts {
    position: sticky; /* fixed en la ficha del paciente */
    top: 64px;        /* justo debajo del AppBar */
    z-index: 20;
    display: flex; flex-wrap: wrap; gap: 8px;
    padding: 8px 16px;
    background: rgba(254, 226, 226, 0.95);
    backdrop-filter: blur(8px);
    border-bottom: 2px solid var(--mf-danger);
    border-radius: 0 0 var(--mf-radius-md) var(--mf-radius-md);
    animation: slideDown 0.2s ease;
}
.sticky-alert-group { display: flex; align-items: center; gap: 6px; }
.sticky-alert-label { font-size: 0.7rem; font-weight: 800; letter-spacing: 0.05em; color: var(--mf-danger); }
.sticky-chip        { font-size: 0.7rem !important; height: 22px !important; }

[data-theme="dark"] .sticky-clinical-alerts {
    background: rgba(127, 29, 29, 0.85);
}
@keyframes slideDown { from { transform: translateY(-100%); } to { transform: translateY(0); } }
```

### 4.2 `AllergyWarningModal.razor` — bloqueo al prescribir

**Archivo:** `Shared/Clinical/AllergyWarningModal.razor`

```razor
@using MudBlazor
@inject IDialogService DialogService

@* Este componente se usa como servicio — no tiene markup propio *@
@code {
    public async Task<bool> CheckAndConfirmAsync(
        string medicamentName,
        List<string> patientAllergies,
        bool isHardBlock = false)
    {
        // ¿El medicamento coincide con alguna alergia?
        var match = patientAllergies
            .FirstOrDefault(a => medicamentName.Contains(a, StringComparison.OrdinalIgnoreCase));

        if (match == null) return true; // Sin conflicto

        if (isHardBlock)
        {
            // HARD BLOCK: No se puede proceder
            await DialogService.ShowMessageBox(
                "⛔ Contraindicación absoluta",
                $"El paciente tiene alergia documentada a '{match}'. No es posible prescribir '{medicamentName}'.",
                yesText: "Entendido",
                options: new DialogOptions { MaxWidth = MaxWidth.Small });
            return false;
        }

        // SOFT BLOCK: Requiere confirmación doble
        var result = await DialogService.ShowMessageBox(
            "⚠️ Advertencia de alergia",
            $"El paciente tiene alergia a '{match}'. ¿Está seguro de que desea prescribir '{medicamentName}'? Esta acción quedará registrada en la auditoría.",
            yesText: "Confirmar y continuar",
            cancelText: "Cancelar",
            options: new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                BackgroundClass = "allergy-warning-overlay"
            });

        return result == true;
    }
}
```

### 4.3 `MedFarLabValue.razor` — valor de laboratorio con flag

**Archivo:** `Shared/Clinical/MedFarLabValue.razor`

```razor
@using MudBlazor

<div class="mf-lab-value @GetCssClass()">
    <span class="mf-lab-number">@FormattedValue</span>
    <span class="mf-lab-unit">@Unit</span>
    @if (IsOutOfRange)
    {
        <MudTooltip Text="@($"Rango: {RefMin} – {RefMax} {Unit}")">
            <span class="mf-lab-arrow">@(NumericValue > RefMax ? "↑" : "↓")</span>
        </MudTooltip>
    }
</div>

@code {
    [Parameter] public decimal? NumericValue { get; set; }
    [Parameter] public string? TextValue { get; set; }
    [Parameter] public string? Unit { get; set; }
    [Parameter] public decimal? RefMin { get; set; }
    [Parameter] public decimal? RefMax { get; set; }
    [Parameter] public bool IsOutOfRange { get; set; }

    private string FormattedValue =>
        NumericValue.HasValue ? NumericValue.Value.ToString("G6") : TextValue ?? "—";

    private string GetCssClass()
    {
        if (!IsOutOfRange) return "mf-lab-normal";
        return NumericValue > RefMax ? "mf-lab-high" : "mf-lab-low";
    }
}
```

**Archivo:** `Shared/Clinical/MedFarLabValue.razor.css`

```css
.mf-lab-value   { display: inline-flex; align-items: baseline; gap: 4px; font-family: var(--mf-font-mono); }
.mf-lab-number  { font-size: 1rem; font-weight: 600; }
.mf-lab-unit    { font-size: 0.75rem; color: var(--mud-palette-text-secondary); }
.mf-lab-arrow   { font-size: 0.875rem; font-weight: 800; }
.mf-lab-high    { color: var(--mf-danger) !important; }
.mf-lab-high .mf-lab-arrow { color: var(--mf-danger); }
.mf-lab-low     { color: var(--mf-info) !important; }
.mf-lab-low .mf-lab-arrow  { color: var(--mf-info); }
.mf-lab-normal  { color: var(--mf-success); }
```

### 4.4 `MedFarConfirmDialog.razor` — confirmación estándar para acciones críticas

**Archivo:** `Shared/Components/MedFarConfirmDialog.razor`

```razor
@using MudBlazor

<MudDialog>
    <TitleContent>
        <div class="d-flex align-center gap-2">
            <MudIcon Icon="@Icon" Color="@IconColor"/>
            <MudText Typo="Typo.h6">@Title</MudText>
        </div>
    </TitleContent>
    <DialogContent>
        <MudText>@Message</MudText>
        @if (AdditionalContent != null)
        {
            <div class="mt-3">@AdditionalContent</div>
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel" Variant="Variant.Text">@CancelText</MudButton>
        <MudButton OnClick="Confirm" Variant="Variant.Filled"
                   Color="@ConfirmColor" Class="@(IsDangerous ? "btn-danger" : "btn-emerald-gradient")">
            @ConfirmText
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public string Title { get; set; } = "Confirmar acción";
    [Parameter] public string Message { get; set; } = "¿Está seguro de que desea continuar?";
    [Parameter] public string ConfirmText { get; set; } = "Confirmar";
    [Parameter] public string CancelText { get; set; } = "Cancelar";
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Help;
    [Parameter] public Color IconColor { get; set; } = Color.Warning;
    [Parameter] public Color ConfirmColor { get; set; } = Color.Primary;
    [Parameter] public bool IsDangerous { get; set; } = false;
    [Parameter] public RenderFragment? AdditionalContent { get; set; }

    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
    private void Cancel()  => MudDialog.Cancel();
}
```

---

## 5. Layout — AppShell glassmorphism

### 5.1 `MainLayout.razor` — refactor completo

```razor
@inherits LayoutComponentBase
@using MudBlazor
@inject MedFarThemeService ThemeService

<MudThemeProvider Theme="@MedFarTheme.Build()"
                  IsDarkMode="@ThemeService.IsDarkMode"/>
<MudSnackbarProvider/>
<MudDialogProvider MaxWidth="MaxWidth.Medium"
                   FullWidth="false"
                   CloseButton="true"
                   BackdropClick="false"/>

<MudLayout>
    <!-- AppBar glassmorphism -->
    <MudAppBar Elevation="0" Class="mf-appbar surface-glass">
        <MudIconButton Icon="@Icons.Material.Filled.Menu"
                       Edge="Edge.Start"
                       OnClick="@ToggleDrawer"/>
        <div class="mf-appbar-brand">
            <img src="/img/logo-medfar.svg" alt="MedFarLab" height="32"/>
        </div>
        <MudSpacer/>
        <MedFarDarkModeToggle/>
        <MudMenu Icon="@Icons.Material.Filled.AccountCircle"
                 AnchorOrigin="Origin.BottomRight">
            <MudMenuItem>Mi perfil</MudMenuItem>
            <MudMenuItem>Cerrar sesión</MudMenuItem>
        </MudMenu>
    </MudAppBar>

    <!-- Sidebar glassmorphism -->
    <MudDrawer @bind-Open="@_drawerOpen"
               Elevation="0"
               Class="mf-sidebar surface-glass"
               ClipMode="DrawerClipMode.Always">
        <NavMenu/>
    </MudDrawer>

    <!-- Contenido principal -->
    <MudMainContent Class="mf-main-content">
        <MudContainer MaxWidth="MaxWidth.False" Class="pa-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _drawerOpen = true;
    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;
}
```

**Archivo:** `MainLayout.razor.css`

```css
.mf-appbar {
    border-bottom: 1px solid var(--mf-border) !important;
    backdrop-filter: blur(var(--mf-glass-blur)) !important;
}
.mf-appbar-brand { display: flex; align-items: center; margin-left: 8px; }
.mf-sidebar {
    border-right: 1px solid var(--mf-border) !important;
    backdrop-filter: blur(var(--mf-glass-blur)) !important;
}
.mf-main-content { background: var(--mf-bg) !important; min-height: 100vh; }
```

### 5.2 `MedFarDarkModeToggle.razor`

```razor
@using MudBlazor
@inject MedFarThemeService ThemeService

<MudTooltip Text="@(ThemeService.IsDarkMode ? "Modo claro" : "Modo oscuro")">
    <MudIconButton Icon="@(ThemeService.IsDarkMode
                            ? Icons.Material.Filled.LightMode
                            : Icons.Material.Filled.DarkMode)"
                   OnClick="@ThemeService.Toggle"
                   Size="Size.Medium"/>
</MudTooltip>

@code { }
```

**Archivo:** `Services/MedFarThemeService.cs`

```csharp
[RegisterScoped]
public class MedFarThemeService
{
    public bool IsDarkMode { get; private set; } = false;
    public event Action? OnChange;

    public void Toggle()
    {
        IsDarkMode = !IsDarkMode;
        OnChange?.Invoke();
    }
}
```

---

## 6. Patrón Code-Behind — ejemplo completo

### Ejemplo: `Pacientes/DirectorioPacientes.razor`

```razor
@page "/pacientes"
@using MudBlazor
@inherits DirectorioPacientesBase

<MedFarPageHeader Title="Directorio de Pacientes"
                  Icon="@Icons.Material.Filled.People"
                  Subtitle="@($"{TotalPacientes} pacientes registrados")">
    <PrimaryActions>
        <MudButton Variant="Variant.Filled"
                   StartIcon="@Icons.Material.Filled.PersonAdd"
                   Class="btn-emerald-gradient"
                   OnClick="NuevoPaciente">
            Nuevo Paciente
        </MudButton>
    </PrimaryActions>
    <SecondaryActions>
        <MudMenuItem Icon="@Icons.Material.Filled.FileDownload"
                     OnClick="ExportarCSV">Exportar CSV</MudMenuItem>
        <MudMenuItem Icon="@Icons.Material.Filled.Print"
                     OnClick="Imprimir">Imprimir directorio</MudMenuItem>
    </SecondaryActions>
</MedFarPageHeader>

<MedFarTable TItem="PatientDTO"
             Items="@Pacientes"
             IsLoading="@CargandoPacientes"
             SearchFilter="@FiltrarPaciente"
             SearchPlaceholder="Buscar por nombre o cédula..."
             ColumnCount="5"
             EmptyTitle="@MedFarEmptyState.Patients.Title"
             EmptyMessage="@MedFarEmptyState.Patients.Message"
             EmptyIcon="@MedFarEmptyState.Patients.Icon">

    <ToolbarContent>
        <MudSelect T="int?" @bind-Value="FiltroSucursal"
                   Label="Sucursal" Variant="Variant.Outlined"
                   Dense="true" Clearable="true" Style="width:180px">
            @foreach (var s in Sucursales)
            {
                <MudSelectItem Value="@((int?)s.Id)">@s.Name</MudSelectItem>
            }
        </MudSelect>
    </ToolbarContent>

    <HeaderContent>
        <MudTh>Paciente</MudTh>
        <MudTh>Identificación</MudTh>
        <MudTh>Edad</MudTh>
        <MudTh>Última consulta</MudTh>
        <MudTh>Acciones</MudTh>
    </HeaderContent>

    <RowTemplate>
        <MudTd>
            <div class="d-flex align-center gap-3">
                <MudAvatar Color="Color.Primary" Size="Size.Small">
                    @context.FullName[0]
                </MudAvatar>
                <div>
                    <MudText Typo="Typo.body2" Class="font-weight-bold">@context.FullName</MudText>
                    @if (context.HasCriticalAlerts)
                    {
                        <MudChip T="string" Size="Size.Small" Color="Color.Error"
                                 Icon="@Icons.Material.Filled.Warning">Alergia</MudChip>
                    }
                </div>
            </div>
        </MudTd>
        <MudTd>@context.IdNumber</MudTd>
        <MudTd>@context.Age años</MudTd>
        <MudTd>@(context.LastVisit?.ToString("dd/MM/yyyy") ?? "Sin consultas")</MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Visibility"
                           Size="Size.Small"
                           OnClick="@(() => VerExpediente(context.Id))"/>
            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                           Size="Size.Small"
                           OnClick="@(() => EditarPaciente(context.Id))"/>
        </MudTd>
    </RowTemplate>

    <EmptyActionContent>
        <MudButton Variant="Variant.Filled"
                   StartIcon="@Icons.Material.Filled.PersonAdd"
                   Class="btn-emerald-gradient"
                   OnClick="NuevoPaciente">
            Registrar primer paciente
        </MudButton>
    </EmptyActionContent>

</MedFarTable>
```

### `DirectorioPacientes.razor.cs` (Code-Behind)

```csharp
public partial class DirectorioPacientes : ComponentBase
{
    [Inject] private IApplicationDispatcher Dispatcher { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    protected List<PatientDTO> Pacientes { get; set; } = new();
    protected List<BranchDTO> Sucursales { get; set; } = new();
    protected bool CargandoPacientes { get; set; } = true;
    protected int TotalPacientes { get; set; } = 0;
    protected int? FiltroSucursal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CargandoPacientes = true;
        var result = await Dispatcher.DispatchAsync<List<PatientDTO>>(
            new GetPatientDirectoryRequestDTO());

        if (result.IsSuccess && result.Data != null)
        {
            Pacientes = result.Data;
            TotalPacientes = Pacientes.Count;
        }
        CargandoPacientes = false;
    }

    protected bool FiltrarPaciente(PatientDTO p, string term) =>
        p.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
        p.IdNumber.Contains(term, StringComparison.OrdinalIgnoreCase);

    protected void VerExpediente(long patientId) => Nav.NavigateTo($"/pacientes/{patientId}");
    protected void EditarPaciente(long patientId) => Nav.NavigateTo($"/pacientes/{patientId}/editar");
    protected void NuevoPaciente() => Nav.NavigateTo("/pacientes/nuevo");
    protected async Task ExportarCSV() { /* implementar */ }
    protected async Task Imprimir() { /* implementar */ }
}
```

---

## 7. Plan de migración — 4 fases no-breaking

### Fase 1 — Fundamentos (Semana 1-2)
```
Crear sin tocar páginas existentes:
□ medfar-tokens.css + app.css actualizado
□ MedFarTheme.Build() en Program.cs
□ MedFarThemeService.cs (dark mode)
□ MedFarSkeleton.razor (+.cs +.css)
□ MedFarEmptyState.razor (+.cs +.css)
□ MedFarTable.razor (+.cs +.css)
□ MedFarPageHeader.razor (+.cs +.css)
□ MedFarConfirmDialog.razor (+.cs)
□ MainLayout.razor refactorizado
□ MedFarDarkModeToggle.razor
```

### Fase 2 — Componentes clínicos (Semana 3-4)
```
□ StickyPatientAlerts.razor (+.cs +.css)
□ AllergyWarningModal.razor (+.cs)
□ MedFarLabValue.razor (+.cs +.css)
□ MedFarSnackbar.razor con TraceId
□ DuplicatePatientWarning.razor
□ Migrar página: DirectorioPacientes (ejemplo piloto)
□ Migrar página: ExpedientePaciente
□ Migrar página: RegistroPaciente
```

### Fase 3 — Módulos core (Semana 5-6)
```
Migrar MudTable → MedFarTable en:
□ Care: Consultas, Citas, Recetas
□ Laboratory: Órdenes, Resultados, Plantillas
□ Billing: Facturas, Pagos
□ Identity: Usuarios, Organizaciones

Aplicar Code-Behind Triple Archivo en todas las páginas migradas.
```

### Fase 4 — Módulos secundarios + pulido (Semana 7-8)
```
□ Billing: Suscripciones, Planes SaaS
□ Inventory: Servicios, Precios
□ Security: Roles, Permisos
□ Modo oscuro: verificación en TODAS las páginas
□ Dark Mode: Radiología y Laboratorio (densidad +20%)
□ Audit de accesibilidad WCAG 2.1
□ Pruebas en mobile/tablet
□ Cleanup: eliminar MudProgressCircular en todas las páginas
```

---

## 8. Checklist anti-regresión por página migrada

Antes de marcar una página como migrada, verificar:

```
□ MudTable → MedFarTable (con IsLoading, EmptyState, SearchFilter)
□ MudProgressCircular eliminado → MedFarSkeleton en su lugar
□ Página dividida en 3 archivos (.razor / .razor.cs / .razor.css)
□ Lógica movida a Code-Behind (0 bloques @code complejos en .razor)
□ MedFarPageHeader implementado con botones primarios y kebab
□ Acciones críticas usan MedFarConfirmDialog
□ Snackbar de error incluye TraceId
□ Página verificada en Light Mode
□ Página verificada en Dark Mode
□ Página verificada en mobile (< 768px)
□ Si es clínica: StickyPatientAlerts implementado
□ Si es laboratorio: MedFarLabValue en valores de resultado
□ Si es recetario: AllergyWarningModal conectado
```

---

## 9. Mapa de archivos — todos los que toca Antigravity

### Crear nuevos
| Archivo | Fase |
|---|---|
| `wwwroot/css/medfar-tokens.css` | 1 |
| `Services/MedFarThemeService.cs` | 1 |
| `Shared/Components/MedFarSkeleton.razor` (+.cs +.css) | 1 |
| `Shared/Components/MedFarEmptyState.razor` (+.cs +.css) | 1 |
| `Shared/Components/MedFarTable.razor` (+.cs +.css) | 1 |
| `Shared/Components/MedFarPageHeader.razor` (+.cs +.css) | 1 |
| `Shared/Components/MedFarConfirmDialog.razor` (+.cs) | 1 |
| `Shared/Components/MedFarDarkModeToggle.razor` (+.cs) | 1 |
| `Shared/Clinical/StickyPatientAlerts.razor` (+.cs +.css) | 2 |
| `Shared/Clinical/AllergyWarningModal.razor` (+.cs) | 2 |
| `Shared/Clinical/MedFarLabValue.razor` (+.cs +.css) | 2 |
| `Shared/Clinical/DuplicatePatientWarning.razor` (+.cs) | 2 |

### Modificar existentes
| Archivo | Cambio | Fase |
|---|---|---|
| `Program.cs` | `MedFarTheme.Build()` + `MedFarThemeService` | 1 |
| `wwwroot/css/app.css` | Import tokens, eliminar estilos inline obsoletos | 1 |
| `Shared/MainLayout.razor` (+.cs +.css) | Glassmorphism AppBar + Drawer | 1 |
| `Pages/Patients/*.razor` (3 páginas) | Triple archivo + MedFarTable | 2 |
| `Pages/Care/*.razor` (4 páginas) | Triple archivo + MedFarTable | 3 |
| `Pages/Laboratory/*.razor` (4 páginas) | Triple archivo + MedFarTable + MedFarLabValue | 3 |
| `Pages/Billing/*.razor` (4 páginas) | Triple archivo + MedFarTable | 3-4 |
| `Pages/Identity/*.razor` (3 páginas) | Triple archivo + MedFarTable | 4 |
| `Pages/Inventory/*.razor` (2 páginas) | Triple archivo + MedFarTable | 4 |
| `Pages/Security/*.razor` (2 páginas) | Triple archivo + MedFarTable | 4 |

---

*Fin del documento — MedfarLabs PWA · Refactorización Total UI/UX · Mayo 2025*
