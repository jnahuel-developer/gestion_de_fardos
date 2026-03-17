# Checklist de pruebas

## Skeleton actual

- Restauracion de dependencias de la solucion.
- Compilacion completa de la solucion.
- Ejecucion basica de la aplicacion WinForms.

## Pruebas manuales - Acceso a Modo Service

- Verificar que `Ctrl+Shift+S` abre el prompt de contrasena.
- Verificar que `Ctrl+Alt+Shift+S` abre el prompt de contrasena.
- Verificar que al cancelar (`Esc` o boton **Cancelar**) no se abre `ServiceForm`.
- Verificar que contrasena incorrecta muestra error y no abre `ServiceForm`.
- Verificar que contrasena correcta abre `ServiceForm`.
- Verificar que si `ServiceForm` ya esta abierta, la hotkey la trae al frente.
- Verificar mensaje de advertencia cuando una hotkey no puede registrarse.
- Verificar mensaje claro cuando no existe `config.json` y que indica como resolverlo.

## Placeholder para etapas futuras

- Validacion de persistencia SQLite.
- Validacion de exportacion Excel.

## Pruebas manuales - Service con balanza y pulsador

- Preparar `config.json` junto al ejecutable con seccion `Scale` valida, `Button.InputLine` configurado y `Passwords.Service` completo.
- Verificar que al abrir Service se observe actualizacion periodica del peso en kg y de la ultima trama ASCII.
- Verificar que el estado de conexion indique **Conectada** cuando el puerto serial esta disponible.
- Verificar que con puerto inexistente (ej. `COM999`) el formulario no se cierra, muestra **Desconectada** y deja el pulsador en `Sin lectura`.
- Verificar que cuando la trama no contiene numero entero se muestre el error correspondiente sin crash.
- Verificar con el simulador solo de balanza que la recepcion de pesos siga funcionando igual que antes.
- Verificar con el simulador `--ui --button-line rts` y `Button.InputLine = Cts` que cada pulso de 500 ms muestre `Presionado`, registre una unica opresion y no interrumpa la balanza.
- Verificar con el simulador `--ui --button-line dtr` y `Button.InputLine = Dsr` el mismo comportamiento.
- Verificar que un valor invalido en `Button.InputLine` no rompa la lectura de balanza y que el formulario muestre `Sin lectura` con diagnostico claro para el pulsador.
