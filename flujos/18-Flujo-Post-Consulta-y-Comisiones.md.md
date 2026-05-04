# Flujo 18: Eventos Post-Consulta y Acumulación SaaS

## 1. Descripción
Define las acciones automáticas que se disparan al finalizar una atención médica, impactando la facturación del Tenant y el esquema de suscripción del Doctor.

## 2. Proceso de Eventos (Event-Driven Architecture)

### Paso 1: Disparador
El Médico ejecuta el `CompleteConsultationCommand` en el módulo `Care`.

### Paso 2: Publicación del Evento `ConsultationFinalizedEvent`
Este evento viaja por el `Mediator` y activa de forma asíncrona tres procesos secundarios:

1. **Generación de Cargo (Billing):**
   - Un handler escucha el evento e inserta un ítem pendiente de cobro en la cuenta del paciente.
   - Si la consulta tiene un costo base, se suma al balance de la factura unificada para su posterior pago en el Flujo 09.

2. **Acumulación para Suscripción (SaaS):**
   - Si el plan del Doctor es "Pago por Uso" o basado en volumen, se emite un `OrgBillingEvent` para sumar +1 al contador de consultas del mes.
   - Esto impacta directamente en el Flujo 12 (Cobro de Planes) al final del periodo.

3. **Actualización de Historial (Identity/Care):**
   - Se marca al paciente como "Atendido" y se libera el espacio en la agenda para estadísticas de eficiencia.

## 3. Reglas Técnicas
* **Atomicidad:** Si el registro en el historial falla, la consulta no debe completarse (Transaccionalidad en la base de datos).
* **Idempotencia:** Si el médico guarda dos veces por error, el `IdempotencyRepository` captura el `RequestId` repetido y asegura que el sistema solo sume una consulta al contador de suscripción y no genere cobros duplicados al paciente.