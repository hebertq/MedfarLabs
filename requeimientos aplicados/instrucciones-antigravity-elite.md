# Plan de Implementación de Mejoras UX/UI de Élite (Instrucciones para Antigravity)

Este documento define la hoja de ruta técnica y estética para elevar la experiencia de usuario de **MedFarLab** a grado médico, siguiendo estrictamente las Directivas de Élite (ANTIGRAVITY).

---

## 1. Fase de Refactorización de Arquitectura y Base
**Objetivo:** Asegurar que el terreno sea fértil para las mejoras visuales sin comprometer la estabilidad.

* **Validación del Triple Archivo:** Verificar que todas las páginas (`ConsultationWorkspace`, `PatientDirectory`, `ClinicalDashboard`) cumplan con la separación en archivos `.razor`, `.razor.cs` y `.razor.css`. [cite: medfarlab-elite-directives.md, frontend-standards.md]
* **Limpieza de Marcado:** Eliminar bloques `@code` extensos en los archivos `.razor` (límite de 5 líneas). [cite: medfarlab-elite-directives.md]
* **Migración de Carga (Skeleton First):**
    * Identificar y eliminar todos los componentes `MudProgressCircular` y spinners genéricos. [cite: medfarlab-elite-directives.md]
    * Implementar `<MedFarSkeleton />` con animación de pulso (Wave) en cada contenedor de carga asíncrona. [cite: frontend-elite-skills.md, medfarlab-elite-directives.md]

## 2. Fase de Estética de Élite (Glassmorphism & Modo Oscuro)
**Objetivo:** Crear una interfaz táctica y moderna que reduzca la fatiga visual.

* **Implementación de Superficies:**
    * Aplicar la clase `.surface-glass` a todos los paneles principales y cards de dashboard. [cite: medfarlab-elite-directives.md]
    * Configurar el `backdrop-filter: blur(12px)` y fondos semitransparentes en el archivo CSS global. [cite: medfarlab-elite-directives.md]
* **Estandarización de Botones:**
    * Reemplazar botones planos por botones con gradientes sutiles y la clase `.shadow-float`. [cite: medfarlab-elite-directives.md]
* **Optimización del Modo Oscuro:**
    * Verificar que la paleta `PaletteDark` use grises profundos (evitando el #000) y que el color `Primary` sea `#4ADE80` para legibilidad. [cite: frontend-elite-skills.md]

## 3. Fase de Seguridad Clínica (Poka-Yoke & Alertas)
**Objetivo:** Minimizar el error humano mediante ayudas visuales persistentes.

* **Sticky Patient Alerts:**
    * En `ConsultationWorkspace`, fijar las Alergias (Rojo) y Riesgos (Naranja) en la parte superior derecha de la ficha. [cite: clinical-ux-advanced.md, medfarlab-elite-directives.md]
    * Asegurar que estos badges sean visibles en menos de 2 segundos. [cite: clinical-ux-advanced.md]
* **Bloqueos de Seguridad (Soft Blocks):**
    * Implementar validación cruzada antes de guardar recetas: si hay coincidencia de alergia, disparar un modal de confirmación doble. [cite: clinical-ux-advanced.md, clinical-ux-standards.md]
* **Visualización de Datos Críticos:**
    * Configurar los valores de laboratorio fuera de rango para que se muestren automáticamente en **negrita y rojo**. [cite: clinical-ux-advanced.md, medfarlab-elite-directives.md]

## 4. Fase de Eficiencia Operativa (Densidad & Navegación)
**Objetivo:** Maximizar el escaneo visual rápido de datos.

* **Ajuste de Densidad:**
    * Aumentar la densidad de datos en un 20% en el `LabDashboard` y pantallas de resultados de laboratorio. [cite: clinical-ux-advanced.md]
* **Estandarización de Headers:**
    * Asegurar que cada página interna tenga el `MedFarPageHeader` con botones primarios visibles y acciones secundarias ocultas en un menú de 3 puntos (Kebab). [cite: frontend-standards.md]

## 5. Procedimientos de Prueba y Mejora Continua
**Objetivo:** Asegurar que los cambios no rompan funcionalidades existentes y sean resilientes.

* **Validación de Trazabilidad:**
    * Confirmar que cada mensaje de error en el Snackbar incluya el `TraceId` de la petición. [cite: medfarlab-elite-directives.md]
* **Pruebas de Resiliencia:**
    * Verificar el funcionamiento de la política de reintento en los repositorios durante fallos de red. [cite: medfarlab-elite-directives.md]
* **Checklist Clínico de Vista:** Antes de marcar una mejora como finalizada, responder:
    1. ¿A cuántos clics estoy de ver las alergias? [cite: clinical-ux-standards.md]
    2. ¿El formato de dosis previene errores de lectura? [cite: clinical-ux-standards.md]
    3. ¿La vista en modo oscuro es legible en baja luz? [cite: clinical-ux-standards.md]

---
**Instrucción Final para Antigravity:** Ejecuta los cambios de forma incremental por módulo, priorizando `ConsultationWorkspace` por su naturaleza crítica. No cierres ninguna tarea sin verificar la adherencia al Mandamiento del Triple Archivo.
