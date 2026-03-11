# gestion_de_fardos

Proyecto base para el sistema de gestión de fardos con arquitectura por capas y solución .NET 8.

## Requisitos

- .NET 8 SDK

## Compilación y ejecución

### Opción recomendada (Windows)

1. Abrir una terminal en cualquier carpeta.
2. Ejecutar el wrapper:
   ```cmd
   scripts\dev.cmd
   ```

El script `scripts/dev.ps1` detecta automáticamente la raíz del repositorio y ejecuta, en este orden:

1. `dotnet --info`
2. `dotnet restore GestionDeFardos.sln`
3. `dotnet build GestionDeFardos.sln -c Debug`
4. `dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug`

Si no hay `dotnet` en el `PATH` o no está instalado un SDK 8.x, el script finaliza con un mensaje de error claro.

### Alternativa manual

1. Restaurar dependencias:
   ```bash
   dotnet restore GestionDeFardos.sln
   ```
2. Compilar la solución en Debug:
   ```bash
   dotnet build GestionDeFardos.sln -c Debug
   ```
3. Ejecutar la aplicación WinForms en Debug:
   ```bash
   dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug
   ```

## Estructura del repositorio

- `src/GestionDeFardos.App`: aplicación WinForms.
- `src/GestionDeFardos.Core`: contratos, modelos y utilitarios sin IO.
- `src/GestionDeFardos.Infrastructure`: stubs para implementaciones futuras.
- `docs/`: documentación de arquitectura, operación y reglas para contribución.
- `samples/`: archivos de ejemplo de configuración.
