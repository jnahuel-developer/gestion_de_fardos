# Manual operativo

## Acceso a Modo Service

1. Con la aplicacion abierta, presione `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
2. Se abrira un cuadro modal con el texto **Ingrese contrasena de Service**.
3. Ingrese la contrasena y presione **Aceptar** (o `Enter`).
4. Para cancelar, use **Cancelar** (o `Esc`), sin cambios en pantalla.
5. Si la contrasena es correcta, se abrira la pantalla **Modo Service**.
6. Si la contrasena es incorrecta, se mostrara un mensaje de acceso denegado.
7. Si **Modo Service** ya esta abierto, la hotkey lo trae al frente.

## Nota de configuracion

- La contrasena se toma de `config.json` en la carpeta del ejecutable (`AppContext.BaseDirectory`).
- Debe existir `Passwords.Service` con el valor esperado.

## Ubicacion y contenido de config.json

- El archivo `config.json` debe ubicarse en `AppContext.BaseDirectory` (misma carpeta del ejecutable).
- Puede copiarse desde `samples/config.example.json` y luego ajustar valores.
- Para habilitar acceso a Service, `Passwords.Service` debe existir y contener un valor no vacio.

## Configuracion de balanza serial

En la seccion `Scale` de `config.json` se deben completar:

- `PortName`: puerto serial de la balanza (ejemplo `COM3`).
- `BaudRate`: velocidad serial (ejemplo `9600`).
- `Parity`: paridad (`None`, `Odd`, `Even`, etc.).
- `DataBits`: cantidad de bits de datos (ejemplo `8`).
- `StopBits`: bits de parada (`One`, `Two`, etc.).
- `NewLine`: separador de fin de trama (`"\\n"` o `"\\r\\n"`).

## Configuracion del pulsador en el mismo puerto

En la seccion `Button` de `config.json` se debe completar:

- `InputLine`: linea de entrada que observara la app (`Cts` o `Dsr`).

Mapeo recomendado para pruebas con el simulador:

- Simulador con `--button-line rts`: configurar `Button.InputLine = Cts`.
- Simulador con `--button-line dtr`: configurar `Button.InputLine = Dsr`.

## Que muestra la pantalla Service

- Ultima trama ASCII recibida desde la balanza.
- Peso actual convertido a kg.
- Estado de conexion del puerto.
- Estado textual del pulsador: `Presionado`, `No presionado` o `Sin lectura`.
- Ultima opresion detectada o diagnostico del pulsador si la lectura no esta disponible.
