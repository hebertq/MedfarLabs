# ⚕️ Módulo Care

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Care**.

## Endpoints Disponibles

### `POST` `/api/Care/5001` (RegistrarConsulta)

**Payload DTO Requerido:** `ConsultationRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `expediente_id` | `Int64` | 🟢 Sí |
| `medico_id` | `Int64` | 🟢 Sí |
| `datos_subjetivos` | `String` | 🟡 Opcional |
| `datos_objetivos` | `String` | 🟡 Opcional |
| `analisis_medico` | `String` | 🟡 Opcional |
| `plan_tratamiento` | `String` | 🟡 Opcional |
| `signos_vitales` | `VitalSignsDTO` | 🟢 Sí |
| `diagnosticos` | `List<String>` | 🟡 Opcional |
| `recetas` | `List<PrescriptionItemDTO>` | 🟡 Opcional |
| `ordenes_laboratorio` | `List<LabOrderDTO>` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

