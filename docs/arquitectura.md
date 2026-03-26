# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms, composición de la aplicación, pantalla principal y acceso a Service.
- **GestionDeFardos.Core**: configuración, modelos, contratos y reglas puras.
- **GestionDeFardos.Infrastructure**: configuración, logging, monitoreo serial y persistencia local.

## Persistencia local

- La Etapa 2 incorpora una base SQLite local.
- La ruta sale de `Database.FilePath` en `config.json`.
- El archivo se resuelve relativo a `AppContext.BaseDirectory` cuando no es una ruta absoluta.
- El bootstrap de base se ejecuta al iniciar la aplicación, antes de la operación funcional.

## Modelo de pesada local

La persistencia base de `WeighingRecord` contempla:

- `Id`: autonumérico creciente
- `Timestamp`: fecha y hora de captura
- `WeightKg`: valor de negocio exportable
- `RawGrams`: lectura cruda en gramos, si existe
- `RawFrame`: trama o dato crudo asociado, si existe
- `IsEditedToZero`: marca mínima de auditoría para edición
- `EditedAt`: fecha y hora de edición a cero

## Repositorio de pesadas

`IWeighingRepository` queda preparado para cubrir:

- inicialización de la base
- inserción
- consulta por id
- obtención de última pesada
- listado por rango de fechas
- edición a cero
- borrado histórico por fecha

La implementación actual es `SqliteWeighingRepository`.

## Monitoreo serial en Service

- `ServiceForm` sigue usando `IServicePortMonitor`.
- La implementación actual es `SerialServicePortMonitor`.
- El monitor administra dos `SerialPort` independientes:
  - uno para balanza
  - uno para pulsador

## Balanza

- La configuración física del puerto sale de `Scale.PortName`, `BaudRate`, `DataBits`, `Parity`, `StopBits` y `Handshake`.
- `Scale.Protocol` solo define cómo interpretar los bytes recibidos.
- Protocolos soportados:
  - `w180-t`
  - `simple-ascii`
- Si el protocolo configurado no es soportado, la app sigue mostrando recepción cruda para diagnóstico.

## Pulsador

- El pulsador usa otro puerto serie y su propia configuración física.
- La app escucha la secuencia `$P1!`.
- Cuando la recibe, responde `$B1!`.
- El flujo del pulsador no comparte parser con la balanza.

## Logging

- Los eventos se persisten en `./logs/gestion-de-fardos-YYYYMMDD.log`.
- Se registran apertura y cierre de puertos, recepción cruda, interpretación de tramas y eventos de persistencia local.
