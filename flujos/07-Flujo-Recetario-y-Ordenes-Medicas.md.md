# Flujo 07: Prescripciones y Órdenes de Servicio

## 1. Descripción
Generación de documentos derivados de la consulta: recetas de medicamentos y órdenes para exámenes diagnósticos.

## 2. Pasos del Flujo (Happy Path)
1. **Prescripción Farmacéutica:** El médico busca medicamentos en el catálogo de `Inventory` y define dosis[cite: 3].
2. **Orden de Apoyo Diagnóstico:** Si se requieren exámenes, el médico genera una orden que dispara automáticamente una solicitud al módulo `Laboratory`[cite: 3].
3. **Impresión/Digitalización:** Generación de PDF (vía Reporting API) con firma y sello para el paciente[cite: 3].
4. **Interoperabilidad:** La orden de laboratorio queda visible en el Flujo 01 (Admisión de Lab) para evitar doble registro.

## 3. Casos de Uso Relacionados
* UC-16: Emisión de Receta Médica.
* UC-17: Solicitud Inter-departamental (Laboratorio/Imagen).