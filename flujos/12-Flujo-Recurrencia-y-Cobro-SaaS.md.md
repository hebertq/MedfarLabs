# Flujo 12: Facturación Recurrente (Billing SaaS)

## 1. Descripción
Gestión de la cartera de clientes de MedFarLab, procesamiento de pagos mensuales y suspensión por falta de pago.

## 2. Pasos del Flujo
1. **Corte Mensual:** El sistema genera una factura de suscripción basada en el plan activo del Tenant[cite: 3].
2. **Notificación de Cobro:** Envío automático de factura proforma.
3. **Procesamiento de Pago:** Integración con pasarelas de pago o registro de transferencia manual.
4. **Estado de Cuenta:** El administrador del Tenant puede descargar sus facturas de uso del software desde el módulo de configuración.

## 3. Casos de Uso
* UC-24: Generación masiva de facturas de suscripción.
* UC-25: Bloqueo automático por mora.