# Arquitectura

## Capas

- `GestionDeFardos.App`
  - WinForms
  - pantalla principal
  - Modo Service
  - popups de edicion y exportacion
- `GestionDeFardos.Core`
  - configuracion
  - contratos
  - modelos
  - utilidades puras
- `GestionDeFardos.Infrastructure`
  - configuracion
  - logging a archivo
  - monitoreo serial
  - persistencia SQLite
  - exportacion Excel

## Runtime compartido

- La app crea un unico `IWeighingRuntime` al iniciar.
- `MainForm` y `ServiceForm` consumen el mismo runtime.
- Service no abre puertos por su cuenta.
- La implementacion actual es `SerialServicePortMonitor`.

## Canales seriales

### Balanza

- Puerto propio.
- Interpretacion por protocolo configurable.
- Protocolos soportados:
  - `w180-t`
  - `simple-ascii`

### Pulsador

- Puerto propio.
- La app escucha `$P1!`.
- La app responde `$B1!`.

## Persistencia local

- Base SQLite local.
- Ruta definida por `Database.FilePath`.
- Bootstrap automatico al iniciar la app.
- Implementacion actual: `SqliteWeighingRepository`.

## Modelo de pesada

`WeighingRecord` persiste:

- `Id`
- `Timestamp`
- `WeightKg`
- `RawGrams`
- `RawFrame`
- `IsEditedToZero`
- `EditedAt`

## Flujo de captura

1. La balanza actualiza el ultimo peso valido.
2. El pulsador envia `$P1!`.
3. La app responde `$B1!`.
4. El runtime valida thresholds.
5. Si el peso es valido, lo guarda en SQLite.
6. Si no es valido, informa rechazo sin insertar.

## Edicion

- La pantalla principal permite seleccionar una pesada por numero.
- Se valida `Passwords.Edit`.
- El peso se actualiza a `0`.
- El registro se conserva para auditoria minima.

## Exportacion

- La pantalla principal pide un rango de fechas.
- Consulta registros en SQLite.
- Genera `.xlsx` con `ClosedXML`.
- La implementacion actual es `ClosedXmlReportExporter`.

## Modo Service

Service expone:

- snapshot tecnico de balanza
- snapshot tecnico de pulsador
- chunk raw
- trama interpretada
- diagnosticos
- borrado historico por fecha

El borrado historico:

- usa doble confirmacion
- elimina registros hasta una fecha inclusive
- refresca el ultimo registro del runtime

## Logging

- Implementacion actual: `FileAppLogger`
- Destino: `./logs`
- Archivo: `gestion-de-fardos-YYYYMMDD.log`

Se registran:

- inicio y cierre
- apertura y cierre de puertos
- recepcion raw
- interpretacion de balanza
- eventos del pulsador
- guardados
- ediciones
- exportaciones
- borrados
