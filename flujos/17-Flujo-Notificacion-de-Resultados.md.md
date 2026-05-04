# Flujo 17: Notificaciones de Resultados (Email & Webhook PWA)

## 1. Descripción
Mecanismo de mensajería automatizada que se dispara una vez que los resultados de laboratorio han sido validados y firmados, utilizando múltiples canales de entrega.

## 2. Actores
* **Sistema (Core/Application):** Disparador del evento.
* **Servicio de Email (SMTP/SendGrid):** Canal externo para el paciente.
* **PWA (Push Notifications):** Canal directo para el usuario móvil.
* **Webhook Service:** Interfaz para integración con terceros o actualización de estado en tiempo real.

## 3. Pasos del Flujo y Patrón Outbox

### Paso 1: Disparador (The Trigger)
Cuando el bioanalista o supervisor ejecuta el `ValidateOrderCommand` en el módulo de laboratorio y el estado de la orden cambia a `Validated`.

### Paso 2: Publicación del Evento (Domain Event)
El Core publica un evento interno llamado `LabTestValidatedEvent`. Este evento contiene `OrderId`, `PatientEmail` y `TenantId` (para branding de la clínica).

### Paso 3: Consumo Garantizado (Outbox Pattern)
Para asegurar que la notificación no se pierda si el servicio de correos falla temporalmente, los mensajes se encolan en una tabla Outbox. Luego, un worker asíncrono los procesa:

1. **Email Handler:** 
   - Solicita al `Reporting API` la generación del PDF.
   - Construye el cuerpo del correo usando una plantilla HTML personalizada por el Tenant.
   - Adjunta el resultado y lo envía al correo registrado del paciente.

2. **PWA/Webhook Handler:**
   - Envía un **Webhook** (POST Request) al endpoint de notificaciones.
   - Si el usuario tiene la PWA instalada y suscribió notificaciones push, el Service Worker de la PWA muestra la alerta: *"Sus resultados ya están listos. Haga clic para ver"*.

### Paso 4: Trazabilidad y Confirmación
El sistema registra en la base de datos el estado del envío: `NotificationSent`, `NotificationFailed` o `NotificationDelivered`.

## 4. Reglas de Negocio
* **Privacidad:** El PDF adjunto en el correo debe poder protegerse con contraseña (ej. últimos dígitos del DNI) según la configuración del Tenant.
* **Consentimiento:** El sistema debe verificar si el paciente aceptó recibir resultados por medios electrónicos en el "Flujo 01: Admisión".
* **Falla de Red:** Si el envío por Webhook falla, el sistema debe reintentar hasta 3 veces antes de marcarlo como error (Circuit Breaker con Polly).