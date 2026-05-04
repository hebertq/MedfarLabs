# Flujo 10: Onboarding de Clientes (Tenancy)

## 1. Descripción
Proceso de registro de una nueva entidad legal en la plataforma, creación de su espacio de trabajo y configuración inicial.

## 2. Pasos del Flujo y Seed de Catálogos
1. **Registro de Cuenta:** El administrador de la clínica registra los datos legales (RUC, Nombre Comercial, Dirección).
2. **Aislamiento de Datos:** El sistema asigna un `TenantId` único.
3. **Configuración de Identidad:** Creación del primer usuario Administrador del Tenant mediante el módulo `Identity`.
4. **Catálogos por Defecto (Seeding):** Al crear el Tenant, un manejador de eventos en background clona e inyecta los `mst_catalog_detail` base (estados, impuestos por defecto, tipos de sangre, etc.) para que el laboratorio pueda operar inmediatamente sin configuración engorrosa.
5. **Setup de Sucursales:** Definición de las ubicaciones físicas donde opera el Tenant.

## 3. Casos de Uso Relacionados
* UC-20: Registro de nuevo Tenant.
* UC-21: Configuración de marca (Logos y encabezados para reportes).