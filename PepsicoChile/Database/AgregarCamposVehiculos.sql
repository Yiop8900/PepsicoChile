-- Agregar campos adicionales a la tabla Vehiculos
USE pepsico_taller;
GO

-- Verificar si las columnas ya existen antes de agregarlas
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'KilometrajeActual')
BEGIN
    ALTER TABLE Vehiculos ADD KilometrajeActual INT NULL;
    PRINT 'Columna KilometrajeActual agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'Motor')
BEGIN
    ALTER TABLE Vehiculos ADD Motor NVARCHAR(50) NULL;
    PRINT 'Columna Motor agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'Chasis')
BEGIN
    ALTER TABLE Vehiculos ADD Chasis NVARCHAR(50) NULL;
    PRINT 'Columna Chasis agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'Color')
BEGIN
    ALTER TABLE Vehiculos ADD Color NVARCHAR(30) NULL;
    PRINT 'Columna Color agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'Combustible')
BEGIN
    ALTER TABLE Vehiculos ADD Combustible NVARCHAR(20) NULL;
    PRINT 'Columna Combustible agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'CapacidadCarga')
BEGIN
 ALTER TABLE Vehiculos ADD CapacidadCarga DECIMAL(10,2) NULL;
    PRINT 'Columna CapacidadCarga agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'UltimoMantenimiento')
BEGIN
    ALTER TABLE Vehiculos ADD UltimoMantenimiento DATE NULL;
    PRINT 'Columna UltimoMantenimiento agregada';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehiculos' AND COLUMN_NAME = 'ProximoMantenimiento')
BEGIN
    ALTER TABLE Vehiculos ADD ProximoMantenimiento DATE NULL;
    PRINT 'Columna ProximoMantenimiento agregada';
END

-- Crear tabla para documentos de vehículos si no existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocumentosVehiculo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DocumentosVehiculo] (
 [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VehiculoId] INT NOT NULL FOREIGN KEY REFERENCES Vehiculos(Id) ON DELETE CASCADE,
        [TipoDocumento] NVARCHAR(50) NOT NULL, -- RevisionTecnica, SeguroObligatorio, PermisoCirculacion, Contrato, Manual, Otro
        [NombreArchivo] NVARCHAR(255) NOT NULL,
        [RutaArchivo] NVARCHAR(500) NOT NULL,
        [FechaSubida] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [FechaVencimiento] DATE NULL,
        [UsuarioSubidaId] INT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    [Descripcion] NVARCHAR(MAX) NULL,
        [TamañoBytes] BIGINT NOT NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DocumentosVehiculo creada';
END
ELSE
BEGIN
    PRINT 'La tabla DocumentosVehiculo ya existe';
END

PRINT 'Script completado exitosamente';
GO
