# MedFarLab Elite Development Directives (ANTIGRAVITY)

Este documento contiene las "Leyes de Robótica" para la generación de código y diseño de MedFarLab. Antigravity DEBE seguir estas reglas sin excepción para asegurar un producto de grado médico.

## 1. Arquitectura de Código (Mandamiento del Triple Archivo)
Toda página o componente complejo se divide en 3 archivos bajo el mismo nombre:
- **`Nombre.razor`**: Solo marcado HTML y componentes Blazor. PROHIBIDO bloques `@code` de más de 5 líneas.
- **`Nombre.razor.cs`**: Clase `partial` con toda la lógica C#, inyección de dependencias y gestión de estados.
- **`Nombre.razor.css`**: Estilos Scoped. Evitar estilos en línea (inline styles).

## 2. Estándar de Carga (Skeleton First)
- **Prohibición de Spinners:** Queda prohibido el uso de `MudProgressCircular` o spinners genéricos para cargas de página completa.
- **Uso de MedFarSkeleton:** Se debe usar obligatoriamente `<MedFarSkeleton Type="..." />` mientras `IsLoading` sea true.
- **Reserva de Espacio:** El skeleton debe imitar la estructura final para evitar el "Cumulative Layout Shift" (saltos visuales).

## 3. UI/UX Premium (Glassmorphism & Depth)
- **Superficies:** Usar la clase `.surface-glass` para Sidebars, Modales y Cards principales.
- **Efecto Glass:** `backdrop-filter: blur(12px); background: rgba(255, 255, 255, 0.7);`.
- **Botones:** Nunca usar botones planos. Deben tener gradientes sutiles y la clase `.shadow-float`.
- **Modo Oscuro:** Toda nueva UI debe ser verificada en `PaletteDark` para asegurar legibilidad en entornos clínicos de baja luz.

## 4. Seguridad Clínica (Poka-Yoke)
- **Sticky Badges:** En la ficha del paciente, las Alergias y Riesgos Críticos deben estar en un contenedor con `position: fixed` para que nunca desaparezcan con el scroll.
- **Alertas Visuales:** Los valores de laboratorio fuera de rango DEBEN estar en rojo y negrita.
- **Bloqueos Suaves:** Antes de acciones críticas (Cerrar consulta, Guardar factura), validar datos en el front y mostrar confirmaciones claras.

## 5. Resiliencia y Datos (Traceability)
- **TraceId:** En cada mensaje de error (Snackbar/Toast), se debe incluir el `TraceId` de la petición para facilitar el soporte técnico.
- **BaseResponse:** El frontend siempre debe esperar y procesar el objeto `BaseResponse<T>`, manejando `IsSuccess` antes de intentar acceder a `Data`.
- **Idempotencia:** Toda acción de "Guardar" debe enviar un `x-trace-id` en el header para prevenir duplicados.

## 6. Persistencia (BaseRepository)
- Todo nuevo repositorio debe heredar de `BaseRepository<T>`.
- Debe implementar Soft Delete (`is_active`).
- Debe usar la política de reintento `_retryPolicy` para errores transitorios de base de datos.
- Debe capturar y traducir `PostgresException` a mensajes amigables mediante `TranslateException`.
