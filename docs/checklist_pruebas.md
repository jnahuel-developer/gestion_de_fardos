# Checklist de pruebas

## Build y publicacion

- `dotnet restore GestionDeFardos.sln`
- `dotnet build GestionDeFardos.sln -c Debug`
- `scripts\release.cmd`
- `scripts\build-installer.cmd`

Validar:

- salida en `artifacts/publish/win-x64`
- instalador en `artifacts/dist`
- presencia de `config.example.json`
- presencia de documentacion `.md` en `publish/docs`

## Configuracion inicial

- Verificar `config.json` valido.
- Verificar `Scale.Protocol = "w180-t"` por default.
- Verificar `Database.FilePath`.
- Verificar `Export.Folder`.
- Verificar `Passwords.Edit`.
- Verificar `Passwords.Service`.

## Pruebas end-to-end con simulador

### Escenario base

- App configurada con puertos virtuales correctos.
- Simulador o puente enviando balanza.
- Simulador enviando pulsador.

Validar:

- peso visible en pantalla principal
- ultima opresion visible
- respuesta `$B1!` al pulsador

### Captura correcta

- Enviar un peso dentro de rango.
- Disparar el pulsador.

Validar:

- se guarda un nuevo registro
- la pantalla principal actualiza el ultimo registro
- el dato queda persistido al reiniciar la app

### Captura fuera de rango

- Enviar un peso por debajo o por encima de thresholds.
- Disparar el pulsador.

Validar:

- la captura se rechaza
- no se inserta un registro nuevo
- el motivo queda visible

### Diagnostico raw

- Enviar datos invalidos o mal configurados desde la balanza.

Validar:

- Service abre igual
- aparece el chunk crudo
- el error queda visible

## Edicion de registros

- Probar `Editar registro` con `Passwords.Edit` correcta.
- Probar con clave incorrecta.
- Probar con base vacia.

Validar:

- el peso pasa a `0`
- el registro se conserva
- el ultimo registro visible se refresca si corresponde
- la clave de Service no habilita la edicion

## Exportacion a Excel

- Exportar un rango con datos.
- Exportar un rango sin datos.

Validar:

- se genera `.xlsx`
- nombre del archivo legible con `desde`, `hasta` y `hecha`
- columnas `Nro de Fardo`, `Dia`, `Hora`, `Kg`
- los registros editados salen con `0`
- la UI no queda bloqueada despues de exportar

## Modo Service

- Abrir con `Ctrl+Shift+S`
- Abrir con `Ctrl+Alt+Shift+S`
- Probar clave correcta e incorrecta

Validar:

- diagnostico de balanza
- diagnostico de pulsador
- chunks raw
- tramas interpretadas

## Borrado historico

- Seleccionar una fecha con registros previos.
- Ejecutar las dos confirmaciones.

Validar:

- se borran los registros hasta esa fecha inclusive
- la cantidad eliminada se informa correctamente
- el ultimo registro mostrado se refresca

## Logging

- Verificar creacion de `logs`
- Verificar archivo diario

Confirmar que se registran:

- inicio y cierre de la app
- apertura y cierre de puertos
- tramas raw
- interpretacion de balanza
- `BUTTON RX $P1!`
- `BUTTON TX $B1!`
- guardados
- ediciones
- exportaciones
- borrados
