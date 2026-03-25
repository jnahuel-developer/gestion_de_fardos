# gestion_de_fardos

Proyecto base para el sistema de gestion de fardos con arquitectura por capas y solucion .NET 8.

## Requisitos

- .NET 8 SDK con soporte WindowsDesktop operativo.
- Inno Setup 6.x para compilar el instalador `.exe` de entrega.

## Compilacion y ejecucion

### Opcion recomendada (Windows)

1. Abrir una terminal en cualquier carpeta.
2. Ejecutar el wrapper:
   ```cmd
   scripts\dev.cmd
   ```

### Alternativa manual

1. Restaurar dependencias:
   ```bash
   dotnet restore GestionDeFardos.sln
   ```
2. Compilar la solucion en Debug:
   ```bash
   dotnet build GestionDeFardos.sln -c Debug
   ```
3. Ejecutar la aplicacion WinForms en Debug:
   ```bash
   dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug
   ```

## Publicacion e instalador

- Publicacion self-contained x64:
  ```powershell
  scripts\release.cmd
  ```
- Compilacion del instalador `.exe`:
  ```powershell
  scripts\build-installer.cmd
  ```

Artefactos esperados:

- `artifacts/publish/win-x64`: binarios publicados para la entrega.
- `artifacts/dist`: instalador `.exe` generado por Inno Setup.

## Configuracion serial actual

- La balanza y el pulsador usan puertos serie independientes.
- `Scale.Protocol` define como interpretar las tramas de la balanza, no la configuracion fisica del puerto.
- Protocolos soportados por la app:
  - `w180-t` (default)
  - `simple-ascii`
- La configuracion fisica de ambos puertos se define en `config.json`:
  - `PortName`
  - `BaudRate`
  - `DataBits`
  - `Parity`
  - `StopBits`
  - `Handshake`
- `Scale.NewLine` solo aplica a protocolos por linea como `simple-ascii`.
- El pulsador trabaja por tramas `$P1!` / `$B1!`.

## Diagnostico en Service

- Service ya no bloquea por configuraciones seriales no esperadas.
- La pantalla muestra el ultimo chunk crudo recibido de la balanza y la ultima trama interpretada correctamente.
- Si el protocolo configurado no es soportado, Service sigue abriendo y muestra solo recepcion cruda.
