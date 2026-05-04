# Flujo 16: Restricción de Funcionalidad por Plan (SaaS Policy)

## 1. Descripción
Define cómo el Plan de Suscripción (Bronce, Plata, Oro) limita las acciones físicas que los usuarios pueden realizar en el Core.

## 2. Matriz de Influencia (Ejemplo)
* **Plan Bronce:** Solo permite acciones de `Care` (Clínica) y `Billing` básico. Bloquea comandos de `Laboratory`.
* **Plan Plata:** Habilita `Laboratory` pero limita el número de usuarios activos a 5.
* **Plan Oro:** Acceso total a todos los módulos, incluyendo `Inventory` avanzado y Reportes Estadísticos.

## 3. Implementación Técnica y Optimización
1. **Claims Transformation y Caché:** Al loguearse, el sistema añade una Claim de `Plan_Type` al usuario. Para evitar consultas constantes a la base de datos, las políticas de los planes se almacenan en Memoria/Redis (Caching).
2. **Requirement Handlers:** Antes de ejecutar un `CreateLabOrderCommand`, un decorador (o `ActionFilter`) verifica de forma ultra rápida si la Claim del Plan permite esta acción según el caché.
3. **UI Feedback:** En la PWA, los botones de acciones "Premium" aparecen bloqueados o con un banner de "Actualiza tu plan".

## 4. Casos de Uso
* UC-32: Validación de cuotas de uso (ej. "Has llegado al límite de 100 facturas de tu plan").