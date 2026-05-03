# 🗃️ Módulo de Catálogos (Common)

Recupera y provee diccionarios y listas maestras usadas globalmente por la aplicación (Géneros, Nacionalidades, Estados, Tipos de Sangre, etc).

## Endpoints Principales

### `GET /api/common/catalogs`
Extrae todos los catálogos vinculados a la Organización. Utiliza caché en memoria.

**Payload DTO: `None`**
El endpoint es un GET. Se autoinyecta `X-Branch-Id` si es que requiere filtrado por organización.

