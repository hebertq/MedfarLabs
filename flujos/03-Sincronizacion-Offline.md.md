# Flujo 03: Sincronización de Datos (PWA)

## 1. Descripción
Mecanismo para permitir la operación en puntos de toma de muestra remotos y asegurar la integridad de los datos al recuperar conexión.

## 2. Actores
* Sistema (PWA Offline Handler)
* Usuario de campo

## 3. Pasos del Flujo (Happy Path)
1. **Modo Desconectado:** La PWA detecta falta de red. Las acciones se guardan en `IOfflineStorage` (Local Storage/IndexedDB)[cite: 3].
2. **Cola de Comandos:** Cada acción (ej. Tomar muestra) se encola como un "Comando Pendiente"[cite: 3].
3. **Reconexión:** El `OfflineCommandHandler` detecta señal y comienza el envío secuencial al API[cite: 3].
4. **Conciliación:** El servidor procesa los comandos y devuelve confirmación. La PWA limpia la cola local.

## 4. Casos de Uso Relacionados
* UC-07: Almacenamiento local de transacciones.
* UC-08: Sincronización de fondo (Background Sync).