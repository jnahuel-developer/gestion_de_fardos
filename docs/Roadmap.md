# Roadmap de Desarrollo

## Estado actual

- La Etapa 1 se considera finalizada.
- Ya están resueltos el esqueleto WinForms, el acceso a Service, la lectura de balanza y la lectura del pulsador.
- La versión 1.0.0 fue instalada y validada en el cliente.

## Objetivo de Etapa 2

Completar la operación funcional de la app con:

- base de datos local
- registro de pesadas
- pantalla principal operativa
- edición de registros
- exportación a Excel
- borrado histórico desde Service

## Estrategia de ramas

- Cada desarrollo parte desde `develop`.
- Cada entrega funcional usa una rama `modxxxx`.
- Cada mod debe cerrar con:
  - compilación correcta
  - validación manual mínima
  - push a remoto
- El merge a `develop` se realiza manualmente luego de la validación.

## Plan incremental

### `mod0005` - Base local y modelo de datos

- Incorporar SQLite con `Microsoft.Data.Sqlite`.
- Agregar `Database.FilePath` a la configuración.
- Crear bootstrap automático de base.
- Implementar `SqliteWeighingRepository`.
- Ampliar `WeighingRecord` y `IWeighingRepository`.
- Validación:
  - la base se crea
  - se puede insertar
  - se puede leer la última pesada

### `mod0006` - Motor de captura y coordinación

- Introducir un runtime compartido para pantalla principal y Service.
- Consumir balanza y pulsador desde un flujo técnico único.
- Guardar la pesada cuando se detecte `$P1!`.
- Bloquear guardado fuera de thresholds.
- Validación:
  - una pulsación guarda o rechaza con motivo
  - Service sigue funcionando sin conflicto

### `mod0007` - Pantalla principal operativa

- Reemplazar el placeholder por la UI real.
- Mostrar peso actual, estado de conexión, última opresión y último registro guardado.
- Agregar botones `Editar registro` y `Exportar a Excel`.
- Quitar datos de configuración y guía de acceso a Service de la pantalla principal.
- Validación:
  - la pantalla refleja peso vivo y última captura

### `mod0008` - Edición de registros

- Implementar edición por número de pesada.
- Pedir `Passwords.Edit`.
- Dejar el peso en cero y conservar el registro.
- Validación:
  - edición correcta
  - rechazo por contraseña incorrecta
  - rechazo por id inexistente

### `mod0009` - Exportación a Excel

- Implementar exportación `.xlsx` con `ClosedXML`.
- Pedir rango `desde / hasta`.
- Exportar columnas:
  - `Nro de Fardo`
  - `Dia`
  - `Hora`
  - `Kg`
- Validación:
  - archivo Excel válido
  - contenido correcto
  - manejo de rango vacío

### `mod0010` - Borrado histórico desde Service

- Activar el borrado por fecha en Service.
- Pedir fecha inicial.
- Borrar todo hacia atrás desde esa fecha.
- Exigir doble confirmación.
- Validación:
  - borrado efectivo
  - confirmación doble
  - resultado visible

### `mod0011` - Cierre y entrega final

- Ajustes de UX, mensajes y logs.
- Actualización completa de documentación operativa.
- Checklist end-to-end con simulador.
- Preparación de instalador final.

## Criterio de cierre del proyecto

El proyecto se considera completo cuando:

- la pantalla principal funciona en operación real
- las pesadas se guardan en base local
- se puede editar una pesada
- se puede exportar a Excel
- Service puede borrar histórico
- la documentación y el instalador quedan actualizados
