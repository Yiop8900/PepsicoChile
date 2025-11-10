-- Migración: RenombrarFechaRecepcionAFechaEntrega
-- Esta migración renombra la columna FechaRecepcion a FechaEntrega en la tabla SolicitudesRepuesto

-- SQL que se ejecutará:
EXEC sp_rename 'dbo.SolicitudesRepuesto.FechaRecepcion', 'FechaEntrega', 'COLUMN';

-- Insertar en historial de migraciones
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250110000000_RenombrarFechaRecepcionAFechaEntrega', N'9.0.10');
