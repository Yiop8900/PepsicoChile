-- Insertar todos los roles de usuarios
USE pepsico_taller;
GO

DECLARE @password NVARCHAR(255) = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=';

-- Mecánico
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'mecanico@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Juan', 'Mecánico', 'mecanico@pepsico.cl', '+56900000001', 'Mecanico', '22.222.222-2', @password, 1);
END

-- Jefe Taller
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'jefetaller@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Pedro', 'Jefe', 'jefetaller@pepsico.cl', '+56900000002', 'JefeTaller', '33.333.333-3', @password, 1);
END

-- Coordinador Zona
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'coordinador@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Ana', 'Coordinadora', 'coordinador@pepsico.cl', '+56900000003', 'CoordinadorZona', '44.444.444-4', @password, 1);
END

-- Guardia Acceso
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'guardia@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Luis', 'Guardia', 'guardia@pepsico.cl', '+56900000004', 'GuardiaAcceso', '55.555.555-5', @password, 1);
END

-- Recepcionista
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'recepcionista@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('María', 'Recepción', 'recepcionista@pepsico.cl', '+56900000005', 'Recepcionista', '66.666.666-6', @password, 1);
END

-- Asistente Repuestos
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'repuestos@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Carlos', 'Repuestos', 'repuestos@pepsico.cl', '+56900000006', 'AsistenteRepuestos', '77.777.777-7', @password, 1);
END

-- Supervisor
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'supervisor@pepsico.cl')
BEGIN
    INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Rosa', 'Supervisora', 'supervisor@pepsico.cl', '+56900000007', 'Supervisor', '88.888.888-8', @password, 1);
END

-- Encargado Llaves
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'llaves@pepsico.cl')
BEGIN
  INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo)
    VALUES ('Diego', 'Llaves', 'llaves@pepsico.cl', '+56900000008', 'EncargadoLlaves', '99.999.999-9', @password, 1);
END

PRINT 'Todos los roles de usuario han sido creados';
PRINT 'Contraseña para todos: 123456';
GO
