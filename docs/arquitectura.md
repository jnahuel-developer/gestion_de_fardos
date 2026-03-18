# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms, composicion de la aplicacion y acceso al modo Service.
- **GestionDeFardos.Core**: modelos, contratos y reglas puras.
- **GestionDeFardos.Infrastructure**: configuracion, puerto serial, logging a archivo y packaging tecnico.

## Ubicaciones de archivos

- Configuracion: `AppContext.BaseDirectory/config.json`.
- Logs: `AppContext.BaseDirectory/logs`.
- Artefactos de release: `artifacts/`.

## Acceso a Modo Service

- El acceso se inicia desde `MainForm` mediante `WM_HOTKEY`.
- Se registran dos combinaciones:
  - `Ctrl+Shift+S`
  - `Ctrl+Alt+Shift+S`
- Al detectar la hotkey se solicita contrasena y, si es correcta, se abre `ServiceForm`.
- Si `ServiceForm` ya esta abierta, no se crea una nueva instancia; se trae al frente.

## Monitoreo serial compartido en Service

- `ServiceForm` inicia un `IServicePortMonitor` al mostrarse y lo detiene al cerrarse.
- La implementacion actual es `SerialServicePortMonitor`, basada en un unico `SerialPort`.
- La balanza se lee por `DataReceived`, acumulando texto ASCII hasta linea completa y extrayendo el primer entero en gramos.
- El pulsador se detecta por `PinChanged`, evaluando la linea configurada (`CTS` o `DSR`) y detectando solo el flanco ascendente como opresion.
- El snapshot expone peso convertido, ultima trama ASCII, conexion, error, linea configurada, estado crudo de `CTS/DSR`, estado logico del pulsador y ultima opresion.

## Logging basico

- El contrato comun es `IAppLogger`.
- La implementacion actual es `FileAppLogger`.
- Los eventos se persisten en `./logs/gestion-de-fardos-YYYYMMDD.log`.
- Se registran inicio/cierre de app, eventos de Service, apertura/cierre de puerto, tramas de balanza, cambios de lineas de control y errores.

## Configuracion serial relevante

- `Scale.PortName`, `Scale.BaudRate`, `Scale.Parity`, `Scale.DataBits`, `Scale.StopBits`, `Scale.NewLine`.
- `Button.InputLine` define la linea de entrada observada por la app (`Cts` o `Dsr`).
- Para pruebas con el simulador:
  - `--button-line rts` implica `Button.InputLine = Cts`.
  - `--button-line dtr` implica `Button.InputLine = Dsr`.

## Release e instalador

- La publicacion de entrega se genera self-contained `win-x64`.
- El instalador se arma con Inno Setup a partir de la carpeta publicada.
- La ruta por defecto es `C:\GestionDeFardos`, fuera de `Program Files`, para permitir `config.json` y `logs` junto al ejecutable.
