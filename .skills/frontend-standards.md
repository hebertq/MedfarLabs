# Estándares de Programación - Perfil Frontend (Blazor WebAssembly / Server)

Este documento define el patrón arquitectónico para las aplicaciones de cliente interactivo (`MedFarLab.Pwa` y `MedFarLab.WebUI`).

## 1. Patrón Smart & Dumb Components
Para mantener el proyecto testeable, mantenible y responsivo:
- **Smart Components (Páginas/Contenedores):** 
  - Son responsables de inyectar los servicios (`[Inject] HttpClient`, `IStateContainer`).
  - Manejan el ciclo de vida de la página (`OnInitializedAsync`).
  - Obtienen los datos y los pasan a sus componentes hijos a través de parámetros.
- **Dumb Components (Elementos de Interfaz Visual):**
  - No tienen dependencias inyectadas con `[Inject]` (salvo localizadores de UI o IJSRuntime si es estrictamente visual).
  - Reciben datos exclusivamente por `[Parameter]`.
  - Devuelven acciones al padre exclusivamente por `[Parameter] EventCallback`.

## 2. Consumo de API y Manejo de Tokens
- El `HttpClient` debe configurarse mediante el `IHttpClientFactory` y un `DelegatingHandler` (Ej. `CustomAuthorizationMessageHandler`) encargado de inyectar el token JWT (`Bearer`) automáticamente en cada petición saliente.
- Evitar interceptar localmente los 401s en cada página. Un `AuthenticationStateProvider` global debe detectar expiraciones de sesión y forzar el cierre de sesión redirigiendo a `/login`.

## 3. Gestión de Estado (State Management)
- NO inyectar servicios Scoped en WebAssembly que muten su estado directamente sin notificar a la UI, lo cual genera componentes desincronizados.
- Usar un patrón contenedor de estado ligero (`StateContainer`):
```csharp
public class CartState
{
    public int Count { get; private set; }
    public event Action OnChange;
    public void Add() { Count++; NotifyStateChanged(); }
    private void NotifyStateChanged() => OnChange?.Invoke();
}
```
- En componentes visuales, suscribirse en `OnInitialized` (`State.OnChange += StateHasChanged`) y NO olvidar el `Dispose()`.

## 4. Optimización de Ciclo de Vida
- Minimizar re-renders. Si un componente depende de parámetros que cambian mucho pero la vista no siempre necesita cambiar, sobrescribir `ShouldRender()`.
- Utilizar `StateHasChanged()` de forma defensiva, solo cuando estemos seguros de que el marco de trabajo de Blazor no puede interceptar automáticamente el cambio de estado (ej: retornos asíncronos en JS Interop o Timers).

## 5. Estandarización de Barras de Acción (Page Headers)
Para unificar la experiencia en todas las pantallas y ventanas modales:
- **Páginas Secundarias (No Menú Principal):** Toda página interna o de detalle DEBE implementar un componente de "Header / Action Bar" estandarizado en la parte superior.
- **Botones y Acciones:** Este componente debe contener sistemáticamente los botones primarios: Regresar (Back), Guardar (Save), Actualizar (Update), Imprimir (Print) o Anular (Void) según el contexto.
- **Menú Kebab (3 puntos):** Cualquier acción secundaria o menos frecuente debe agruparse obligatoriamente dentro de un menú desplegable (botón de 3 puntos) integrado en este mismo Header.
- **Diálogos y Modales:** Las ventanas emergentes (Dialogs) también deben reutilizar este componente Header (o una variante minimalista del mismo) para que los botones de acción principal y cierre mantengan una posición y estilo idénticos en toda la aplicación.

## 6. Patrón Code-Behind Obligatorio (3 Archivos por Página)
Para mantener una estricta separación de responsabilidades y evitar código espagueti en la UI, toda página (o componente complejo) DEBE dividirse en tres archivos bajo el mismo nombre:
- **`NombrePagina.razor`**: Contiene estrictamente el marcado HTML, la estructura visual y la invocación de otros componentes de Blazor. NO debe contener bloques `@code { }` con lógica compleja.
- **`NombrePagina.razor.cs` (Code-Behind)**: Clase `partial` que alberga toda la lógica en C#. Es la única responsable de conectarse con la capa de aplicación (servicios inyectados), gestionar el estado y manejar los eventos delegados de las acciones de la página.
- **`NombrePagina.razor.css` (Scoped CSS)**: Archivo opcional (pero recomendado si se requiere estilo personalizado) que contiene exclusivamente las reglas de diseño particulares de ese componente para evitar la contaminación global de estilos.

## 7. Nomenclatura Estricta en Inglés para Archivos y Rutas
- Todos los nombres de archivos de componentes, páginas Razor y código C# deben estar estrictamente en **Inglés** (Ej. `PatientRecord.razor` en lugar de `ExpedientePaciente.razor`, `ConsultationWorkspace.razor` en lugar de `ConsultaActiva.razor`).
- Las rutas `@page` también deben seguir una estructura lógica en inglés o estandarizada (Ej. `@page "/patients/{PatientId:long}"`).

## 8. Optimizaciones UI/UX y Sistema de Diseño (Fase 4)
Para lograr una experiencia de usuario moderna e intuitiva, se deben respetar las siguientes directrices de diseño implementadas en la arquitectura base:
- **Tokens Semánticos:** Utilizar siempre las variables CSS de `medfar-tokens.css` (Ej. `var(--mf-primary)`, `var(--mf-danger)`) en lugar de hardcodear colores. Para sombras y profundidades, usar `var(--mf-shadow-sm)` o `var(--mf-shadow-float)`.
- **Componentes Compartidos Core:**
  - Emplear `MedFarTable` para todas las tablas. Integrar un `EmptyState` visualmente atractivo cuando no haya datos (`EmptyTitle`, `EmptyMessage`, `EmptyIcon`).
  - Utilizar `MedFarSkeleton` para animaciones fluidas (shimmer) durante los estados de carga `Cargando=true`.
  - Asegurar un tamaño mínimo táctil de 44px en elementos interactivos y componentes de formulario.
- **Flujos de Trabajo Optimizados (UX):**
  - Los Dashboards principales (Ej. *ConsultationWorkspace*) deben utilizar un layout de "Paneles Maestros" (Master Panels) reduciendo la apertura de diálogos/modales innecesarios. Mostrar historial/contexto en un panel fijo y herramientas de edición/captura en el panel principal.
- **Sincronización Offline:** Evidenciar visualmente si los datos están pendientes de sincronización (Nube Naranja) o al día (Nube Azul), respetando la Optimistic UI.
