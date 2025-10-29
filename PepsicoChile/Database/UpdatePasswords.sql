-- Script para actualizar las contraseñas de los usuarios existentes
-- PepsiCo Chile - Sistema de Gestión de Taller

USE pepsico_taller;
GO

-- Password hash correcto de "123456" usando SHA256
DECLARE @password NVARCHAR(255) = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=';

-- Actualizar todas las contraseñas
UPDATE Usuarios
SET Password = @password
WHERE Email IN (
    'supervisor@pepsico.cl',
    'juan.perez@pepsico.cl',
    'pedro.gonzalez@pepsico.cl',
    'carlos.rojas@pepsico.cl',
    'luis.munoz@pepsico.cl'
);

PRINT '=================================================================';
PRINT 'Contraseñas actualizadas correctamente.';
PRINT 'Todos los usuarios ahora tienen la contraseña: 123456';
PRINT '';
PRINT 'Hash SHA256: jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=';
PRINT '=================================================================';
GO
