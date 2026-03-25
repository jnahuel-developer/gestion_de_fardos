# Instructivo de instalacion y puesta en marcha

## Alcance de esta entrega

Esta entrega corresponde al Modulo Service de Gestion de Fardos. Permite validar:

- Lectura de balanza por puerto serial.
- Lectura de balanza por protocolo configurable (`w180-t` o `simple-ascii`).
- Lectura del pulsador por puerto serial dedicado.
- Visualizacion de recepcion cruda e interpretada para diagnostico.
- Registro basico de eventos y errores.

## Configuracion inicial

1. Abrir `config.json` en la carpeta de instalacion.
2. Completar todos los parametros de la balanza en `Scale`.
3. Completar todos los parametros del pulsador en `Button`, si se utilizara.
4. Definir `Passwords.Service`.
5. Guardar el archivo.

Notas:

- `Scale.Protocol` define el formato de trama esperado, no la configuracion fisica del puerto.
- El valor recomendado por default es `Scale.Protocol = "w180-t"`.
- Si `Scale.Protocol` no es valido, Service abre igual y muestra solo recepcion cruda.
- Si `Button.PortName` queda vacio, Service abre sin pulsador.
- La app lee `config.json` solo al iniciar. Si se cambia, hay que cerrar y volver a abrir la aplicacion.

## Primera validacion en planta

1. Abrir la aplicacion.
2. Ingresar a Service con `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S`.
3. Confirmar que se visualizan:
   - protocolo configurado
   - puerto y perfil de balanza
   - ultimo chunk crudo de balanza
   - ultima trama interpretada
   - peso y tara, si existen
   - puerto y perfil del pulsador
   - ultima trama `$P1!`
   - ultima respuesta `$B1!`
   - diagnosticos de error si corresponden
4. Si la balanza o el pulsador no responden, revisar `logs`.
