# Flujo 19: Órdenes de Referencia y Prospección de Laboratorio (Referral)

## 1. Descripción
Mecanismo de comunicación activa cuando un médico refiere a un paciente a un laboratorio específico dentro (o fuera) de la red del Tenant.

## 2. Pasos del Flujo de Referencia e Integración CRM

### Paso 1: Selección de Destino
Durante la consulta, el médico genera una orden de laboratorio y selecciona un laboratorio destino desde un buscador alimentado por el módulo `Auth/Tenancy`.

### Paso 2: Generación del Evento `LabReferralCreatedEvent`
A diferencia de una orden interna, este evento genera una "Prospección" (Lead) que puede integrarse externamente:
1. **Notificación al Laboratorio:** El laboratorio seleccionado recibe una alerta en su PWA: *"Nuevo paciente referido por Dr. [Nombre]"*.
2. **Transferencia de Datos Restringida (Data Shaper):** El laboratorio recibe acceso temporal al expediente. Para garantizar el secreto médico, el Core aplica una **Proyección Condicional**, enviando **únicamente** los datos demográficos y la orden específica (bloqueo total de la historia clínica, notas de evolución y diagnósticos de la clínica emisora).

### Paso 3: Contacto Proactivo (Webhooks/API)
El laboratorio, al recibir el Webhook asíncrono, puede disparar un proceso en su propio CRM (ej. HubSpot, Salesforce, etc.):
- Enviar un mensaje de WhatsApp automatizado al paciente: *"Hola [Nombre], el Dr. [X] nos envió su orden. ¿Desea agendar su cita para la toma de muestra?"*.

## 3. Beneficios del Flujo
* **Conversión y Retención:** Evita que el paciente "pierda" la orden física o decida irse a la competencia.
* **Trazabilidad de Referencias:** El médico puede ver en su panel si el paciente cumplió con los exámenes referidos, lo que cierra el ciclo diagnóstico. Además, permite calcular métricas de colaboración inter-laboratorio/clínica.