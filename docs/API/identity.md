# ⚕️ Módulo Identity

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Identity**.

## Endpoints Disponibles

### `POST` `/api/Identity/2005` (RegistrarOrganizacion)

**Payload DTO Requerido:** `OrganizationRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `nombre_organizacion` | `String` | 🟡 Opcional |
| `numero_fiscal` | `String` | 🟡 Opcional |
| `notas_auditoria` | `String` | 🟡 Opcional |
| `is_active` | `Boolean` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Identity/2001` (RegistrarPersona)

**Payload DTO Requerido:** `PersonRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `primer_nombre` | `String` | 🟡 Opcional |
| `segundo_nombre` | `String` | 🟡 Opcional |
| `primer_apellido` | `String` | 🟡 Opcional |
| `segundo_apellido` | `String` | 🟡 Opcional |
| `fecha_nacimiento` | `DateTime` | 🟢 Sí |
| `genero_id` | `Int32` | 🟢 Sí |
| `pais_nacimiento_id` | `Int32` | 🟢 Sí |
| `correo` | `String` | 🟡 Opcional |
| `telefono` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

