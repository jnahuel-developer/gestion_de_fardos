# gestion_de_fardos

Aplicacion WinForms para captura de pesadas con balanza y pulsador seriales, persistencia local SQLite, edicion de registros, exportacion a Excel y diagnostico tecnico desde Modo Service.

## Estado actual

- Etapa 1 finalizada y validada en cliente.
- Etapa 2 finalizada.
- Version objetivo de entrega: `2.0.0`.

## Funcionalidad principal

- Lectura continua de balanza por puerto serie con protocolo configurable:
  - `w180-t`
  - `simple-ascii`
- Lectura del pulsador por puerto serie independiente.
- Registro automatico de pesadas al recibir `$P1!`.
- Respuesta `$B1!` al pulsador.
- Validacion por thresholds antes de guardar.
- Base local SQLite.
- Edicion de una pesada por numero, dejandola en `0`.
- Exportacion de registros a `.xlsx`.
- Borrado historico por fecha desde Modo Service.
- Logging diario y diagnostico raw de balanza y pulsador.

## Estructura de la solucion

- `src/GestionDeFardos.App`: interfaz WinForms.
- `src/GestionDeFardos.Core`: configuracion, contratos y modelos.
- `src/GestionDeFardos.Infrastructure`: serial, logging, SQLite y exportacion Excel.
- `docs`: documentacion tecnica y operativa.
- `samples/config.example.json`: configuracion de referencia.
- `installer`: script Inno Setup.
- `scripts`: helpers de compilacion, publicacion e instalador.

## Configuracion

La app lee `config.json` desde la carpeta del ejecutable al iniciar.

Secciones relevantes:

- `Scale`: puerto, protocolo y posicion de coma de balanza.
- `Button`: puerto del pulsador.
- `Database`: ruta del archivo SQLite.
- `Thresholds`: rango valido de kg para guardar.
- `Passwords`: claves de edicion y Service.
- `Export`: carpeta de salida para los `.xlsx`.

Si se cambia `config.json`, hay que cerrar y volver a abrir la app.

## Compilacion y ejecucion

### Desarrollo

```cmd
scripts\dev.cmd
```

### Manual

```bash
dotnet restore GestionDeFardos.sln
dotnet build GestionDeFardos.sln -c Debug
dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug
```

## Publicacion

Publicacion self-contained x64 con defaults de la version `2.0.0`:

```powershell
scripts\release.cmd
```

Salida esperada:

- `artifacts/publish/win-x64`

La publicacion copia:

- binarios de la app
- `config.example.json`
- `config.template.json`
- documentacion `.md` desde `docs`

## Instalador

Compilacion del instalador:

```powershell
scripts\build-installer.cmd
```

Salida esperada:

- `artifacts/dist/GestionDeFardos-Setup-2.0.0-x64.exe`
- `artifacts/dist/instalacion_cliente.md`

Si `iscc` no esta disponible en el PATH, el script usa el bootstrapper integrado.

## Flujo operativo resumido

1. La pantalla principal muestra el peso actual, la ultima opresion y el ultimo registro guardado.
2. Cada `$P1!` dispara un intento de guardado usando el ultimo peso valido disponible.
3. El usuario puede editar un registro desde la pantalla principal.
4. El usuario puede exportar registros por rango de fechas desde la pantalla principal.
5. `Ctrl+Shift+S` o `Ctrl+Alt+Shift+S` abren Modo Service.
6. Service muestra diagnostico raw e interpretado y permite borrar historico por fecha.

## Logs

- Carpeta: `./logs`
- Nombre diario: `gestion-de-fardos-YYYYMMDD.log`
- Incluye:
  - apertura y cierre de puertos
  - recepcion raw
  - interpretacion de balanza
  - `BUTTON RX $P1!`
  - `BUTTON TX $B1!`
  - capturas, ediciones, exportaciones y borrados

## Documentacion relacionada

- `docs/manual_operativo.md`
- `docs/instalacion_cliente.md`
- `docs/checklist_pruebas.md`
- `docs/arquitectura.md`
- `docs/Roadmap.md`
