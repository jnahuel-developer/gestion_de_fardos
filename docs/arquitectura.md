# Arquitectura base

## Capas

- **GestionDeFardos.App**: interfaz WinForms y composición de la aplicación.
- **GestionDeFardos.Core**: modelos, contratos y reglas de negocio puras.
- **GestionDeFardos.Infrastructure**: implementaciones técnicas (en esta etapa, solo carga mínima de configuración).

## Ubicaciones de archivos

- Configuración: `AppContext.BaseDirectory/config.json`.
- Base de datos SQLite: `AppContext.BaseDirectory`.
- Logs: `./logs` relativo al ejecutable.

## Acceso a Modo Service

- El acceso se inicia desde `MainForm` mediante `WM_HOTKEY` (WinAPI).
- Se registran dos combinaciones:
  - `Ctrl+Shift+S`
  - `Ctrl+Alt+Shift+S`
- Si la hotkey no puede registrarse (por conflicto con otra app), se muestra advertencia y la app continúa.
- Al detectar hotkey se solicita contraseña en un diálogo modal.
- La validación se realiza contra `Passwords.Service` leído desde `config.json`.
- Si `ServiceForm` ya está abierta, no se crea una nueva instancia; se trae al frente.

## Portabilidad y permisos

Se debe instalar la aplicación en una carpeta con permisos de escritura para permitir persistencia local de configuración, base de datos y logs.
