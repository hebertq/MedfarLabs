# Flujo 14: Control de Acceso Basado en Roles (RBAC) y Acciones

## 1. Descripción
Mecanismo que valida si un usuario tiene el permiso jerárquico para ejecutar un `Command` o acceder a una `Query` específica en el Core, garantizando la trazabilidad estricta del acceso a la información.

## 2. Niveles de Autorización
1. **Tenant Level:** Verifica si el suscriptor tiene el módulo activo (según su Plan).
2. **Role Level:** Verifica si el rol (ej. Bioanalista, Recepcionista, Médico) tiene la "Acción" permitida en la BD.
3. **Owner Level & Cross-Tenant Audit:** Verifica si el usuario tiene permiso sobre el registro específico. Si un usuario de una "Organización B" accede al "Resumen Público" de un paciente atendido por la "Organización A", se considera un cruce de Tenant autorizado, pero auditado.

## 3. Proceso de Validación y Auditoría (Native AOT)
1. **Intercepción:** Cada petición llega al API con un Token JWT que contiene el `TenantId` y los `Roles`.
2. **Middleware de Privacidad (Pipeline Behavior):** Antes de resolver la consulta, el sistema intercepta la petición, extrae el contexto del usuario y decide si aplicar un filtrado de Data Shaper (Proyección Condicional).
3. **Ejecución:** Si es válido, el `Mediator` procesa el comando; de lo contrario, devuelve un `403 Forbidden`.
4. **Trazabilidad AOT-Compatible:** Toda acción exitosa se registra asíncronamente en el `ActionEventRepository` almacenando el `payload` en formato `jsonb` utilizando el `MedfarLabsJsonSerializerContext`.
5. **Log de Acceso a Expediente Ajeno:** Si ocurrió un acceso "Cross-Tenant", el Interceptor de Auditoría inserta un log específico en `AccessLogs` capturando: `UsuarioId`, `PacienteId`, `OrganizaciónId_Visitante` y la Fecha/Hora exacta del acceso.

## 4. Casos de Uso
* UC-28: Asignación de menús dinámicos en la PWA según el rol.
* UC-29: Auditoría de acciones (Log de quién ejecutó qué acción y cuándo).