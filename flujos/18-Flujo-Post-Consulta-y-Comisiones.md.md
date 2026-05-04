# Flujo 18: Eventos Post-Consulta y Acumulación SaaS

## 1. Descripción
Define las acciones automáticas que se disparan al finalizar una atención médica, impactando la facturación del Tenant y el esquema de suscripción del Doctor.

## 2. Proceso de Eventos (Efecto Dominó)

### Paso 1: Disparador
El Médico ejecuta el `CompleteConsultationCommand` en el módulo `Care`.

### Paso 2: Publicación del Evento `ConsultationFinalizedEvent`
Este evento viaja por el `Mediator` y activa tres procesos secundarios:

1. **Generación de Cargo (Billing):**
   - El sistema inserta un ítem pendiente de cobro en la cuenta del paciente[cite: 3].
   - Si la consulta tiene un costo base, se suma al balance de la factura unificada[cite: 3].

2. **Acumulación para Suscripción (SaaS):**
   - Si el plan del Doctor es "Pago por Uso" o basado en volumen, el sistema suma +1 al contador de consultas del mes[cite: 3].
   - Esto impacta directamente en el Flujo 12 (Cobro de Planes) al final del periodo.

3. **Actualización de Historial (Identity/Care):**
   - Se marca al paciente como "Atendido" y se libera el espacio en la agenda para estadísticas de eficiencia[cite: 3].

## 3. Reglas Técnicas
* **Atomicidad:** Si el registro en el historial falla, el evento de cobro NO debe generarse (transaccionalidad).
* **Idempotencia:** Si el médico guarda dos veces por error, el sistema solo debe sumar una consulta al contador de suscripción.