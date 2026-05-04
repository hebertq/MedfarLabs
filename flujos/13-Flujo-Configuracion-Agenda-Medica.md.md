# Flujo 13: Configuración de Disponibilidad Médica

## 1. Descripción
Definición de la capacidad operativa de la clínica mediante la configuración de turnos y horarios de los especialistas.

## 2. Pasos del Flujo y Manejo Global
1. **Perfil del Especialista:** El médico (o asistente) define sus especialidades y servicios.
2. **Definición de Jornada (TimeZone Aware):** Configuración de días de la semana y rangos horarios. El sistema asocia esta configuración a la `TimeZone` de la sucursal del Tenant para asegurar precisión global en caso de citas internacionales (Telemedicina).
3. **Intervalos de Atención:** Definición de la duración por cita (ej. 20 min para consulta general, 40 min para especialidad).
4. **Bloqueos y Excepciones:** Registro de vacaciones, días feriados o congresos donde no habrá disponibilidad.

## 3. Casos de Uso Relacionados
* UC-26: Setup de matriz de horarios por sucursal/médico.
* UC-27: Configuración de duración de servicios.