# Manual operativo

## Acceso a Modo Service

1. Con la aplicación abierta, presione `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
2. Se abrirá un cuadro modal con el texto **Ingrese contraseña de Service**.
3. Ingrese la contraseña y presione **Aceptar** (o `Enter`).
4. Para cancelar, use **Cancelar** (o `Esc`), sin cambios en pantalla.
5. Si la contraseña es correcta, se abrirá la pantalla **Modo Service**.
6. Si la contraseña es incorrecta, se mostrará un mensaje de acceso denegado.
7. Si **Modo Service** ya está abierto, la hotkey lo trae al frente.

## Nota de configuración

- La contraseña se toma de `config.json` en la carpeta del ejecutable (`AppContext.BaseDirectory`).
- Debe existir `Passwords.Service` con el valor esperado.

## Ubicación y contenido de config.json

- El archivo `config.json` debe ubicarse en `AppContext.BaseDirectory` (misma carpeta del ejecutable).
- Puede copiarse desde `samples/config.example.json` y luego ajustar valores.
- Para habilitar acceso a Service, `Passwords.Service` debe existir y contener un valor no vacío.

## Configuración de balanza serial

En la sección `Scale` de `config.json` se deben completar:

- `PortName`: puerto serial de la balanza (ejemplo `COM3`).
- `BaudRate`: velocidad serial (ejemplo `9600`).
- `Parity`: paridad (`None`, `Odd`, `Even`, etc.).
- `DataBits`: cantidad de bits de datos (ejemplo `8`).
- `StopBits`: bits de parada (`One`, `Two`, etc.).
- `NewLine`: separador de fin de trama (`"\\n"` o `"\\r\\n"`).
