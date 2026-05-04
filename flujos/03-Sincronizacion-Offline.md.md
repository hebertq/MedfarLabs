# Flujo 03: Sincronización de Datos (PWA)

## 1. Descripción
Mecanismo para permitir la operación en puntos de toma de muestra remotos y asegurar la integridad de los datos al recuperar conexión.

## 2. Actores
* Sistema (PWA Offline Handler)
* Usuario de campo

## 3. Pasos del Flujo y Resolución de Conflictos
1. **Modo Desconectado:** La PWA detecta falta de red. Las acciones se guardan en `IOfflineStorage` (IndexedDB).
2. **Cola de Comandos:** Cada acción (ej. Tomar muestra) se encola como un "Comando Pendiente" con un `RequestId` único (Idempotencia).
3. **Reconexión:** El `OfflineCommandHandler` detecta señal y comienza el envío secuencial al API.
4. **Resolución de Conflictos:** Si el API detecta que el registro ya fue modificado por otro usuario (ej. muestra ya tomada por otro flebotomista), aplica una regla de *Last-Write-Wins* o devuelve una alerta `ConflictResult` a la PWA para resolución manual.
5. **Conciliación:** El servidor procesa los comandos exitosos y devuelve confirmación. La PWA limpia la cola local.

## 4. Casos de Uso Relacionados
* UC-07: Almacenamiento local de transacciones.
* UC-08: Sincronización de fondo (Background Sync).