-- Script SQL para crear las tablas en la base de datos pepsico_taller
-- PepsiCo Chile - Sistema de Gestión de Taller

-- Conectar a la instancia correcta y crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'pepsico_taller')
BEGIN
    CREATE DATABASE pepsico_taller;
    PRINT 'Base de datos pepsico_taller creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La base de datos pepsico_taller ya existe';
END
GO

USE pepsico_taller;
GO

-- Tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Usuarios] (
   [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Nombre] NVARCHAR(100) NOT NULL,
[Apellido] NVARCHAR(100) NOT NULL,
     [Email] NVARCHAR(200) NOT NULL UNIQUE,
        [Telefono] NVARCHAR(20) NULL,
        [Rol] NVARCHAR(50) NOT NULL, -- Chofer, Supervisor, Mecanico
        [Rut] NVARCHAR(20) NOT NULL UNIQUE,
        [Password] NVARCHAR(255) NOT NULL,
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Usuarios creada exitosamente';
END
ELSE
BEGIN
  PRINT 'La tabla Usuarios ya existe';
END
GO

-- Tabla Vehiculos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vehiculos]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Vehiculos] (
   [Id] INT IDENTITY(1,1) PRIMARY KEY,
      [Patente] NVARCHAR(10) NOT NULL UNIQUE,
        [Marca] NVARCHAR(50) NOT NULL,
        [Modelo] NVARCHAR(50) NOT NULL,
        [Año] INT NOT NULL,
 [TipoVehiculo] NVARCHAR(50) NOT NULL, -- Camión, Camioneta, etc.
    [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Disponible', -- Disponible, En Taller, En Ruta
        [KilometrajeActual] INT NULL
    );
   PRINT 'Tabla Vehiculos creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla Vehiculos ya existe';
END
GO

-- Tabla IngresosTaller
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IngresosTaller]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[IngresosTaller] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [VehiculoId] INT NOT NULL FOREIGN KEY REFERENCES Vehiculos(Id),
        [ChoferId] INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
  [FechaProgramada] DATETIME2 NOT NULL,
        [FechaIngresoReal] DATETIME2 NULL,
        [FechaSalidaEstimada] DATETIME2 NULL,
     [FechaSalidaReal] DATETIME2 NULL,
      [MotivoIngreso] NVARCHAR(100) NOT NULL, -- Mantenimiento, Reparación, Siniestro
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Programado', -- Programado, En Proceso, Pausado, Completado, Cancelado
        [DescripcionProblema] NVARCHAR(MAX) NULL,
        [ObservacionesChofer] NVARCHAR(MAX) NULL,
[SupervisorId] INT NULL FOREIGN KEY REFERENCES Usuarios(Id),
   [RequiereRepuestos] BIT NOT NULL DEFAULT 0,
        [KilometrajeIngreso] INT NULL
    );
    PRINT 'Tabla IngresosTaller creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla IngresosTaller ya existe';
END
GO

-- Tabla TareasTaller
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TareasTaller]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TareasTaller] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
   [IngresoTallerId] INT NOT NULL FOREIGN KEY REFERENCES IngresosTaller(Id) ON DELETE CASCADE,
     [Descripcion] NVARCHAR(MAX) NOT NULL,
        [MecanicoAsignadoId] INT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        [FechaAsignacion] DATETIME2 NOT NULL,
        [FechaInicio] DATETIME2 NULL,
   [FechaFinalizacion] DATETIME2 NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- Pendiente, En Proceso, Completada, Cancelada
   [Observaciones] NVARCHAR(MAX) NULL,
      [TiempoEstimadoHoras] INT NOT NULL,
[TiempoRealHoras] INT NULL,
  [Prioridad] NVARCHAR(50) NOT NULL DEFAULT 'Media' -- Alta, Media, Baja
    );
    PRINT 'Tabla TareasTaller creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla TareasTaller ya existe';
END
GO

