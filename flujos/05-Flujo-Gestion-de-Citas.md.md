# Flujo 05: Gestión de Citas y Agenda Médica

## 1. Descripción
Administración de la disponibilidad de los médicos y el proceso de reserva para asegurar una atención ordenada.

## 2. Actores
* Paciente (vía portal o teléfono)
* Recepcionista
* Médico (Consulta de agenda)

## 3. Pasos del Flujo y Consideración de TimeZones
1. **Consulta de Disponibilidad:** El sistema filtra por especialidad, médico y sucursal. Se aplica la conversión de `TimeZone` del Tenant, ya que las fechas en la base de datos se almacenan en estricto `UTC`.
2. **Reserva:** Se crea el registro de la cita vinculando al `PatientId` con un bloque de tiempo y un estado inicial (ej. "Agendada").
3. **Confirmación:** Envío de notificación (Correo/SMS) mediante Outbox y cambio de estado a "Confirmada".
4. **Manejo de Estados:** El sistema soporta estados del catálogo `mst_catalog_detail` como "En Espera", "Atendido", "No-Show" (Paciente no asistió) o "Cancelada".
5. **Check-in:** El día de la cita, la recepcionista marca al paciente como "En Espera" al llegar a la clínica.

## 4. Casos de Uso Relacionados
* UC-11: Configuración de horarios por médico.
* UC-12: Cancelación y reprogramación.