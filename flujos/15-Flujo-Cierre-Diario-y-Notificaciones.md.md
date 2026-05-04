# Flujo 15: Cierre Diario y Notificaciones Administrativas

## 1. Descripción
Proceso automatizado y manual para consolidar la operación del día y notificar a los dueños del negocio (Tenants).

## 2. Pasos del Flujo
1. **Conciliación de Caja:** El cajero ejecuta el cierre de su turno. El sistema suma facturas pagadas vs. métodos de pago[cite: 3].
2. **Resumen de Operación:** 
   - **Clínica:** Total de citas atendidas vs. canceladas.
   - **Laboratorio:** Total de órdenes creadas vs. resultados validados.
3. **Generación de Reporte:** El `Reporting API` crea un resumen ejecutivo en PDF[cite: 3].
4. **Envío de Notificación:** El sistema dispara un correo electrónico o mensaje push al Administrador del Tenant con el balance del día.

## 3. Casos de Uso
* UC-30: Cierre de caja por sucursal.
* UC-31: Envío automático de reporte de cierre (Task Scheduler).