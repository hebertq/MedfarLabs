# Flujo 19: Órdenes de Referencia y Prospección de Laboratorio

## 1. Descripción
Mecanismo de comunicación activa cuando un médico refiere a un paciente a un laboratorio específico dentro (o fuera) de la red del Tenant.

## 2. Pasos del Flujo de Referencia

### Paso 1: Selección de Destino
Durante la consulta, el médico genera una orden de laboratorio y selecciona un laboratorio destino desde un buscador alimentado por el módulo `Auth/Tenancy`[cite: 3].

### Paso 2: Generación del Evento `LabReferralCreatedEvent`
A diferencia de una orden interna, este evento genera una "Prospección" (Lead):
1. **Notificación al Laboratorio:** El laboratorio seleccionado recibe una alerta en su PWA: *"Nuevo paciente referido por Dr. [Nombre]"*[cite: 3].
2. **Transferencia de Datos:** El laboratorio recibe acceso temporal a los datos de contacto y la orden técnica (pero no a la historia clínica completa por privacidad)[cite: 3].

### Paso 3: Contacto Proactivo
El laboratorio, al recibir el Webhook (Flujo 17), puede disparar un proceso de CRM:
- Enviar un mensaje al paciente: *"Hola [Nombre], el Dr. [X] nos envió su orden. ¿Desea agendar su cita para la toma de muestra?"*[cite: 3].

## 3. Beneficios del Flujo
* **Conversión:** Evita que el paciente "pierda" la orden o se vaya a la competencia.
* **Trazabilidad:** El médico puede ver en su panel si el paciente cumplió con los exámenes referidos (cierre del ciclo diagnóstico).