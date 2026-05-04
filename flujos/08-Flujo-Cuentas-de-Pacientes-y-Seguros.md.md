# Flujo 08: Facturación Clínica y Gestión de Copagos

## 1. Descripción
Liquidación de los servicios prestados durante la atención médica, diferenciando lo que paga el paciente y lo que cubre el seguro.

## 2. Pasos del Flujo (Happy Path)
1. **Carga de Cargos:** El sistema acumula los costos de la consulta, materiales usados y órdenes generadas.
2. **Validación de Seguro:** Verificación de cobertura y cálculo de copago/deducible[cite: 3].
3. **Emisión de Factura:** Generación del documento fiscal en el módulo `Billing`[cite: 3].
4. **Gestión de Reclamos:** (Opcional) Registro de facturas enviadas a la aseguradora para cobro posterior.

## 3. Casos de Uso Relacionados
* UC-18: Liquidación de cuenta de paciente.
* UC-19: Gestión de convenios con aseguradoras.