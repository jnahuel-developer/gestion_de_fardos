# Manual operativo

## Acceso a Modo Service

1. Con la aplicacion abierta, presione `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
2. Se abrira un cuadro modal para ingresar la contrasena de Service.
3. Ingrese la contrasena y presione **Aceptar**.
4. Si la contrasena es correcta, se abrira la pantalla **Modo Service**.

## Configuracion de `config.json`

- El archivo debe ubicarse en `AppContext.BaseDirectory`.
- Puede copiarse desde `samples/config.example.json`.
- `Passwords.Service` debe existir y contener un valor no vacio.

## Configuracion de balanza serial

En la seccion `Scale` se deben completar:

- `Protocol`
- `PortName`
- `BaudRate`
- `DataBits`
- `Parity`
- `StopBits`
- `Handshake`
- `NewLine`

Notas:

- `Protocol` define como interpretar los datos, no como abrir el puerto.
- Protocolos soportados:
  - `w180-t`
  - `simple-ascii`
- Si el protocolo configurado no es soportado, Service abre igual y muestra solo recepcion cruda.
- `NewLine` solo aplica a `simple-ascii`.

## Configuracion del pulsador

En la seccion `Button` se deben completar:

- `PortName`
- `BaudRate`
- `DataBits`
- `Parity`
- `StopBits`
- `Handshake`

Notas:

- El pulsador usa un puerto distinto al de la balanza.
- La app escucha `$P1!` y responde `$B1!`.
- Si `Button.PortName` queda vacio, Service abre con el pulsador deshabilitado.

## Que muestra la pantalla Service

- Protocolo configurado para la balanza.
- Puerto y perfil serie efectivo de balanza y pulsador.
- Estado de conexion de ambos canales.
- Ultimo chunk crudo recibido de la balanza.
- Ultima trama interpretada correctamente.
- Peso y tara interpretados, si existen.
- Ultimo chunk crudo del pulsador.
- Ultima trama `$P1!` recibida y ultima respuesta `$B1!` enviada.
- Error o diagnostico del ultimo intento de lectura o interpretacion.

## Logging basico

- Los logs se guardan en `./logs` junto al ejecutable.
- El archivo diario se llama `gestion-de-fardos-YYYYMMDD.log`.
- Se registran apertura y cierre de ambos puertos, chunks crudos recibidos, interpretacion de tramas, `BUTTON RX $P1!`, `BUTTON TX $B1!` y errores.
