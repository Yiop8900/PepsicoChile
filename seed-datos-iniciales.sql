-- =================================================================
-- SCRIPT DE DATOS INICIALES - PEPSICO CHILE
-- Sistema de Gestión de Taller
-- =================================================================
-- Password para TODOS los usuarios: 123456
-- Hash SHA256: jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
-- =================================================================

USE PepsicoChileDB;
GO

PRINT '=================================================================';
PRINT 'INICIANDO INSERCIÓN DE DATOS...';
PRINT '=================================================================';

-- =================================================================
-- 1. USUARIOS
-- =================================================================
PRINT '';
PRINT '1. Insertando Usuarios...';

INSERT INTO Usuarios (Nombre, Apellido, Email, Telefono, Rol, Rut, Password, Activo) VALUES
-- Administración
('Administrador', 'Sistema', 'admin@pepsico.cl', '+56912345678', 'Administrador', '11.111.111-1', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),

-- Supervisores y Jefes
('María', 'López', 'supervisor@pepsico.cl', '+56912345679', 'Supervisor', '12.345.678-9', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Pedro', 'Morales', 'jefetaller@pepsico.cl', '+56912345680', 'JefeTaller', '13.456.789-0', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),

-- Coordinadores
('Ana', 'Torres', 'coordinador@pepsico.cl', '+56912345681', 'CoordinadorZona', '14.567.890-1', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),

-- Mecánicos
('Carlos', 'Rojas', 'carlos.rojas@pepsico.cl', '+56912345682', 'Mecanico', '15.678.901-2', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Luis', 'Muñoz', 'luis.munoz@pepsico.cl', '+56912345683', 'Mecanico', '16.789.012-3', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Roberto', 'Silva', 'roberto.silva@pepsico.cl', '+56912345684', 'Mecanico', '17.890.123-4', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Diego', 'Vargas', 'diego.vargas@pepsico.cl', '+56912345685', 'Mecanico', '18.901.234-5', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),

-- Choferes
('Juan', 'Pérez', 'juan.perez@pepsico.cl', '+56912345686', 'Chofer', '19.012.345-6', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Pedro', 'González', 'pedro.gonzalez@pepsico.cl', '+56912345687', 'Chofer', '20.123.456-7', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Jorge', 'Ramírez', 'jorge.ramirez@pepsico.cl', '+56912345688', 'Chofer', '21.234.567-8', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Andrés', 'Castro', 'andres.castro@pepsico.cl', '+56912345689', 'Chofer', '22.345.678-9', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),

-- Personal de Apoyo
('María', 'Fernández', 'recepcionista@pepsico.cl', '+56912345690', 'Recepcionista', '23.456.789-0', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Luis', 'Soto', 'guardia@pepsico.cl', '+56912345691', 'GuardiaAcceso', '24.567.890-1', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Carmen', 'Bravo', 'repuestos@pepsico.cl', '+56912345692', 'AsistenteRepuestos', '25.678.901-2', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1),
('Ricardo', 'Vega', 'llaves@pepsico.cl', '+56912345693', 'EncargadoLlaves', '26.789.012-3', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 1);

PRINT '   ? ' + CAST(@@ROWCOUNT AS VARCHAR) + ' usuarios insertados';

-- =================================================================
-- 2. VEHÍCULOS
-- =================================================================
PRINT '';
PRINT '2. Insertando Vehículos...';

INSERT INTO Vehiculos (Patente, Marca, Modelo, Año, TipoVehiculo, NumeroFlota, Estado, KilometrajeActual, Motor, Chasis, Color, Combustible, CapacidadCarga, FechaCreacion, FechaActualizacion) VALUES
-- Camiones de reparto
('ABCD-12', 'Volvo', 'FH16', 2020, 'Camión', 'FL-001', 'Disponible', 45000, 'D13K540', 'WV1ZZZ3BZLE000001', 'Blanco', 'Diesel', 18.5, GETDATE(), GETDATE()),
('EFGH-34', 'Mercedes-Benz', 'Actros', 2019, 'Camión', 'FL-002', 'Disponible', 52000, 'OM471', 'WDB9630081L000001', 'Azul', 'Diesel', 20.0, GETDATE(), GETDATE()),
('IJKL-56', 'Scania', 'R450', 2021, 'Camión', 'FL-003', 'Disponible', 38000, 'DC13', 'XLSSC4X40KE000001', 'Rojo', 'Diesel', 19.0, GETDATE(), GETDATE()),
('MNOP-78', 'Volvo', 'FM12', 2018, 'Camión', 'FL-004', 'Disponible', 68000, 'D11A', 'WV1ZZZ3BZKE000001', 'Blanco', 'Diesel', 17.5, GETDATE(), GETDATE()),
('QRST-90', 'Freightliner', 'Cascadia', 2020, 'Camión', 'FL-005', 'Disponible', 41000, 'DD15', '1FUJA6CV3LLB00001', 'Gris', 'Diesel', 21.0, GETDATE(), GETDATE()),

-- Camionetas
('UVWX-11', 'Ford', 'Ranger', 2022, 'Camioneta', 'CM-001', 'Disponible', 15000, '2.0L Bi-Turbo', '8AFBR22E8N0000001', 'Negro', 'Diesel', 1.2, GETDATE(), GETDATE()),
('YZAB-22', 'Toyota', 'Hilux', 2021, 'Camioneta', 'CM-002', 'Disponible', 22000, '2.8L D-4D', 'MR0FZ22G5M0000001', 'Plateado', 'Diesel', 1.0, GETDATE(), GETDATE()),
('CDEF-33', 'Chevrolet', 'Colorado', 2020, 'Camioneta', 'CM-003', 'Disponible', 28000, '2.8L Duramax', '3GCPYEED5LG000001', 'Azul', 'Diesel', 1.1, GETDATE(), GETDATE()),

-- Vehículos de servicio
('GHIJ-44', 'Nissan', 'NP300', 2019, 'Van', 'SV-001', 'Disponible', 35000, '2.5L', 'JN1TESY50Z0000001', 'Blanco', 'Diesel', 0.8, GETDATE(), GETDATE()),
('KLTF-35', 'Mercedes-Benz', 'Sprinter', 2021, 'Van', 'SV-002', 'Disponible', 18000, '2.1L CDI', 'WDB9066661R000001', 'Gris', 'Diesel', 1.5, GETDATE(), GETDATE()),

-- Vehículos de emergencia
('MNOP-55', 'Ford', 'Transit', 2022, 'Van', 'EM-001', 'Disponible', 12000, '2.0L EcoBlue', 'WF0AXXTTGANA00001', 'Rojo', 'Diesel', 1.3, GETDATE(), GETDATE()),
('QRST-66', 'Iveco', 'Daily', 2020, 'Van', 'EM-002', 'Disponible', 25000, '3.0L F1C', 'ZCFC35A3005000001', 'Amarillo', 'Diesel', 1.4, GETDATE(), GETDATE());

PRINT '   ? ' + CAST(@@ROWCOUNT AS VARCHAR) + ' vehículos insertados';

-- =================================================================
-- RESUMEN
-- =================================================================
PRINT '';
PRINT '=================================================================';
PRINT 'RESUMEN DE INSERCIÓN:';
PRINT '=================================================================';
PRINT 'Usuarios: ' + CAST((SELECT COUNT(*) FROM Usuarios) AS VARCHAR);
PRINT 'Vehículos: ' + CAST((SELECT COUNT(*) FROM Vehiculos) AS VARCHAR);
PRINT '';
PRINT '? DATOS INICIALES INSERTADOS CORRECTAMENTE';
PRINT '';
PRINT 'CREDENCIALES DE ACCESO:';
PRINT '------------------------';
PRINT 'Email: admin@pepsico.cl';
PRINT 'Password: 123456';
PRINT '';
PRINT 'Todos los usuarios tienen password: 123456';
PRINT '=================================================================';

GO

-- Mostrar usuarios creados
SELECT 
    Id,
    Nombre + ' ' + Apellido AS [Nombre Completo],
    Email,
    Rol,
    Telefono,
    CASE WHEN Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
FROM Usuarios
ORDER BY Rol, Nombre;

GO
