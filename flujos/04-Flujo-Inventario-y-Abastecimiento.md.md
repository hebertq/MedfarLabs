# Flujo 04: Control de Inventario y Reactivos

## 1. Descripción
Gestión de stock de insumos médicos y reactivos químicos vinculados a la producción del laboratorio.

## 2. Pasos del Flujo y Patrón Outbox
1. **Entrada de Mercancía:** Registro de compras y lotes (con fechas de vencimiento).
2. **Consumo Event-Driven:** Al validar un resultado de laboratorio, se dispara el evento `LabTestValidatedEvent`. El handler asociado ejecuta el `ConsumeReagentsCommand` para rebajar el inventario.
3. **Patrón Outbox:** Para garantizar que el inventario se rebaje incluso si el módulo de inventario está temporalmente caído, la instrucción de rebaja se guarda en la tabla `OutboxMessages` dentro de la transacción de validación. Un proceso en background (Worker) despacha los mensajes al bus garantizando "Entrega al menos una vez" (At-Least-Once Delivery).
4. **Alertas:** Notificación asíncrona de stock bajo o productos próximos a vencer cuando el stock traspasa el umbral mínimo.

## 3. Casos de Uso Relacionados
* UC-09: Gestión de Lotes y Vencimientos.
* UC-10: Ajuste de inventario por consumo técnico.