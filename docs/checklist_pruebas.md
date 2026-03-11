# Checklist de pruebas

## Skeleton actual

- Restauración de dependencias de la solución.
- Compilación completa de la solución.
- Ejecución básica de la aplicación WinForms.

## Pruebas manuales - Acceso a Modo Service

- Verificar que `Ctrl+Shift+S` abre el prompt de contraseña.
- Verificar que `Ctrl+Alt+Shift+S` abre el prompt de contraseña.
- Verificar que al cancelar (`Esc` o botón **Cancelar**) no se abre `ServiceForm`.
- Verificar que contraseña incorrecta muestra error y no abre `ServiceForm`.
- Verificar que contraseña correcta abre `ServiceForm`.
- Verificar que si `ServiceForm` ya está abierta, la hotkey la trae al frente.
- Verificar mensaje de advertencia cuando una hotkey no puede registrarse.
- Verificar mensaje claro cuando no existe `config.json` y que indica cómo resolverlo.

## Placeholder para etapas futuras

- Validación de lectura serial.
- Validación de persistencia SQLite.
- Validación de exportación Excel.
