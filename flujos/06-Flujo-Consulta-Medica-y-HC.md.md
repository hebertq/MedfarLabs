# Flujo 06: Consulta Médica e Historia Clínica (HC)

## 1. Descripción
Registro del acto médico, anamnesis, examen físico y diagnóstico, manteniendo la integridad de la Historia Clínica Única.

## 2. Actores
* Médico / Especialista
* Enfermero (Triaje)

## 3. Pasos del Flujo (Happy Path)
1. **Triaje:** Registro de signos vitales (Presión, peso, temperatura) por enfermería.
2. **Anamnesis:** El médico registra el motivo de consulta y antecedentes.
3. **Examen Físico:** Registro detallado por sistemas.
4. **Diagnóstico:** Selección de códigos (CIE-10 o similares) y plan de tratamiento.
5. **Cierre de Evolución:** Se firma la nota médica, la cual queda bloqueada para ediciones posteriores según normas legales[cite: 3].

## 4. Casos de Uso Relacionados
* UC-13: Registro de Signos Vitales.
* UC-14: Evolución de Historia Clínica.
* UC-15: Visualización de historial de consultas previas.