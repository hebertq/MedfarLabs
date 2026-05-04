# 🛠️ Plan de Trabajo: Cobertura de Fakers, Acciones y Tests de Integración

Este documento contiene las instrucciones precisas para expandir la infraestructura de pruebas y seguridad de **MedFarLab.Core**. El objetivo es cerrar las brechas de cobertura detectadas en los módulos de Facturación, Laboratorio, Farmacia e Identidad.

---

## 1. 🧪 Generación de Fakers (Capa de Infraestructura de Pruebas)

**Ubicación:** `tests/SharedFakers/Fakers/`

Crea los siguientes Fakers heredando de `AutoFaker<T>` para permitir la generación de datos aleatorios consistentes en los tests:

* **Módulo Billing:**
    * `ConsultationLedgerFaker`: Generar registros contables vinculados a citas.
    * `SubscriptionInvoiceFaker` y `SubscriptionPaymentFaker`: Para pruebas de flujo de caja SaaS.
* **Módulo Laboratory:**
    * `LabSampleFaker`: Configurar muestras con estados (Pendiente, Procesada, Rechazada).
    * `LabExamTemplateItemFaker`: Generar ítems de plantillas con rangos de referencia.
    * `LabResultItemFaker`: Resultados vinculados a ítems de examen.
* **Módulo Pharmacy:**
    * `MedicationFaker`: Catálogo de medicinas con nombre comercial, genérico y presentación.
* **Módulo Identity:**
    * `BranchFaker`: Sedes de la organización.

---

## 2. 🔐 Definición de Acciones (AppActions - Capa Domain)

**Ubicación:** `src/Domain/Const/`

Completa las constantes de permisos para asegurar que el sistema de auditoría y multitenancy pueda validar las operaciones:

* **Archivo `AppAction.Pharmacy.cs`:**
    * `SearchMedications`: Permitir consulta del catálogo.
    * `ManageInventory`: Permisos para actualizar existencias.
* **Archivo `AppAction.Laboratory.cs`:**
    * `ConfigureLabTemplates`: Acceso para modificar `LabExamTemplateItem`.
    * `ValidateLabResults`: Permiso para la firma técnica de resultados.
* **Archivo `AppAction.Care.cs`:**
    * `PrintDailyConsultationReport`: Acción específica para el reporte MINSA.

---

## 3. ⚙️ Tests de Integración de Acciones (Capa de Tests)

**Ubicación:** `tests/MedFarLab.Infrastructure.Tests/Integration/`

Crea una suite de pruebas para validar que las acciones ejecutan correctamente la lógica de negocio y respetan el aislamiento de datos:

### Escenario A: Seguridad y Multitenancy
* **Test:** `Ensure_Organization_Data_Isolation`.
* **Lógica:** Usar `MasterSeeder` para crear dos organizaciones. Intentar ejecutar una acción de lectura de paciente desde el contexto de una organización diferente y validar que el resultado sea nulo o lance `UnauthorizedAccessException`.

### Escenario B: Flujo de Laboratorio
* **Test:** `Complete_Laboratory_Workflow_With_Actions`.
* **Lógica:** 1. Crear una orden de laboratorio.
    2. Usar `LabSampleFaker` para registrar la toma de muestra.
    3. Registrar resultados y validar que la acción de auditoría `ActionLog` registre quién realizó la validación técnica.

### Escenario C: Auditoría de Acceso a Pacientes
* **Test:** `Audit_Log_Created_On_Patient_Access`.
* **Lógica:** Ejecutar una consulta de expediente médico y verificar que en la tabla `PatientAccessLog` se haya insertado un registro con el `UserId` y `PatientId` correctos a través de la `PatientAccessAuditOutputAction`.

---

## 📌 Instrucciones Finales para Antigravity
> "Implementa estos cambios siguiendo el patrón de Arquitectura Limpia. Asegúrate de que todos los nuevos Fakers se registren en la clase base de pruebas para estar disponibles mediante inyección. Los tests de integración deben usar una base de datos In-Memory o TestContainers según la configuración actual del proyecto."
