# Flujo 02: Gestión de Muestras y Resultados

## 1. Descripción
Control de la fase pre-analítica (toma) y analítica (procesamiento) de los exámenes solicitados.

## 2. Actores
* Técnico de Laboratorio / Flebotomista
* Bioanalista / Especialista
* Supervisor / Firmante

## 3. Pasos del Flujo (Happy Path)
1. **Toma de Muestra:** El técnico identifica al paciente y marca los ítems de la `LabOrder` como "Colectados" (Registra fecha/hora y responsable)[cite: 3].
2. **Recepción en Área Técnica:** Las muestras se reciben en las áreas de trabajo (Hematología, Química, etc.). El estado cambia a "En Proceso".
3. **Carga de Resultados:** 
   - Ingreso de valores manuales o captura automática.
   - Validación automática contra rangos de referencia (Edad/Sexo) definidos en el Core[cite: 3].
4. **Revisión y Firma:** Un supervisor valida los resultados. El estado cambia a "Completado/Validado".

## 4. Casos de Uso Relacionados
* UC-04: Registro de toma de muestra (Soporte Offline PWA)[cite: 3].
* UC-05: Captura de resultados y comparación de rangos.
* UC-06: Firma digital de reportes.