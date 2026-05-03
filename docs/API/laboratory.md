# ⚕️ Módulo Laboratory

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Laboratory**.

## Endpoints Disponibles

### `POST` `/api/Laboratory/8000` (RegistrarResultado)

**Payload DTO Requerido:** `LabResultRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `orden_laboratorio_id` | `Int64` | 🟢 Sí |
| `estado_id` | `Int32` | 🟢 Sí |
| `datos_tecnicos_json` | `String` | 🟡 Opcional |
| `observaciones` | `String` | 🟡 Opcional |
| `notas_auditoria` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).


### GET /api/Laboratory/8015 (GetOrgTemplates)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetOrgTemplatesRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| include_global | Boolean | 🟡 Opcional |

### GET /api/Laboratory/8016 (GetTemplateWithConfig)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetTemplateWithConfigRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| 	emplate_id | Int64 | 🟢 Sí |

### POST /api/Laboratory/8017 (ResetTemplateToDefault)

**Payload DTO Requerido:** ResetTemplateRequestDTO (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| 	emplate_id | Int64 | 🟢 Sí |

### GET /api/Laboratory/8018 (GetGlobalTemplates)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetGlobalTemplatesRequestDTO

### GET /api/Laboratory/8019 (ViewTemplateItems)

**Payload DTO Requerido:** (Query Parameter ?payload=...) ViewTemplateItemsRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| 	emplate_id | Int64 | 🟢 Sí |

### POST /api/Laboratory/8006 (SaveServiceSampleConfigs)

**Payload DTO Requerido:** SaveServiceSampleConfigsRequestDTO (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| service_id | Int64 | 🟢 Sí |
| configs | List<ServiceSampleConfigDTO> | 🟢 Sí |