-- Tabla Pausas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pausas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Pausas] (
     [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [IngresoTallerId] INT NOT NULL FOREIGN KEY REFERENCES IngresosTaller(Id) ON DELETE CASCADE,
        [FechaInicio] DATETIME2 NOT NULL,
        [FechaFin] DATETIME2 NULL,
        [Motivo] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(MAX) NOT NULL,
  [UsuarioRegistroId] INT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        [Activa] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Pausas creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla Pausas ya existe';
END
GO

-- Tabla Documentos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documentos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Documentos] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [IngresoTallerId] INT NOT NULL FOREIGN KEY REFERENCES IngresosTaller(Id) ON DELETE CASCADE,
[TipoDocumento] NVARCHAR(50) NOT NULL, -- Foto, Informe, Presupuesto, Orden de Trabajo
        [NombreArchivo] NVARCHAR(255) NOT NULL,
        [RutaArchivo] NVARCHAR(500) NOT NULL,
        [FechaSubida] DATETIME2 NOT NULL,
        [UsuarioSubidaId] INT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        [Descripcion] NVARCHAR(MAX) NULL,
  [TamañoBytes] BIGINT NOT NULL
  );
    PRINT 'Tabla Documentos creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla Documentos ya existe';
END
GO

-- Tabla Repuestos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Repuestos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Repuestos] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
[TareaTallerId] INT NOT NULL FOREIGN KEY REFERENCES TareasTaller(Id) ON DELETE CASCADE,
        [Nombre] NVARCHAR(200) NOT NULL,
[CodigoRepuesto] NVARCHAR(100) NOT NULL,
      [Cantidad] INT NOT NULL,
      [Proveedor] NVARCHAR(200) NULL,
    [FechaSolicitud] DATETIME2 NULL,
     [FechaRecepcion] DATETIME2 NULL,
  [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Solicitado' -- Solicitado, En Tránsito, Recibido, Instalado
    );
    PRINT 'Tabla Repuestos creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla Repuestos ya existe';
END
GO

-- Insertar usuarios de prueba
PRINT 'Insertando usuarios de prueba...';

-- Password hash de "123456" usando SHA256
DECLARE @password NVARCHAR(255) = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI='; -- Hash correcto de "123456"

-- Supervisor
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'supervisor@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('María', 'López', 'supervisor@pepsico.cl', '+56912345678', 'Supervisor', '12.345.678-9', @password, 1);
    PRINT 'Usuario Supervisor creado';
END

-- Chofer 1
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'juan.perez@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Juan', 'Pérez', 'juan.perez@pepsico.cl', '+56987654321', 'Chofer', '11.222.333-4', @password, 1);
 PRINT 'Usuario Chofer 1 creado';
END

-- Chofer 2
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'pedro.gonzalez@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Pedro', 'González', 'pedro.gonzalez@pepsico.cl', '+56998877665', 'Chofer', '22.333.444-5', @password, 1);
    PRINT 'Usuario Chofer 2 creado';
END

-- Mecánico 1
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'carlos.rojas@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
 VALUES ('Carlos', 'Rojas', 'carlos.rojas@pepsico.cl', '+56955443322', 'Mecanico', '33.444.555-6', @password, 1);
    PRINT 'Usuario Mecánico 1 creado';
END

-- Mecánico 2
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'luis.munoz@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Luis', 'Muñoz', 'luis.munoz@pepsico.cl', '+56911223344', 'Mecanico', '44.555.666-7', @password, 1);
    PRINT 'Usuario Mecánico 2 creado';
END

PRINT '';
PRINT '=================================================================';
PRINT 'Base de datos configurada correctamente.';
PRINT 'Usuarios de prueba creados con contraseña: 123456';
PRINT '';
PRINT 'Usuarios disponibles:';
PRINT '  supervisor@pepsico.cl - Supervisor';
PRINT '  juan.perez@pepsico.cl - Chofer';
PRINT '  pedro.gonzalez@pepsico.cl - Chofer';
PRINT '  carlos.rojas@pepsico.cl - Mecánico';
PRINT '  luis.munoz@pepsico.cl - Mecánico';
PRINT '=================================================================';
GO
