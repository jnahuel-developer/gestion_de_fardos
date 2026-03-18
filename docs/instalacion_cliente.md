# Instructivo de instalacion y puesta en marcha

## Alcance de esta entrega

Esta entrega corresponde al Modulo Service de Gestion de Fardos. Permite validar:

- Lectura de balanza por puerto serial.
- Visualizacion de la ultima trama ASCII.
- Deteccion del pulsador por lineas de control.
- Registro basico de eventos y errores.

## Requisitos previos en la PC

- Windows x64.
- Permisos para ejecutar el instalador.
- Puerto serial fisico o adaptador USB-Serial correctamente instalado.
- Driver del adaptador serial instalado, si corresponde.
- Dato del puerto COM real que usara la balanza.
- Parametros seriales de la balanza disponibles.

## Instalacion

1. Ejecutar el instalador `GestionDeFardos-Setup-...exe`.
2. Confirmar la carpeta de instalacion propuesta. La recomendada es `C:\GestionDeFardos`.
3. Finalizar el asistente.
4. Verificar que la carpeta de instalacion contiene el ejecutable, `config.json` o `config.template.json`, la carpeta `docs` y luego `logs` cuando la app se ejecute.

## Actualizacion a una nueva version

1. Cerrar la aplicacion si estuviera abierta.
2. Ejecutar el nuevo instalador `GestionDeFardos-Setup-...exe`.
3. Indicar la misma carpeta de instalacion usada previamente. La recomendada es `C:\GestionDeFardos`.
4. Finalizar la instalacion.
5. Verificar que `config.json` conserve los parametros ya validados en planta.

Notas importantes:

- La actualizacion reemplaza los binarios de la aplicacion.
- `config.json` se preserva automaticamente durante la reinstalacion sobre la misma carpeta.
- La carpeta `logs` no se incluye dentro del instalador, por lo que el historial existente no se pisa.
- No es necesario desinstalar primero para pasar de una version a otra en la misma PC.

## Configuracion inicial

1. Abrir `config.json` en la carpeta de instalacion.
2. Completar `Scale.PortName`, `BaudRate`, `Parity`, `DataBits`, `StopBits` y `NewLine`.
3. Configurar `Button.InputLine` con `Cts` o `Dsr`.
4. Definir `Passwords.Service`.
5. Guardar el archivo.

## Primera validacion en planta

1. Abrir la aplicacion.
2. Ingresar a Service con `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
3. Confirmar que se visualizan:
   - peso convertido
   - ultima trama ASCII
   - conexion del puerto
   - linea configurada
   - estados de `CTS` y `DSR`
   - estado del pulsador
   - ultima opresion o diagnostico
4. Si la balanza o el pulsador no responden, revisar `logs`.

## Logs

- Ubicacion: `logs` junto al ejecutable.
- Nombre diario: `gestion-de-fardos-YYYYMMDD.log`.
- Uso principal: diagnostico de lectura serial, pulsador y errores de puerto.

## Desinstalacion

- Con el instalador actual de esta etapa, la aplicacion no queda registrada en "Aplicaciones instaladas" de Windows.
- Si se desea retirar la aplicacion, cerrar el programa y eliminar manualmente la carpeta de instalacion, por ejemplo `C:\GestionDeFardos`.
- Si existen accesos directos en escritorio o menu Inicio, eliminarlos manualmente.
- Antes de eliminar la carpeta, respaldar `config.json` y `logs` si se desea conservar la configuracion o el historial de validacion.
