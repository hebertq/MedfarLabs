# ⚕️ Módulo Clinical

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Clinical**.

## Endpoints Disponibles

### `POST` `/api/Clinical/4113` (ConfigurarMedico)

**Payload DTO Requerido:** `DoctorConfigurationRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `doctor_user_id` | `Int64` | 🟢 Sí |
| `available_hours` | `String` | 🟡 Opcional |
| `min_consultation_time_mins` | `Int32` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Clinical/4005` (RegistrarPaciente)

**Payload DTO Requerido:** `PatientRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `persona_id` | `Int64` | 🟢 Sí |
| `codigo_interno` | `String` | 🟡 Opcional |
| `notas_auditoria` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Clinical/4006` (RegistrarSignosVitales)

**Payload DTO Requerido:** `VitalSignsRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `paciente_id` | `Int64` | 🟢 Sí |
| `presion_sistolica` | `Int32` | 🟢 Sí |
| `presion_diastolica` | `Int32` | 🟢 Sí |
| `frecuencia_cardiaca` | `Int32` | 🟢 Sí |
| `temperatura` | `Decimal` | 🟢 Sí |
| `peso_kg` | `Decimal` | 🟢 Sí |
| `altura_cm` | `Decimal` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Clinical/4114` (GetPatientAlerts)

**Payload DTO Requerido:** `GetPatientAlertsRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene `IEnumerable<PatientAlertResponseDTO>`.

### `POST` `/api/Clinical/4115` (CreatePatientAlert)

**Payload DTO Requerido:** `CreatePatientAlertRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `alert_type_id` | `Int32` | 🟢 Sí |
| `severity` | `String` | 🟢 Sí |
| `message` | `String` | 🟢 Sí |
| `source_type_id` | `Int32` | 🟢 Sí |
| `source_id` | `Int64` | 🟡 Opcional |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene `PatientAlertResponseDTO`.

### `POST` `/api/Clinical/4116` (AcknowledgeAlert)

**Payload DTO Requerido:** `AcknowledgeAlertRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `alert_id` | `Int64` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa.

### `POST` `/api/Clinical/4120` (GetPatientContacts)

**Payload DTO Requerido:** `GetPatientContactsRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene `IEnumerable<PatientContactResponseDTO>`.

### `POST` `/api/Clinical/4121` (CreatePatientContact)

**Payload DTO Requerido:** `CreatePatientContactRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `contact_type_id` | `Int32` | 🟡 Opcional |
| `full_name` | `String` | 🟢 Sí |
| `phone` | `String` | 🟡 Opcional |
| `email` | `String` | 🟡 Opcional |
| `relationship` | `String` | 🟡 Opcional |
| `is_primary` | `Boolean` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene `PatientContactResponseDTO`.

### `POST` `/api/Clinical/4122` (UpdatePatientContact)

**Payload DTO Requerido:** `UpdatePatientContactRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `id` | `Int64` | 🟢 Sí |
| `contact_type_id` | `Int32` | 🟡 Opcional |
| `full_name` | `String` | 🟢 Sí |
| `phone` | `String` | 🟡 Opcional |
| `email` | `String` | 🟡 Opcional |
| `relationship` | `String` | 🟡 Opcional |
| `is_primary` | `Boolean` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa.

### `POST` `/api/Clinical/4123` (DeletePatientContact)

**Payload DTO Requerido:** `GenericIdRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `id` | `Int64` | 🟢 Sí |
| `organization_id` | `Int64` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa.
