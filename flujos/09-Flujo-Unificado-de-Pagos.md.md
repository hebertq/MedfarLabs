# Flujo 09: Procesamiento Unificado de Pagos (Clínica y Lab)

## 1. Descripción
Proceso centralizado para la liquidación de cargos financieros generados por cualquier unidad de negocio del sistema MedFarLab.

## 2. Orígenes del Cargo (Estructura de Datos)
El sistema debe ser capaz de agrupar cargos de distintas fuentes antes de facturar:
* **Fuente A (Care):** Consultas, procedimientos, honorarios médicos.
* **Fuente B (Laboratory):** Perfiles, exámenes individuales, recargos por toma a domicilio.
* **Fuente C (Inventory):** Venta directa de medicamentos o insumos.

## 3. Pasos del Flujo (Happy Path)

### Paso 1: Consolidación de la Cuenta
El cajero busca al paciente y el sistema muestra todos los ítems con estado `Pendiente de Pago`. 
* *Regla:* Se pueden marcar/desmarcar ítems para pagos parciales.

### Paso 2: Aplicación de Beneficios (Seguros/Convenios)
* El sistema aplica el descuento según el plan del paciente (SaaS Multitenant)[cite: 3].
* Se calcula el **Copago** (lo que paga el paciente) y la **Cuenta por Cobrar** (lo que se factura a la aseguradora)[cite: 3].

### Paso 3: Selección de Método de Pago
* Soporte para múltiples formas: Efectivo, Tarjeta, Transferencia o Crédito.
* Registro de referencia de transacción para auditoría.

### Paso 4: Generación del Documento Fiscal e Impacto en Operaciones
Al confirmar el `ProcessPaymentCommand`:
1. **Billing:** Se genera la `Invoice` y se marca como `Paid`[cite: 3].
2. **Laboratory:** Si hay una orden vinculada, su estado cambia de `PendingPayment` a `AwaitingSample` de forma automática[cite: 3].
3. **Care:** Se libera la consulta para que el médico pueda iniciar la atención si el pago era requisito.

## 4. Componentes Técnicos (Capa Aplicación)
* **Command:** `CollectPendingChargesQuery` (Agrupa cargos de todas las tablas).
* **Command:** `GenerateUnifiedInvoiceCommand`
* **Event:** `PaymentConfirmedIntegrationEvent` (Notifica a Lab y Clínica que pueden proceder).

## 5. Ventajas de la Unificación
* **Vista Única del Cliente:** El paciente recibe una sola factura por todos los servicios del día.
* **Cierre de Caja Simplificado:** El cajero no tiene que cerrar "Caja de Lab" y "Caja de Clínica" por separado.
* **Escalabilidad:** Si mañana agregas "Rayos X", solo lo conectas como una nueva fuente de cargos al mismo flujo de pagos.