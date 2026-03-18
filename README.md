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

El script `scripts/dev.ps1` detecta automaticamente la raiz del repositorio y ejecuta, en este orden:

1. `dotnet --info`
2. `dotnet restore GestionDeFardos.sln`
3. `dotnet build GestionDeFardos.sln -c Debug`
4. `dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug`

Si no hay `dotnet` en el `PATH` o no esta instalado un SDK 8.x, el script finaliza con un mensaje de error claro.

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

`build-installer` usa Inno Setup si `iscc` esta disponible. Si no, cae automaticamente a IExpress para generar un instalador `.exe` nativo de Windows.

Artefactos esperados:

- `artifacts/publish/win-x64`: binarios publicados para la entrega.
- `artifacts/dist`: instalador `.exe` generado por Inno Setup.

## Estructura del repositorio

- `src/GestionDeFardos.App`: aplicacion WinForms.
- `src/GestionDeFardos.Core`: contratos, modelos y utilitarios sin IO.
- `src/GestionDeFardos.Infrastructure`: acceso a configuracion, puerto serial, logging y adaptadores tecnicos.
- `docs/`: documentacion operativa, arquitectura, instalacion y checklist.
- `samples/`: archivos de ejemplo de configuracion.
- `installer/`: script `.iss` del instalador.
