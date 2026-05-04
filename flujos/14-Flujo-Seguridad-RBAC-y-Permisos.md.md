# Flujo 14: Control de Acceso Basado en Roles (RBAC) y Acciones

## 1. Descripción
Mecanismo que valida si un usuario tiene el permiso jerárquico para ejecutar un `Command` o acceder a una `Query` específica en el Core[cite: 3].

## 2. Niveles de Autorización
1. **Tenant Level:** Verifica si el suscriptor tiene el módulo activo (según su Plan)[cite: 3].
2. **Role Level:** Verifica si el rol (ej. Bioanalista, Recepcionista, Médico) tiene la "Acción" permitida en la BD[cite: 3].
3. **Owner Level:** Verifica si el usuario tiene permiso sobre el registro específico (ej. un médico solo ve sus pacientes asignados).

## 3. Proceso de Validación en el Core
1. **Intercepción:** Cada petición llega al API con un Token JWT que contiene el `TenantId` y los `Roles`[cite: 3].
2. **Middleware de Autorización:** El Core consulta la tabla de permisos para validar si `Action_Create_Order` está permitida para ese rol[cite: 3].
3. **Ejecución:** Si es válido, el `Mediator` procesa el comando; de lo contrario, devuelve un `403 Forbidden`.

## 4. Casos de Uso
* UC-28: Asignación de menús dinámicos en la PWA según el rol[cite: 3].
* UC-29: Auditoría de acciones (Log de quién ejecutó qué acción y cuándo).