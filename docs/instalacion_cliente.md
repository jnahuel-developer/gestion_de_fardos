# Instructivo de instalacion y puesta en marcha

## Alcance de esta entrega

Esta entrega corresponde a la version final operativa `1.1.0` de Gestion de Fardos.

Incluye:

- lectura de balanza por puerto serie
- lectura de pulsador por puerto serie independiente
- guardado automatico en base local
- edicion de registros
- exportacion a Excel
- Modo Service con diagnostico y borrado historico

## Instalacion

1. Ejecutar el instalador `GestionDeFardos-Setup-1.1.0-x64.exe`.
2. Confirmar la carpeta de destino.
3. Finalizar la instalacion.
4. Abrir la carpeta instalada.

## Configuracion inicial

Antes de usar la app por primera vez:

1. Abrir `config.json`.
2. Completar `Scale` con el puerto y perfil de la balanza.
3. Completar `Button` con el puerto y perfil del pulsador.
4. Revisar `Database.FilePath`.
5. Revisar `Thresholds`.
6. Definir `Passwords.Edit`.
7. Definir `Passwords.Service`.
8. Revisar `Export.Folder`.
9. Guardar el archivo.

Notas:

- `Scale.Protocol` define el formato de datos, no la configuracion fisica del puerto.
- El valor recomendado por default es `w180-t`.
- Si cambia `config.json`, hay que cerrar y volver a abrir la aplicacion.

## Primera puesta en marcha

1. Abrir la aplicacion.
2. Confirmar que el peso actual aparece en pantalla.
3. Confirmar que la ultima opresion cambia al accionar el pulsador.
4. Confirmar que el ultimo registro guardado se actualiza despues de una captura valida.

## Operacion diaria

Desde la pantalla principal se puede:

- ver el peso actual
- revisar el estado de la ultima opresion
- editar una pesada
- exportar registros a Excel

## Edicion de registros

1. Presionar `Editar registro`.
2. Seleccionar el numero de pesada.
3. Ingresar la contrasena de edicion.
4. Confirmar.

Resultado:

- la pesada queda en `0`
- el registro no se elimina

## Exportacion a Excel

1. Presionar `Exportar a Excel`.
2. Seleccionar `Desde` y `Hasta`.
3. Confirmar.

Resultado:

- se genera un archivo `.xlsx` en la carpeta configurada en `Export.Folder`

## Modo Service

Para acceder:

- `Ctrl+Shift+S`
- o `Ctrl+Alt+Shift+S`

Service permite:

- ver diagnostico raw e interpretado de balanza
- ver diagnostico del pulsador
- borrar registros historicos por fecha

## Logs y diagnostico

Ubicacion:

- `.\logs`

Archivo diario:

- `gestion-de-fardos-YYYYMMDD.log`

Si hay problemas de comunicacion o de guardado, revisar ese archivo.
