# Checklist de pruebas

## Basico de compilacion

- Restauracion de dependencias de la solucion.
- Compilacion de Core, Infrastructure y App.
- Publicacion self-contained `win-x64`.

## Pruebas manuales - Acceso a Modo Service

- Verificar que `Ctrl+Shift+S` abre el prompt de contrasena.
- Verificar que `Ctrl+Alt+Shift+S` abre el prompt de contrasena.
- Verificar que al cancelar no se abre `ServiceForm`.
- Verificar que contrasena incorrecta muestra error y no abre `ServiceForm`.
- Verificar que contrasena correcta abre `ServiceForm`.
- Verificar que si `ServiceForm` ya esta abierta, la hotkey la trae al frente.

## Pruebas manuales - Service con balanza y pulsador

- Preparar `config.json` con `Scale` valida, `Button.InputLine` configurado y `Passwords.Service` completo.
- Verificar que Service muestra peso convertido, ultima trama ASCII, conexion y error.
- Verificar que Service muestra la linea configurada y los estados crudos de `CTS` y `DSR`.
- Verificar que Service muestra el estado logico del pulsador y la ultima opresion.
- Verificar que con puerto inexistente el formulario no se cierra, muestra estado desconectado y deja las lineas en `Sin lectura`.
- Verificar que una trama sin entero genera error controlado sin crash.
- Verificar con simulador `--ui --button-line rts` y `Button.InputLine = Cts` que cada pulso genere una sola opresion.
- Verificar con simulador `--ui --button-line dtr` y `Button.InputLine = Dsr` el mismo comportamiento.
- Verificar que un valor invalido en `Button.InputLine` no rompa la lectura de balanza y deje diagnostico claro para el pulsador.

## Pruebas manuales - Logging

- Verificar creacion de la carpeta `logs`.
- Verificar creacion del archivo diario `gestion-de-fardos-YYYYMMDD.log`.
- Verificar que se registran apertura/cierre de puerto, tramas de balanza, cambios de lineas y errores.

## Pruebas manuales - Instalador

- Verificar instalacion en Windows x64 limpio.
- Verificar que el instalador deja la app en la ruta writable definida.
- Verificar que `config.json` existente no se sobreescribe en una reinstalacion.
- Verificar que el paquete final no contiene codigo fuente.
