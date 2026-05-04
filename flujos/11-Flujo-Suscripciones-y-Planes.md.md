# Flujo 11: Gestión de Suscripciones y Límites

## 1. Descripción
Administración de los planes comerciales (Bronce, Plata, Oro) y el control de acceso a módulos basado en el plan activo.

## 2. Pasos del Flujo
1. **Selección de Plan:** El cliente elige un plan que define límites (ej. Número de médicos, capacidad de almacenamiento de imágenes, o acceso a módulo de Laboratorio)[cite: 3].
2. **Activación de Módulos:** La PWA habilita o deshabilita secciones del menú lateral según las `Claims` del Tenant.
3. **Control de Cuotas:** El sistema monitorea el uso (ej. "Has alcanzado el límite de 500 órdenes mensuales de tu plan").

## 3. Casos de Uso
* UC-22: Cambio de plan (Upgrade/Downgrade).
* UC-23: Validación de permisos por suscripción.