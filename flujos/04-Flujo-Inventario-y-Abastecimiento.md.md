# Flujo 04: Control de Inventario y Reactivos

## 1. Descripción
Gestión de stock de insumos médicos y reactivos químicos vinculados a la producción del laboratorio.

## 2. Pasos del Flujo (Happy Path)
1. **Entrada de Mercancía:** Registro de compras y lotes (con fechas de vencimiento).
2. **Consumo Automático:** Al validar un resultado de laboratorio, el sistema descuenta automáticamente los insumos asociados según la configuración de la prueba.
3. **Alertas:** Notificación de stock bajo o productos próximos a vencer.

## 3. Casos de Uso Relacionados
* UC-09: Gestión de Lotes y Vencimientos.
* UC-10: Ajuste de inventario por consumo técnico.