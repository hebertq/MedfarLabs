# 🤝 Contribuyendo a MedfarLabs.Core

Gracias por interesarte en contribuir a la base técnica de MedfarLabs. Este documento detalla los estándares de arquitectura, calidad de código y flujo de trabajo necesarios para mantener un núcleo (Core) escalable y resiliente.

---

## 🏗️ Estándares de Arquitectura (DDD)

Este proyecto sigue estrictamente los principios de **Domain-Driven Design (DDD)**. Cada contribución debe respetar las responsabilidades de cada capa:

1.  **Domain**: Contiene lógica pura de negocio, interfaces de repositorio (`IBaseRepository`), Enums y Entidades que heredan de `BaseEntity`.
2.  **Application**: Orquesta el flujo mediante `BaseDomain` y `ActionDispatcher`. Aquí residen los servicios y las reglas de validación.
3.  **Infrastructure**: Implementa detalles técnicos como persistencia con Dapper, mensajería con AWS SQS y seguridad AES.

---

## 🚀 Flujo de Desarrollo para una Nueva Funcionalidad

Para añadir una nueva acción (ej: `AnularFactura`), debe seguir este orden exacto para garantizar la trazabilidad:

### 1. Contrato y Mapeo
Cree el DTO en la capa de aplicación y vincúlelo mediante el atributo `ActionMapping`. Asegúrese de implementar `IHasOrganization` para aislamiento multi-tenant.

### 2. Reglas de Validación
Hereda de `BaseValidationRuleSet<T>`. No use validadores externos; toda la lógica de integridad debe ser inyectable y comprobable en los tests de estrategia.

### 3. Implementación del Servicio
Añada el método a la interfaz del servicio correspondiente y su implementación heredando de `BaseService` para aprovechar la gestión de transacciones atómicas.

### 4. Registro en el Dominio
Vincule la `AppAction` con el método del servicio en la clase `Domain` específica:
```csharp
RegisterActionHandler<IBillingService>(AppAction.AnularFactura, nameof(IBillingService.AnularAsync));
