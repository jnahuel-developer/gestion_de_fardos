# Roadmap de Desarrollo

## Estado final

- Etapa 1: completada
- Etapa 2: completada
- Version objetivo de cierre: `1.1.0`

## Mods implementadas

### `mod0005`

- Base local SQLite
- modelo de datos de pesadas
- bootstrap automatico

### `mod0006`

- runtime compartido
- captura automatica al pulsador
- validacion por thresholds

### `mod0007`

- pantalla principal operativa
- peso actual
- estado de ultima opresion
- ultimo registro guardado

### `mod0008`

- edicion de registros por numero
- validacion de `Passwords.Edit`
- actualizacion a cero

### `mod0009`

- exportacion a Excel
- rango de fechas
- salida `.xlsx`

### `mod0010`

- borrado historico desde Service
- doble confirmacion
- refresco del ultimo registro

### `mod0011`

- cierre de UX y logging
- alineacion de version final
- documentacion operativa final
- scripts de publicacion e instalador preparados para la entrega

## Resultado funcional final

La app final permite:

- leer balanza y pulsador por puertos independientes
- guardar pesadas automaticamente
- rechazar capturas fuera de rango
- editar registros
- exportar a Excel
- diagnosticar la comunicacion en Service
- borrar historico por fecha
- operar con base local SQLite
