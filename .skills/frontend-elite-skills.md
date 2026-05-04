# Estándares Frontend - Skills de Élite (Percepción y Resiliencia)

Este documento complementa los estándares básicos, enfocándose en la experiencia de usuario de alto rendimiento y adaptabilidad ambiental.

## 1. Percepción de Velocidad (Skeleton Loaders)
- **Cero Pantallas en Blanco:** Nunca se debe mostrar una vista vacía mientras se cargan datos asíncronos.
- **Implementación de Skeletons:** Se deben utilizar componentes `MudSkeleton` que imiten la estructura real de la página.
  - Para tablas: Usar una fila de skeletons que simule celdas.
  - Para perfiles: Simular el avatar y líneas de texto (Bio).
- **Animación:** Los skeletons deben tener una animación de "pulso" suave (Wave) para indicar actividad al usuario.

## 2. Arquitectura de Modo Oscuro (Ergo-Design)
- **Contraste Adaptativo:** El sistema debe detectar la preferencia del sistema operativo, pero permitir el cambio manual en el `NavMenu`.
- **Paleta Dark Profesional:** Evitar el negro puro (#000). Utilizar grises profundos para superficies y colores acentuados con mayor saturación para mantener la accesibilidad.
- **Implementación en MudBlazor:**
  ```csharp
  public PaletteDark PaletteDark => new() {
      Primary = "#4ADE80", // Emerald más brillante para Dark
      Surface = "#1E1E2E",
      Background = "#121218",
      AppbarBackground = "rgba(30, 30, 46, 0.8)"
  };
