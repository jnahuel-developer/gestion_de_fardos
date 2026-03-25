# Checklist de pruebas

## Basico de compilacion

- Restauracion de dependencias de la solucion.
- Compilacion de Core, Infrastructure y App.
- Publicacion self-contained `win-x64`.

## Pruebas manuales - Service

- Verificar que `Ctrl+Shift+S` y `Ctrl+Alt+Shift+S` siguen abriendo el prompt de contrasena.
- Verificar que Service abre con `Scale.Protocol = "w180-t"` y configuracion serial no estandar.
- Verificar que Service abre con `Scale.Protocol = "simple-ascii"`.
- Verificar que Service abre con `Scale.Protocol` invalido y muestra solo recepcion cruda.
- Verificar que una `Parity`, `StopBits` o `Handshake` invalida deja diagnostico claro dentro de Service sin bloquear el acceso.
- Verificar que el ultimo chunk crudo de balanza aparece aun cuando la trama no pueda interpretarse.
- Verificar que la ultima trama interpretada y el peso/tara se actualizan cuando la trama es valida.
- Verificar que el pulsador abre con su configuracion serial propia y responde `$B1!` al recibir `$P1!`.
- Verificar que con `Button.PortName` vacio el pulsador queda deshabilitado y la balanza sigue funcionando.
- Verificar que si el puerto del pulsador falla, la balanza sigue operativa y el error queda visible.

## Pruebas manuales - Logging

- Verificar creacion de la carpeta `logs`.
- Verificar creacion del archivo diario `gestion-de-fardos-YYYYMMDD.log`.
- Verificar que se registran chunks crudos de balanza y pulsador.
- Verificar que se registran tramas interpretadas correctamente.
- Verificar que se registran `BUTTON RX $P1!`, `BUTTON TX $B1!` y errores de apertura o IO.
