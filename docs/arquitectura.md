# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms, pantalla principal y acceso a Service.
- **GestionDeFardos.Core**: configuracion, modelos, contratos y reglas puras.
- **GestionDeFardos.Infrastructure**: configuracion, logging, monitoreo serial y persistencia local.

## Persistencia local

- La Etapa 2 incorpora una base SQLite local.
- La ruta sale de `Database.FilePath` en `config.json`.
- El archivo se resuelve relativo a `AppContext.BaseDirectory` cuando no es una ruta absoluta.
- El bootstrap de base se ejecuta al iniciar la aplicacion.

## Modelo de pesada local

La persistencia base de `WeighingRecord` contempla:

- `Id`: autonumerico creciente
- `Timestamp`: fecha y hora de captura
- `WeightKg`: valor de negocio exportable
- `RawGrams`: lectura cruda en gramos, si existe
- `RawFrame`: trama o dato crudo asociado, si existe
- `IsEditedToZero`: marca minima de auditoria para edicion
- `EditedAt`: fecha y hora de edicion a cero

## Repositorio de pesadas

`IWeighingRepository` queda preparado para cubrir:

- inicializacion de la base
- insercion
- consulta por id
- obtencion de ultima pesada
- listado por rango de fechas
- edicion a cero
- borrado historico por fecha

La implementacion actual es `SqliteWeighingRepository`.

## Runtime compartido

- La app arranca un unico `IWeighingRuntime` desde `MainForm`.
- `ServiceForm` consume el snapshot tecnico del runtime ya iniciado y no abre puertos por su cuenta.
- La implementacion actual es `SerialServicePortMonitor`.
- El runtime administra dos `SerialPort` independientes:
  - uno para balanza
  - uno para pulsador
- El runtime expone dos vistas de estado:
  - snapshot tecnico para Service
  - snapshot funcional para la pantalla principal

## Balanza

- La configuracion fisica del puerto sale de `Scale.PortName`, `BaudRate`, `DataBits`, `Parity`, `StopBits` y `Handshake`.
- `Scale.Protocol` solo define como interpretar los bytes recibidos.
- Protocolos soportados:
  - `w180-t`
  - `simple-ascii`
- Si el protocolo configurado no es soportado, la app sigue mostrando recepcion cruda para diagnostico.

## Pulsador y captura

- El pulsador usa otro puerto serie y su propia configuracion fisica.
- La app escucha la secuencia `$P1!`.
- Cuando la recibe, responde `$B1!`.
- Cada `$P1!` dispara un intento de guardado usando el ultimo peso valido interpretado.
- El guardado se rechaza cuando no hay peso valido o cuando el valor queda fuera de `Thresholds.MinKg` y `Thresholds.MaxKg`.

## Logging

- Los eventos se persisten en `./logs/gestion-de-fardos-YYYYMMDD.log`.
- Se registran apertura y cierre de puertos, recepcion cruda, interpretacion de tramas, eventos del pulsador y persistencia local.
