# ⚕️ Módulo Billing

Documentación Oficial de Contratos de Datos y Operaciones genéricas del Dispatcher para el módulo **Billing**.

## Endpoints Disponibles

### `POST` `/api/Billing/3001` (GenerarFactura)

**Payload DTO Requerido:** `InvoiceRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `patient_id` | `Int64` | 🟢 Sí |
| `subtotal` | `Decimal` | 🟢 Sí |
| `tax` | `Decimal` | 🟢 Sí |
| `total` | `Decimal` | 🟢 Sí |
| `numero_factura` | `String` | 🟡 Opcional |
| `notas_auditoria` | `String` | 🟡 Opcional |
| `items` | `List<InvoiceItemRequestDTO>` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Billing/3006` (RegistrarPago)

**Payload DTO Requerido:** `PaymentRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `InvoiceId` | `Int64` | 🟢 Sí |
| `PaymentMethodId` | `Int32` | 🟢 Sí |
| `AmountPaid` | `Decimal` | 🟢 Sí |
| `TransactionReference` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `GET` `/api/Billing/3005` (BuscarFactura)

**Payload DTO Requerido:** Se codifica como JSON string en el Query Parameter (`?payload=...`) equivalente al modelo `SearchInvoiceRequestDTO`.
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `fecha_inicio` | `DateTime` | 🟡 Opcional |
| `fecha_fin` | `DateTime` | 🟡 Opcional |
| `patient_id` | `Int64` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Billing/3007` (SuscribirOrganizacion)

**Payload DTO Requerido:** `SubscriptionRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `plan_id` | `Int32` | 🟢 Sí |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).

### `POST` `/api/Billing/3008` (PagarSuscripcion)

**Payload DTO Requerido:** `PaySubscriptionRequestDTO` (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| `invoice_id` | `Int64` | 🟢 Sí |
| `amount_paid` | `Decimal` | 🟢 Sí |
| `payment_method` | `String` | 🟡 Opcional |

**Respuestas Esperadas y Filtros Estándar (BaseResponse):**
- `200 OK`: Operación Exitosa. `Data` contiene la respuesta.
- `400 Bad Request`: Error de Validación (BusinessValidationException). Listado en array `Errors`.
- `401 Unauthorized`: Denegación de acceso o credenciales incorrectas.
- `404 Not Found`: El recurso solicitado a operar no existe (KeyNotFoundException).
- `500 Server Error`: Fallo interno no capturado o en Base de Datos (PersistenceException).


### \GET\ \/api/Billing/3013\ (GetSaasPlans)

**Payload DTO Requerido:** (Query Parameter \?payload=...\) \GetSaasPlansRequestDTO\
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| \organization_type_id\ | \Int32\ | 🟡 Opcional |

### \GET\ \/api/Billing/3014\ (GetSaasPlanById)

**Payload DTO Requerido:** (Query Parameter \?payload=...\) \GetSaasPlanByIdRequestDTO\
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| \plan_id\ | \Int32\ | 🟢 Sí |

### \POST\ \/api/Billing/3015\ (CreateSaasPlan)

**Payload DTO Requerido:** \CreateSaasPlanRequestDTO\ (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| \
ame\ | \String\ | 🟢 Sí |
| \monthly_price\ | \Decimal\ | 🟢 Sí |
| \nnual_price\ | \Decimal\ | 🟢 Sí |
| \max_branches\ | \Int32\ | 🟢 Sí |
| \max_users\ | \Int32\ | 🟢 Sí |
| \organization_type_id\ | \Int32\ | 🟢 Sí |
| \is_pay_per_use\ | \Boolean\ | 🟢 Sí |
| \eatures\ | \List<String>\ | 🟡 Opcional |

### \POST\ \/api/Billing/3016\ (UpdateSaasPlan)

**Payload DTO Requerido:** \UpdateSaasPlanRequestDTO\ (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| \plan_id\ | \Int32\ | 🟢 Sí |
| *(Mismos campos que CreateSaasPlan)* | | |

### \GET\ \/api/Billing/3017\ (GetSubscriptionStatus)

**Payload DTO Requerido:** (Query Parameter \?payload=...\) \GetSubscriptionStatusRequestDTO\ (Sin parámetros extra requeridos aparte del contexto del usuario)

### \GET\ \/api/Billing/3018\ (GetSubscriptionInvoices)

**Payload DTO Requerido:** (Query Parameter \?payload=...\) \GetSubscriptionInvoicesRequestDTO\
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| \page\ | \Int32\ | 🟡 Opcional |
| \page_size\ | \Int32\ | 🟡 Opcional |

### \POST\ \/api/Billing/3019\ (CloseBillingPeriod)

**Payload DTO Requerido:** \CloseBillingPeriodRequestDTO\ (JSON Body)
(Sin campos extra en el cuerpo)


### GET /api/Billing/3020 (GetInvoicePayments)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetInvoicePaymentsRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| invoice_id | Int64 | 🟢 Sí |

### GET /api/Billing/3021 (GetPatientBalance)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetPatientBalanceRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| patient_id | Int64 | 🟢 Sí |

### GET /api/Billing/3022 (GetDailyClosing)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetDailyClosingRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
|  ranch_id | Int64 | 🟢 Sí |
| date | DateTime | 🟢 Sí |

### GET /api/Billing/3010 (GetAllInvoices)

**Payload DTO Requerido:** (Query Parameter ?payload=...) GetAllInvoicesRequestDTO
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| page | Int32 | 🟡 Opcional |
| page_size | Int32 | 🟡 Opcional |

### POST /api/Billing/3011 (AnularFactura)

**Payload DTO Requerido:** CancelInvoiceRequestDTO (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| invoice_id | Int64 | 🟢 Sí |
| cancel_reason | String | 🟡 Opcional |

### POST /api/Billing/3012 (ActualizarFactura)

**Payload DTO Requerido:** UpdateInvoiceRequestDTO (JSON Body)
| Campo JSON | Tipo | Requerido |
| :--- | :--- | :---: |
| invoice_id | Int64 | 🟢 Sí |
| subtotal | Decimal | 🟢 Sí |
| tax | Decimal | 🟢 Sí |
| total | Decimal | 🟢 Sí |
| items | List<InvoiceItemRequestDTO> | 🟢 Sí |
