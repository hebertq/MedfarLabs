# Flujo 08: Cuentas de Pacientes y Seguros

## 1. Descripción
Liquidación de los servicios prestados durante la atención médica, diferenciando lo que paga el paciente y lo que cubre el seguro.

## 2. Pasos del Flujo y Catálogos Unificados
1. **Carga de Cargos:** El sistema acumula los costos de la consulta, materiales usados y órdenes generadas.
2. **Validación de Seguro:** Verificación de cobertura y cálculo de copago/deducible basándose en la configuración de aseguradoras (parametrizada vía `mst_catalog_detail`).
3. **Multimoneda y Catálogos:** Las tasas de impuestos, los tipos de moneda y el estado de la cuenta se gestionan utilizando la arquitectura central de catálogos compartidos, evitando tablas quemadas por módulo.
4. **Emisión de Factura:** Generación del documento fiscal en el módulo `Billing`.
5. **Gestión de Reclamos:** (Opcional) Registro de facturas enviadas a la aseguradora para cobro posterior.

## 3. Casos de Uso Relacionados
* UC-18: Liquidación de cuenta de paciente.
* UC-19: Gestión de convenios con aseguradoras.