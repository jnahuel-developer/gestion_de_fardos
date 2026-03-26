# Manual operativo

## Objetivo

La aplicacion permite operar la captura de pesadas con balanza y pulsador seriales, guardar los registros en una base local, corregir pesadas, exportarlas a Excel y diagnosticar la comunicacion desde Modo Service.

## Inicio de la aplicacion

1. Verificar `config.json` junto al ejecutable.
2. Abrir `GestionDeFardos.App.exe`.
3. Esperar la apertura de la pantalla principal.

Al iniciar:

- se carga la configuracion
- se prepara la base local
- se abre el runtime compartido
- se inician la balanza y el pulsador segun lo definido en `config.json`

## Pantalla principal

La pantalla principal muestra:

- peso actual en kg
- estado de conexion de la balanza
- ultima opresion del pulsador
- ultimo registro guardado
- boton `Editar registro`
- boton `Exportar a Excel`

La pantalla principal no expone parametros de configuracion ni instrucciones de acceso tecnico.

## Captura automatica

- La balanza entrega el peso actual por su puerto serie.
- El pulsador trabaja por otro puerto.
- Cuando la app recibe `$P1!`, responde `$B1!`.
- Con cada `$P1!`, la app intenta guardar una pesada usando el ultimo peso valido disponible.

Condiciones de rechazo:

- no hay peso valido
- el peso esta fuera de `Thresholds.MinKg` y `Thresholds.MaxKg`
- falla la persistencia local

En todos los casos el resultado queda visible en la pantalla principal.

## Edicion de registros

Desde `Editar registro`:

1. Se solicita el numero de pesada.
2. Se solicita `Passwords.Edit`.
3. Si la clave es correcta, la pesada se actualiza a `0`.

Reglas:

- el registro no se elimina
- el cambio queda marcado como edicion
- si no hay registros, el popup no se abre
- el selector de numero queda limitado al ultimo Id actual

## Exportacion a Excel

Desde `Exportar a Excel`:

1. Se solicita un rango `Desde / Hasta`.
2. La app busca registros en la base local.
3. Si hay datos, genera un `.xlsx` en `Export.Folder`.

Nombre del archivo:

- `pesadas_desde_YYYYMMDD_hasta_YYYYMMDD_hecha_YYYYMMDD_HHMMSS.xlsx`

Columnas exportadas:

- `Nro de Fardo`
- `Dia`
- `Hora`
- `Kg`

## Acceso a Modo Service

Atajos disponibles:

- `Ctrl+Shift+S`
- `Ctrl+Alt+Shift+S`

Flujo:

1. Se abre un modal de contrasena.
2. Se valida `Passwords.Service`.
3. Si la clave es correcta, se abre la pantalla Service.

## Que muestra Modo Service

### Balanza

- protocolo configurado
- puerto y perfil serie
- estado de conexion
- ultimo chunk crudo
- ultima trama interpretada
- peso y tara interpretados
- ultimo error o diagnostico

### Pulsador

- puerto y perfil serie
- estado de conexion
- ultimo chunk crudo
- ultima trama `$P1!`
- ultima respuesta `$B1!`
- ultimo error o diagnostico

### Administracion

- selector de fecha
- boton `Borrar hasta fecha`
- doble confirmacion obligatoria
- resultado visible de cantidad de registros borrados

## Borrado historico

El borrado desde Service elimina todos los registros con fecha hasta la fecha seleccionada inclusive.

Reglas:

- requiere doble confirmacion
- refresca el ultimo registro mostrado por la app
- informa cantidad de registros borrados

## Logs

Ubicacion:

- `.\logs`

Archivo:

- `gestion-de-fardos-YYYYMMDD.log`

Se registran:

- inicio y cierre de la app
- apertura y cierre de puertos
- recepcion raw
- tramas interpretadas
- eventos de captura
- ediciones
- exportaciones
- borrados historicos
- errores

## Configuracion operativa

`config.json` define:

- `Scale`
- `Button`
- `Database`
- `Thresholds`
- `Passwords`
- `Export`

Todo cambio en `config.json` requiere reiniciar la app.
