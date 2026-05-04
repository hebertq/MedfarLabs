# Flujo 06: Consulta Médica e Historia Clínica (HC)

## 1. Descripción
Registro del acto médico, anamnesis, examen físico y diagnóstico, manteniendo la integridad de la Historia Clínica Única bajo estrictos estándares de privacidad ética inter-organizacional.

## 2. Apertura de Expediente y Proyección Condicional (Data Shaper)
Cuando un médico consulta el historial de un paciente, el `QueryHandler` en el Core evalúa el `OrganizationId` del médico solicitante contra el `OrganizationId` dueño del registro:
*   **Misma Organización:** El sistema proyecta el `FullClinicalRecordDto` incluyendo notas de evolución, impresiones diagnósticas y examen físico detallado.
*   **Organización Visitante:** El Data Shaper actúa como restricción de infraestructura y proyecta un `PublicClinicalRecordDto` (Resumen Público), revelando **únicamente** patologías previas (CIE-10), alergias y grupo sanguíneo para salvaguardar la privacidad.

## 3. Actores
* Médico / Especialista
* Enfermero (Triaje)

## 4. Pasos del Flujo e Idempotencia
1. **Triaje:** Registro de signos vitales (Presión, peso, temperatura) por enfermería.
2. **Anamnesis:** El médico registra el motivo de consulta y antecedentes.
3. **Examen Físico:** Registro detallado por sistemas.
4. **Diagnóstico:** Selección de códigos (CIE-10 o similares) y plan de tratamiento.
5. **Cierre de Evolución e Idempotencia:** Se ejecuta el `CompleteConsultationCommand`. El sistema usa el `IdempotencyRepository` enviando un `RequestId` único para asegurar que, si el médico hace doble clic o la red falla, la consulta no se cierre ni se cobre dos veces.
6. **Firma y Bloqueo:** Tras cerrarse, la nota médica queda bloqueada para ediciones y se asocia una firma digital básica del médico según normas legales.

## 5. Casos de Uso Relacionados
* UC-13: Registro de Signos Vitales.
* UC-14: Evolución de Historia Clínica.
* UC-15: Visualización de historial de consultas previas.