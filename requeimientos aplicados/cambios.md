# Estándar Unificado de Interfaz y Componentes Maestros - MedFarLab

Este documento centraliza las directivas de diseño (ANTIGRAVITY) y el código de los componentes base que garantizan la consistencia Premium y la seguridad clínica en toda la PWA.

---

## 1. Directivas UX/UI para Antigravity (Leyes de Estilo)

Antigravity DEBE seguir estas reglas en cada refactorización:

1.  **Glassmorphism Obligatorio:** Todo contenedor principal (Sidebars, Modales, AppBars) debe usar la clase `.surface-glass`.
2.  **Profundidad y Sombras:** No usar sombras planas. Usar `.shadow-float` para elevar elementos sobre el fondo.
3.  **Botones con Gradiente:** Los botones primarios (Guardar, Confirmar) deben usar la clase `.btn-emerald-gradient`.
4.  **Gestión de Carga (Skeleton First):** Queda terminantemente prohibido mostrar un `MudProgressCircular` para cargas de listas o tablas. Se debe usar `MedFarSkeleton`.
5.  **Manejo de Datos Vacíos:** Nunca dejar una tabla vacía con el mensaje por defecto. Se debe usar `MedFarEmptyState`.
6.  **Poka-Yoke Clínico:** Resultados fuera de rango deben resaltar en rojo y negrita. Alergias deben usar el contenedor `sticky-clinical-alerts`.

---

## 2. Componente: MedFarEmptyState.razor

Diseñado para guiar al usuario de forma empática cuando no hay información disponible.

```razor
@using MudBlazor

<div class="d-flex flex-column align-center justify-center pa-10 text-center">
    <MudIcon Icon="@Icon" Color="Color.Primary" Style="font-size: 5rem; opacity: 0.3;" Class="mb-4" />
    <MudText Typo="Typo.h5" Class="mb-2"><b>@Title</b></MudText>
    <MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-6">@Message</MudText>
    
    @if (ActionContent != null)
    {
        @ActionContent
    }
</div>

@code {
    [Parameter] public string Icon { get; set; } = Icons.Material.Outlined.Inbox;
    [Parameter] public string Title { get; set; } = "Sin registros";
    [Parameter] public string Message { get; set; } = "No hay datos para mostrar en este momento.";
    [Parameter] public RenderFragment? ActionContent { get; set; }
}
3. Componente Maestro: MedFarTable.razor
El estándar de oro para todas las tablas del sistema. Encapsula estilos, skeletons y estados vacíos.

Razor CSHTML
@typeparam TItem
@using MudBlazor

<MudTable Items="@Items" 
          Loading="@IsLoading" 
          Hover="true" 
          Elevation="0" 
          Class="surface-glass shadow-float rounded-xl overflow-hidden"
          Breakpoint="Breakpoint.Sm">
    
    <HeaderContent>
        @HeaderContent
    </HeaderContent>

    <RowTemplate>
        @RowTemplate(context)
    </RowTemplate>

    <LoadingContent>
        <div class="pa-6">
            <MedFarSkeleton Type="SkeletonType.List" Rows="5" />
        </div>
    </LoadingContent>

    <NoRecordsContent>
        <MedFarEmptyState Title="@EmptyTitle" 
                         Message="@EmptyMessage" 
                         Icon="@EmptyIcon"
                         ActionContent="@EmptyActionContent" />
    </NoRecordsContent>

    <PagerContent>
        <MudTablePager InfoFormat="{first_item}-{last_item} de {all_items}" />
    </PagerContent>
</MudTable>

@code {
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment<TItem> RowTemplate { get; set; } = null!;
    
    // Configuración del Empty State
    [Parameter] public string EmptyTitle { get; set; } = "Bandeja vacía";
    [Parameter] public string EmptyMessage { get; set; } = "No hay datos disponibles en esta sección actualmente.";
    [Parameter] public string EmptyIcon { get; set; } = Icons.Material.Filled.Inbox;
    [Parameter] public RenderFragment? EmptyActionContent { get; set; }
}
4. Guía de Implementación Masiva (Refactor)
Antigravity debe reemplazar todas las instancias de MudTable por MedFarTable.

Ejemplo de uso en Directorio de Pacientes:
Razor CSHTML
<MedFarTable TItem="PatientDTO" 
            Items="@Patients" 
            IsLoading="@IsLoading"
            EmptyTitle="Directorio Vacío"
            EmptyMessage="Aún no has registrado pacientes en esta sucursal.">
    <HeaderContent>
        <MudTh>Nombre</MudTh>
        <MudTh>Identificación</MudTh>
        <MudTh>Acciones</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.FullName</MudTd>
        <MudTd>@context.IdNumber</MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Edit" Color="Color.Primary" />
        </MudTd>
    </RowTemplate>
    <EmptyActionContent>
        <MudButton Variant="Variant.Filled" Color="Color.Primary" Class="btn-emerald-gradient">
            Registrar Primer Paciente
        </MudButton>
    </EmptyActionContent>
</MedFarTable>
```

### 🧠 ¿Qué logramos con este archivo?

1.  **Centralización:** Tienes la lógica de negocio (Directivas) y la lógica de presentación (Componentes) en un solo lugar.
2.  **Automatización:** Antigravity puede leer este archivo y entender que `MedFarTable` ya trae incluido el `MedFarSkeleton` y el `MedFarEmptyState`, por lo que puede limpiar el código redundante en las páginas existentes.
3.  **Escalabilidad:** Si en el futuro decidimos que todas las tablas deben tener un botón de exportar a Excel, solo lo agregamos a `MedFarTable.razor` en este estándar y se replica en todo el sistema.

