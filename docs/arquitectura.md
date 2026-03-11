# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms y composición de la aplicación.
- **GestionDeFardos.Core**: modelos, contratos y reglas de negocio puras.
- **GestionDeFardos.Infrastructure**: implementaciones técnicas (en esta etapa, solo stubs).

## Ubicaciones de archivos

- Configuración: `AppContext.BaseDirectory`.
- Base de datos SQLite: `AppContext.BaseDirectory`.
- Logs: `./logs` relativo al ejecutable.

## Hotkeys (pendiente implementación)

- Combinación 1: registro de peso manual.
- Combinación 2: acceso rápido a operaciones de servicio.

## Portabilidad y permisos

Se debe instalar la aplicación en una carpeta con permisos de escritura para permitir persistencia local de configuración, base de datos y logs.
