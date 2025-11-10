# Migración: Renombrar FechaRecepcion a FechaEntrega

## ? Archivos Creados

1. **20250110000000_RenombrarFechaRecepcionAFechaEntrega.cs** - Migración principal
2. **20250110000000_RenombrarFechaRecepcionAFechaEntrega.Designer.cs** - Designer de la migración
3. **RenombrarFechaRecepcionAFechaEntrega.sql** - Script SQL de referencia
4. **ApplicationDbContextModelSnapshot.cs** - Actualizado con FechaEntrega

## ?? Pasos para Aplicar la Migración

### 1. Detener la Aplicación
Detén IIS Express o la aplicación en ejecución.

### 2. Aplicar la Migración
```sh
dotnet ef database update --project PepsicoChile\PepsicoChile.csproj
```

### 3. Verificar en SQL Server
```sql
-- Verificar que la columna se renombró
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SolicitudesRepuesto' 
  AND COLUMN_NAME = 'FechaEntrega';

-- Verificar que FechaRecepcion ya no existe
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SolicitudesRepuesto' 
  AND COLUMN_NAME = 'FechaRecepcion';
```

### 4. Verificar los Datos
```sql
-- Ver solicitudes con sus fechas de entrega
SELECT Id, Nombre, FechaSolicitud, FechaEntrega, Estado
FROM SolicitudesRepuesto
ORDER BY FechaSolicitud DESC;
```

## ?? Rollback (Si es necesario)

Si necesitas revertir la migración:

```sh
dotnet ef database update AgregarGestionRepuestos --project PepsicoChile\PepsicoChile.csproj
```

Esto volverá a `FechaRecepcion`.

## ? Validación Post-Migración

1. ? Compilar el proyecto sin errores
2. ? Iniciar la aplicación
3. ? Navegar a las solicitudes de repuestos
4. ? Verificar que se muestra "Fecha Entrega" correctamente
5. ? Crear una nueva solicitud y cambiar su estado a "Entregado"
6. ? Verificar que se registra la fecha de entrega

## ?? Cambios en la Base de Datos

### Antes:
```
SolicitudesRepuesto
??? Id
??? TareaTallerId
??? Nombre
??? CodigoRepuesto
??? Cantidad
??? Proveedor
??? FechaSolicitud
??? FechaRecepcion  ? ANTES
??? Estado
??? SolicitadoPorId
```

### Después:
```
SolicitudesRepuesto
??? Id
??? TareaTallerId
??? Nombre
??? CodigoRepuesto
??? Cantidad
??? Proveedor
??? FechaSolicitud
??? FechaEntrega    ? AHORA
??? Estado
??? SolicitadoPorId
```

## ?? Impacto

- ? Sin pérdida de datos
- ? Los datos existentes se mantienen
- ? Solo cambia el nombre de la columna
- ? Las relaciones se mantienen intactas

## ?? Notas Importantes

1. Esta es una migración **segura** que solo renombra una columna
2. **No se pierden datos** - es solo un cambio de nombre
3. Asegúrate de tener un **backup** de la base de datos antes de aplicar
4. La migración es **reversible** usando el método `Down()`

## ? Resultado Final

Después de aplicar esta migración, el sistema usará consistentemente "Entregado" en lugar de "Recibido" para indicar que el AsistenteRepuestos entregó el repuesto al mecánico.
