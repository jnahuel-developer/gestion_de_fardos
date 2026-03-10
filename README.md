# gestion_de_fardos

Proyecto base para el sistema de gestión de fardos con arquitectura por capas y solución .NET 8.

## Requisitos

- .NET 8 SDK

## Compilación y ejecución

1. Restaurar dependencias:
   ```bash
   dotnet restore GestionDeFardos.sln
   ```
2. Compilar la solución:
   ```bash
   dotnet build GestionDeFardos.sln
   ```
3. Ejecutar la aplicación WinForms:
   ```bash
   dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj
   ```

## Estructura del repositorio

- `src/GestionDeFardos.App`: aplicación WinForms.
- `src/GestionDeFardos.Core`: contratos, modelos y utilitarios sin IO.
- `src/GestionDeFardos.Infrastructure`: stubs para implementaciones futuras.
- `docs/`: documentación de arquitectura, operación y reglas para contribución.
- `samples/`: archivos de ejemplo de configuración.
