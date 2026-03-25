# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms, composicion de la aplicacion y acceso al modo Service.
- **GestionDeFardos.Core**: modelos, contratos y reglas puras.
- **GestionDeFardos.Infrastructure**: configuracion, puerto serial, logging a archivo y adaptadores tecnicos.

## Monitoreo serial en Service

- `ServiceForm` inicia un `IServicePortMonitor` al mostrarse y lo detiene al cerrarse.
- La implementacion actual es `SerialServicePortMonitor`.
- El monitor administra dos `SerialPort` independientes:
  - uno para balanza
  - uno para pulsador

## Balanza

- La configuracion fisica del puerto sale de `Scale.PortName`, `BaudRate`, `DataBits`, `Parity`, `StopBits` y `Handshake`.
- `Scale.Protocol` solo define como interpretar los bytes recibidos.
- Protocolos soportados:
  - `w180-t`
  - `simple-ascii`
- Si el protocolo configurado no es soportado, la app sigue mostrando recepcion cruda para diagnostico.

## Pulsador

- El pulsador usa otro puerto serie y su propia configuracion fisica.
- La app escucha la secuencia `$P1!`.
- Cuando la recibe, responde `$B1!`.
- El flujo del pulsador no comparte parser con la balanza.

## Snapshot de Service

El snapshot expone:

- protocolo de balanza configurado
- perfil serie de balanza y pulsador
- estado de conexion de ambos puertos
- ultimo chunk crudo de balanza
- ultima trama interpretada correctamente
- peso y tara interpretados
- ultimo chunk crudo del pulsador
- ultima trama `$P1!`, ultima respuesta `$B1!` y ultimo error del pulsador

## Logging

- Los eventos se persisten en `./logs/gestion-de-fardos-YYYYMMDD.log`.
- Se registran apertura y cierre de puertos, recepcion cruda, interpretacion de tramas y errores.
