# ⚕️ Módulo Security

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Security**.

## Endpoints Disponibles

### `POST` `/api/Security/1003` (CrearGrupoRoles)

**Payload DTO Requerido:** `RoleGroupRequestDto` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `OrganizationId` | `Int64` | 🟢 Sí |
| `Name` | `String` | 🟡 Opcional |
| `Description` | `String` | 🟡 Opcional |
| `IsActive` | `Boolean` | 🟢 Sí |
| `RoleIds` | `List<Int32>` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Security/4125` (GetPatientAccessHistory)

**Payload DTO Requerido:** `GetAccessHistoryRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |
| `limit` | `Int32` | 🟡 Opcional |
| `offset` | `Int32` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene `IEnumerable<PatientAccessLogResponseDTO>`.

### `POST` `/api/Security/4126` (LogPatientAccess)

**Payload DTO Requerido:** `LogPatientAccessRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `user_id` | `Int64` | 🟢 Sí |
| `access_type` | `String` | 🟢 Sí |
| `resource_type` | `String` | 🟢 Sí |
| `resource_id` | `Int64` | 🟡 Opcional |
| `reason` | `String` | 🟡 Opcional |
| `ip_address` | `String` | 🟡 Opcional |
| `user_agent` | `String` | 🟡 Opcional |
| `session_id` | `String` | 🟡 Opcional |
| `trace_id` | `String` | 🟡 Opcional |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa.
