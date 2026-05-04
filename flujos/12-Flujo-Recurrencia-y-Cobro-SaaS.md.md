# Flujo 12: Facturación Recurrente (Billing SaaS)

## 1. Descripción
Gestión de la cartera de clientes de MedFarLab, procesamiento de pagos mensuales y suspensión por falta de pago.

## 2. Pasos del Flujo y Eventos de Facturación
1. **Consolidación Diaria/Mensual:** El sistema procesa todos los eventos de consumo (ej. almacenamiento extra o exceso de médicos) registrados como `OrgBillingEvent` a lo largo del mes.
2. **Corte Mensual:** El sistema consolida los eventos generados y emite una factura de suscripción basada en el plan activo del Tenant.
3. **Notificación de Cobro:** Envío automático de factura proforma vía email/webhook.
4. **Procesamiento de Pago:** Integración con pasarelas de pago o registro de transferencia manual.
5. **Estado de Cuenta:** El administrador del Tenant puede descargar sus facturas de uso del software desde el módulo de configuración.

## 3. Casos de Uso Relacionados
* UC-24: Generación masiva de facturas de suscripción.
* UC-25: Bloqueo automático por mora.