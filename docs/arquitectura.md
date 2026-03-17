# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms y composicion de la aplicacion.
- **GestionDeFardos.Core**: modelos, contratos y reglas de negocio puras.
- **GestionDeFardos.Infrastructure**: implementaciones tecnicas y acceso a puertos/configuracion.

## Ubicaciones de archivos

- Configuracion: `AppContext.BaseDirectory/config.json`.
- Base de datos SQLite: `AppContext.BaseDirectory`.
- Logs: `./logs` relativo al ejecutable.

## Acceso a Modo Service

- El acceso se inicia desde `MainForm` mediante `WM_HOTKEY` (WinAPI).
- Se registran dos combinaciones:
  - `Ctrl+Shift+S`
  - `Ctrl+Alt+Shift+S`
- Si la hotkey no puede registrarse (por conflicto con otra app), se muestra advertencia y la app continua.
- Al detectar hotkey se solicita contrasena en un dialogo modal.
- La validacion se realiza contra `Passwords.Service` leido desde `config.json`.
- Si `ServiceForm` ya esta abierta, no se crea una nueva instancia; se trae al frente.

## Monitoreo serial compartido en Service

- `ServiceForm` inicia un `IServicePortMonitor` al mostrarse y lo detiene al cerrarse.
- La implementacion actual es `SerialServicePortMonitor` (Infrastructure), basada en `System.IO.Ports.SerialPort`.
- Se abre un unico `SerialPort` para balanza y pulsador.
- La balanza se procesa por `DataReceived`: se acumula texto ASCII hasta linea completa y se procesa el primer entero encontrado como gramos.
- El pulsador se procesa por `PinChanged`, leyendo una linea de control del mismo puerto y detectando solo el flanco ascendente como nueva opresion.
- Conversion de unidades: gramos a kilogramos mediante `WeightConversionHelper.GramsToKg`.
- Estado expuesto por snapshot thread-safe (`ServicePortSnapshot`): ultima trama ASCII, gramos crudos, kg, fecha de actualizacion, estado de conexion, ultimo error de puerto/balanza, estado del pulsador, ultima opresion y ultimo error del pulsador.

## Configuracion serial relevante

- `Scale.PortName`, `Scale.BaudRate`, `Scale.Parity`, `Scale.DataBits`, `Scale.StopBits`, `Scale.NewLine`.
- `Button.InputLine` define que linea de entrada observa la app (`Cts` o `Dsr`).
- Para las pruebas con el simulador:
  - `--button-line rts` en el simulador implica `Button.InputLine = Cts` en la app.
  - `--button-line dtr` en el simulador implica `Button.InputLine = Dsr` en la app.

## Portabilidad y permisos

Se debe instalar la aplicacion en una carpeta con permisos de escritura para permitir persistencia local de configuracion, base de datos y logs.
