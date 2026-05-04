# Flujo 07: Prescripciones y Órdenes de Servicio

## 1. Descripción
Generación de documentos derivados de la consulta: recetas de medicamentos y órdenes para exámenes diagnósticos.

## 2. Pasos del Flujo y Generación de PDF
1. **Prescripción Farmacéutica:** El médico busca medicamentos en el catálogo de `Inventory` y define dosis.
2. **Orden de Apoyo Diagnóstico:** Si se requieren exámenes, el médico genera una orden que dispara automáticamente una solicitud (o "Draft") al módulo `Laboratory`.
3. **Impresión/Digitalización y Código QR:** Al invocar al `Reporting.Api`, el sistema genera un PDF firmado. Este PDF incluye un Código QR único de validación, permitiendo que las farmacias puedan escanearlo y confirmar la veracidad de la receta médica en línea.
4. **Interoperabilidad:** La orden de laboratorio queda visible en el Flujo 01 (Admisión de Lab) para evitar doble registro.

## 3. Casos de Uso Relacionados
* UC-16: Emisión de Receta Médica.
* UC-17: Solicitud Inter-departamental (Laboratorio/Imagen).