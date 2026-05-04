# Flujo 10: Onboarding de Clientes (Tenancy)

## 1. Descripción
Proceso de registro de una nueva entidad legal en la plataforma, creación de su espacio de trabajo y configuración inicial.

## 2. Pasos del Flujo
1. **Registro de Cuenta:** El administrador de la clínica registra los datos legales (RUC, Nombre Comercial, Dirección).
2. **Aislamiento de Datos:** El sistema asigna un `TenantId` único. Según la arquitectura, esto puede disparar la creación de un esquema de base de datos o simplemente marcar todos los registros futuros.
3. **Configuración de Identidad:** Creación del primer usuario Administrador del Tenant mediante el módulo `Identity`[cite: 3].
4. **Setup de Sucursales:** Definición de las ubicaciones físicas donde opera el Tenant.

## 3. Casos de Uso
* UC-20: Registro de nuevo Tenant.
* UC-21: Configuración de marca (Logos y encabezados para reportes).