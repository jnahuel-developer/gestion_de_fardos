# Manual operativo

## Acceso a Modo Service

1. Con la aplicacion abierta, presione `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
2. Se abrira un cuadro modal para ingresar la contrasena de Service.
3. Ingrese la contrasena y presione **Aceptar**.
4. Si la contrasena es correcta, se abrira la pantalla **Modo Service**.
5. Si la contrasena es incorrecta, se mostrara un mensaje de acceso denegado.

## Configuracion de `config.json`

- El archivo debe ubicarse en `AppContext.BaseDirectory`.
- Puede copiarse desde `samples/config.example.json`.
- `Passwords.Service` debe existir y contener un valor no vacio.

## Configuracion de balanza serial

En la seccion `Scale` se deben completar:

- `PortName`
- `BaudRate`
- `Parity`
- `DataBits`
- `StopBits`
- `NewLine`

## Configuracion del pulsador

En la seccion `Button` se debe completar:

- `InputLine`: `Cts` o `Dsr`

Mapeo recomendado para pruebas con simulador:

- Simulador con `--button-line rts`: usar `Button.InputLine = Cts`
- Simulador con `--button-line dtr`: usar `Button.InputLine = Dsr`

## Que muestra la pantalla Service

- Peso convertido a kg.
- Ultima trama ASCII recibida.
- Estado de conexion del puerto.
- Error de puerto o de parseo, si existe.
- Linea configurada para el pulsador.
- Estado crudo de `CTS`.
- Estado crudo de `DSR`.
- Estado logico del pulsador: `Presionado`, `No presionado` o `Sin lectura`.
- Ultima opresion detectada o diagnostico del pulsador.

## Logging basico

- Los logs se guardan en `./logs` junto al ejecutable.
- El archivo diario se llama `gestion-de-fardos-YYYYMMDD.log`.
- Se registran eventos de balanza, pulsador, errores de puerto y eventos principales de la aplicacion.

## Actualizacion de version

- Para instalar una nueva version en la misma PC, cerrar la aplicacion y ejecutar el nuevo `GestionDeFardos-Setup-...exe`.
- Instalar siempre sobre la misma carpeta, por ejemplo `C:\GestionDeFardos`.
- La reinstalacion preserva `config.json`.
- No hace falta desinstalar antes de instalar la siguiente version.

## Desinstalacion

- Con el instalador actual de esta etapa, la aplicacion no aparece en la desinstalacion estandar de Windows.
- Si es necesario quitarla, cerrar la aplicacion y eliminar manualmente la carpeta de instalacion.
- Si existen accesos directos, eliminarlos manualmente.
- Respaldar `config.json` y `logs` antes de quitarla si se desea conservar informacion.
