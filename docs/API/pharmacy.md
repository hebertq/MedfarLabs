# 💊 Módulo de Farmacia (Pharmacy)

Gestión de medicamentos, dispensación e inventario específico para fármacos y consumibles médicos.

## Endpoints Principales

### `GET /api/pharmacy/dashboard`
Obtiene las métricas y alertas de inventario (stock bajo y fechas de expiración cercanas) para surtir la vista principal de la farmacia.

**Query Parameters**
| Campo | Tipo | Requerido | Descripción |
| :--- | :--- | :---: | :--- |
| `branchId` | `Entero` | 🟢 Sí | ID de la sucursal de farmacia o almacén maestro. |

### `POST /api/pharmacy/restock`
Repone el stock de un medicamento específico.

**Payload DTO: `RestockMedicationCommand`**
| Campo | Tipo | Requerido | Descripción |
| :--- | :--- | :---: | :--- |
| `medicationId` | `Entero` | 🟢 Sí | ID interno del medicamento en la base de datos. |
| `quantity` | `Entero` | 🟢 Sí | Cantidad adquirida a sumar al `currentStock`. |
