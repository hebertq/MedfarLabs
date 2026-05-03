# Estándares de Programación - Perfil Backend (APIs)

Este documento define el estándar de desarrollo para la capa de APIs expuestas (`MedFarLab.Api` y `MedFarLab.Reporting.Api`), las cuales actúan como la puerta de entrada a los servicios del dominio.

## 1. Arquitectura de Controladores Delgados
- **Delegación Absoluta:** Los controladores (`Controllers` o `Minimal APIs`) NUNCA deben contener lógica de negocio.
- **Uso de Dispatcher:** Toda operación debe ser empaquetada en un DTO que herede de un contrato (`AppAction`) y enviada inmediatamente al `IApplicationDispatcher`.
- **Mapeo de Respuestas Estándar:** El retorno de la API debe ser incondicionalmente el objeto `BaseResponse<T>` generado por el motor central. Los códigos HTTP 200/400/500 se manejan globalmente por Middleware o se infieren de `BaseResponse.IsSuccess`.

```csharp
[HttpPost("crear")]
public async Task<IActionResult> Crear([FromBody] CrearPacienteRequestDTO request)
{
    var response = await _dispatcher.DispatchAsync<long>(request);
    return response.IsSuccess ? Ok(response) : BadRequest(response);
}
```

## 2. Autenticación y Autorización (JWT & Multitenancy)
- El JWT validado DEBE extraer los Claims `TenantId`, `BranchId`, y `UserId` obligatoriamente e inyectarlos en el `IUserContext` mediante Middleware (`SessionAuthMiddleware`).
- Bajo NINGUNA circunstancia se debe requerir que el cliente Frontend pase el `OrganizationId` explícitamente en el body del request para las operaciones de negocio si esto puede derivarse del JWT (evitar vulnerabilidades IDOR - Insecure Direct Object Reference).

## 3. Manejo de Contextos Asíncronos
- Evitar bloqueos. Nunca utilizar `.Result` o `.Wait()` en código asíncrono.
- La ejecución de tareas secundarias (emails, logs, cálculos diferidos) debe derivarse a `IOutputAction` y lanzarse al SQS (`QueueOutputAction`), nunca bloquear la respuesta de la API esperando que el email se envíe.

## 4. Inyección de Dependencias
- Utilizar exclusivamente Atributos `[RegisterScoped]`, `[RegisterSingleton]` (mediante la librería `Injectio`) para el registro estático compatible con Native AOT. No usar el escáner reflexivo en `Program.cs`.
