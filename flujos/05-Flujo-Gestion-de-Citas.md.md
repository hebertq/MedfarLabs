# Flujo 05: Gestión de Citas y Agenda Médica

## 1. Descripción
Administración de la disponibilidad de los médicos y el proceso de reserva para asegurar una atención ordenada.

## 2. Actores
* Paciente (vía portal o teléfono)
* Recepcionista
* Médico (Consulta de agenda)

## 3. Pasos del Flujo (Happy Path)
1. **Consulta de Disponibilidad:** El sistema filtra por especialidad, médico y sucursal (Tenant)[cite: 3].
2. **Reserva:** Se crea el registro de la cita vinculando al `PatientId` con un bloque de tiempo.
3. **Confirmación:** Envío de notificación (Correo/SMS) y cambio de estado a "Confirmada".
4. **Check-in:** El día de la cita, la recepcionista marca al paciente como "En Espera" al llegar a la clínica.

## 4. Casos de Uso Relacionados
* UC-11: Configuración de horarios por médico.
* UC-12: Cancelación y reprogramación.