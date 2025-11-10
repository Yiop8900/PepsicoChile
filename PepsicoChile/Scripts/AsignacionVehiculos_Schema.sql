CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Apellido] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Telefono] nvarchar(20) NOT NULL,
    [Rol] nvarchar(50) NOT NULL,
    [Rut] nvarchar(20) NOT NULL,
    [Password] nvarchar(255) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Vehiculos] (
    [Id] int NOT NULL IDENTITY,
    [Patente] nvarchar(10) NOT NULL,
    [Marca] nvarchar(50) NOT NULL,
    [Modelo] nvarchar(50) NOT NULL,
    [Año] int NOT NULL,
    [TipoVehiculo] nvarchar(50) NOT NULL,
    [NumeroFlota] nvarchar(50) NULL,
    [Observaciones] nvarchar(500) NULL,
    [Estado] nvarchar(50) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaActualizacion] datetime2 NOT NULL,
    [KilometrajeActual] int NULL,
    [Motor] nvarchar(50) NULL,
    [Chasis] nvarchar(50) NULL,
    [Color] nvarchar(30) NULL,
    [Combustible] nvarchar(20) NULL,
    [CapacidadCarga] decimal(18,2) NULL,
    [UltimoMantenimiento] datetime2 NULL,
    [ProximoMantenimiento] datetime2 NULL,
    CONSTRAINT [PK_Vehiculos] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Notificaciones] (
    [Id] int NOT NULL IDENTITY,
    [UsuarioId] int NOT NULL,
    [Titulo] nvarchar(100) NOT NULL,
    [Mensaje] nvarchar(500) NOT NULL,
    [Tipo] nvarchar(50) NOT NULL,
    [Leida] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaLeida] datetime2 NULL,
    [TareaId] int NULL,
    [IngresoId] int NULL,
    [VehiculoId] int NULL,
    [UrlAccion] nvarchar(200) NULL,
    [EnviadoWhatsApp] bit NOT NULL,
    [FechaEnvioWhatsApp] datetime2 NULL,
    [WhatsAppSid] nvarchar(max) NULL,
    CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AsignacionesVehiculo] (
    [Id] int NOT NULL IDENTITY,
    [VehiculoId] int NOT NULL,
    [ChoferId] int NOT NULL,
    [FechaAsignacion] datetime2 NOT NULL,
    [FechaDesasignacion] datetime2 NULL,
    [Activa] bit NOT NULL,
    [Observaciones] nvarchar(500) NULL,
    [AsignadoPorId] int NULL,
    CONSTRAINT [PK_AsignacionesVehiculo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AsignacionesVehiculo_Usuarios_AsignadoPorId] FOREIGN KEY ([AsignadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AsignacionesVehiculo_Usuarios_ChoferId] FOREIGN KEY ([ChoferId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AsignacionesVehiculo_Vehiculos_VehiculoId] FOREIGN KEY ([VehiculoId]) REFERENCES [Vehiculos] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [DocumentosVehiculo] (
    [Id] int NOT NULL IDENTITY,
    [VehiculoId] int NOT NULL,
    [TipoDocumento] nvarchar(50) NOT NULL,
    [NombreArchivo] nvarchar(255) NOT NULL,
    [RutaArchivo] nvarchar(500) NOT NULL,
    [FechaSubida] datetime2 NOT NULL,
    [FechaVencimiento] datetime2 NULL,
    [UsuarioSubidaId] int NULL,
    [Descripcion] nvarchar(max) NULL,
    [TamañoBytes] bigint NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_DocumentosVehiculo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DocumentosVehiculo_Usuarios_UsuarioSubidaId] FOREIGN KEY ([UsuarioSubidaId]) REFERENCES [Usuarios] ([Id]),
    CONSTRAINT [FK_DocumentosVehiculo_Vehiculos_VehiculoId] FOREIGN KEY ([VehiculoId]) REFERENCES [Vehiculos] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [IngresosTaller] (
    [Id] int NOT NULL IDENTITY,
    [VehiculoId] int NOT NULL,
    [ChoferId] int NOT NULL,
    [FechaProgramada] datetime2 NOT NULL,
    [FechaIngresoReal] datetime2 NULL,
    [FechaSalidaEstimada] datetime2 NULL,
    [FechaSalidaReal] datetime2 NULL,
    [MotivoIngreso] nvarchar(max) NOT NULL,
    [Estado] nvarchar(max) NOT NULL,
    [DescripcionProblema] nvarchar(max) NULL,
    [ObservacionesChofer] nvarchar(max) NULL,
    [SupervisorId] int NULL,
    [MecanicoAsignadoId] int NULL,
    [RequiereRepuestos] bit NOT NULL,
    [KilometrajeIngreso] int NULL,
    CONSTRAINT [PK_IngresosTaller] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_IngresosTaller_Usuarios_ChoferId] FOREIGN KEY ([ChoferId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IngresosTaller_Usuarios_MecanicoAsignadoId] FOREIGN KEY ([MecanicoAsignadoId]) REFERENCES [Usuarios] ([Id]),
    CONSTRAINT [FK_IngresosTaller_Usuarios_SupervisorId] FOREIGN KEY ([SupervisorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IngresosTaller_Vehiculos_VehiculoId] FOREIGN KEY ([VehiculoId]) REFERENCES [Vehiculos] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Documentos] (
    [Id] int NOT NULL IDENTITY,
    [IngresoTallerId] int NOT NULL,
    [TipoDocumento] nvarchar(max) NOT NULL,
    [NombreArchivo] nvarchar(max) NOT NULL,
    [RutaArchivo] nvarchar(max) NOT NULL,
    [FechaSubida] datetime2 NOT NULL,
    [UsuarioSubidaId] int NULL,
    [Descripcion] nvarchar(max) NULL,
    [TamañoBytes] bigint NOT NULL,
    CONSTRAINT [PK_Documentos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documentos_IngresosTaller_IngresoTallerId] FOREIGN KEY ([IngresoTallerId]) REFERENCES [IngresosTaller] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Documentos_Usuarios_UsuarioSubidaId] FOREIGN KEY ([UsuarioSubidaId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Pausas] (
    [Id] int NOT NULL IDENTITY,
    [IngresoTallerId] int NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaFin] datetime2 NULL,
    [Motivo] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    [UsuarioRegistroId] int NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Pausas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Pausas_IngresosTaller_IngresoTallerId] FOREIGN KEY ([IngresoTallerId]) REFERENCES [IngresosTaller] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pausas_Usuarios_UsuarioRegistroId] FOREIGN KEY ([UsuarioRegistroId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [TareasTaller] (
    [Id] int NOT NULL IDENTITY,
    [IngresoTallerId] int NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    [MecanicoAsignadoId] int NULL,
    [FechaAsignacion] datetime2 NOT NULL,
    [FechaInicio] datetime2 NULL,
    [FechaFinalizacion] datetime2 NULL,
    [Estado] nvarchar(max) NOT NULL,
    [Observaciones] nvarchar(max) NULL,
    [TiempoEstimadoHoras] int NOT NULL,
    [TiempoRealHoras] int NULL,
    [Prioridad] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TareasTaller] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TareasTaller_IngresosTaller_IngresoTallerId] FOREIGN KEY ([IngresoTallerId]) REFERENCES [IngresosTaller] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TareasTaller_Usuarios_MecanicoAsignadoId] FOREIGN KEY ([MecanicoAsignadoId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Repuestos] (
    [Id] int NOT NULL IDENTITY,
    [TareaTallerId] int NOT NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [CodigoRepuesto] nvarchar(max) NOT NULL,
    [Cantidad] int NOT NULL,
    [Proveedor] nvarchar(max) NULL,
    [FechaSolicitud] datetime2 NULL,
    [FechaRecepcion] datetime2 NULL,
    [Estado] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Repuestos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Repuestos_TareasTaller_TareaTallerId] FOREIGN KEY ([TareaTallerId]) REFERENCES [TareasTaller] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AsignacionesVehiculo_AsignadoPorId] ON [AsignacionesVehiculo] ([AsignadoPorId]);
GO


CREATE INDEX [IX_AsignacionesVehiculo_ChoferId_Activa] ON [AsignacionesVehiculo] ([ChoferId], [Activa]);
GO


CREATE INDEX [IX_AsignacionesVehiculo_VehiculoId_Activa] ON [AsignacionesVehiculo] ([VehiculoId], [Activa]);
GO


CREATE INDEX [IX_Documentos_IngresoTallerId] ON [Documentos] ([IngresoTallerId]);
GO


CREATE INDEX [IX_Documentos_UsuarioSubidaId] ON [Documentos] ([UsuarioSubidaId]);
GO


CREATE INDEX [IX_DocumentosVehiculo_UsuarioSubidaId] ON [DocumentosVehiculo] ([UsuarioSubidaId]);
GO


CREATE INDEX [IX_DocumentosVehiculo_VehiculoId] ON [DocumentosVehiculo] ([VehiculoId]);
GO


CREATE INDEX [IX_IngresosTaller_ChoferId] ON [IngresosTaller] ([ChoferId]);
GO


CREATE INDEX [IX_IngresosTaller_MecanicoAsignadoId] ON [IngresosTaller] ([MecanicoAsignadoId]);
GO


CREATE INDEX [IX_IngresosTaller_SupervisorId] ON [IngresosTaller] ([SupervisorId]);
GO


CREATE INDEX [IX_IngresosTaller_VehiculoId] ON [IngresosTaller] ([VehiculoId]);
GO


CREATE INDEX [IX_Notificaciones_UsuarioId] ON [Notificaciones] ([UsuarioId]);
GO


CREATE INDEX [IX_Notificaciones_UsuarioId_Leida] ON [Notificaciones] ([UsuarioId], [Leida]);
GO


CREATE INDEX [IX_Pausas_IngresoTallerId] ON [Pausas] ([IngresoTallerId]);
GO


CREATE INDEX [IX_Pausas_UsuarioRegistroId] ON [Pausas] ([UsuarioRegistroId]);
GO


CREATE INDEX [IX_Repuestos_TareaTallerId] ON [Repuestos] ([TareaTallerId]);
GO


CREATE INDEX [IX_TareasTaller_IngresoTallerId] ON [TareasTaller] ([IngresoTallerId]);
GO


CREATE INDEX [IX_TareasTaller_MecanicoAsignadoId] ON [TareasTaller] ([MecanicoAsignadoId]);
GO


CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
GO


CREATE UNIQUE INDEX [IX_Usuarios_Rut] ON [Usuarios] ([Rut]);
GO


CREATE UNIQUE INDEX [IX_Vehiculos_Patente] ON [Vehiculos] ([Patente]);
GO


