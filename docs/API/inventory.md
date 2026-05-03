# ⚕️ Módulo Inventory

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Inventory**.

## Endpoints Disponibles

### `POST` `/api/Inventory/7000` (RegistrarServicio)

**Payload DTO Requerido:** `MedicalServiceRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `name` | `String` | 🟡 Opcional |
| `description` | `String` | 🟡 Opcional |
| `category_id` | `Int32` | 🟢 Sí |
| `precio_base` | `Decimal` | 🟢 Sí |
| `codigo_sku` | `String` | 🟡 Opcional |
| `notas_auditoria` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

