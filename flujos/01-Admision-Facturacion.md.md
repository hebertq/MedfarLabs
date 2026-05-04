# Flujo 01: Admisión, Facturación y Orden

## 1. Descripción
Proceso de ingreso del paciente, selección de servicios, validación de disponibilidad y formalización del cobro para habilitar el trabajo de laboratorio.

## 2. Actores
* Recepcionista / Cajero
* Paciente
* Sistema (Módulos: Care, Inventory, Billing)

## 3. Pasos del Flujo (Happy Path)
1. **Identificación:** Se busca o registra al paciente en el módulo `Care`[cite: 3].
2. **Selección de Exámenes:** Se agregan servicios al carrito. El sistema consulta a `Inventory` para asegurar que hay insumos/reactivos disponibles[cite: 3].
3. **Tasación:** Se aplican reglas de precio (tarifas por convenio o cliente) según el `Tenant`[cite: 3].
4. **Facturación:** Se genera la factura (`Invoice`) en el módulo `Billing`[cite: 3].
5. **Pago:** Registro del ingreso (Efectivo, Tarjeta, Seguro).
6. **Disparo de Orden:** Al confirmarse el pago (o crédito), se emite el comando para crear la `LabOrder` en el módulo `Laboratory`[cite: 3].

## 4. Casos de Uso Relacionados
* UC-01: Registro de Paciente.
* UC-02: Validación de Stock en tiempo real.
* UC-03: Emisión de factura y recibo.