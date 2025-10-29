# ?? Fix de Hash de Contraseña - PepsiCo Chile

## ? Problema Resuelto

El hash de la contraseña "123456" estaba incorrecto en la base de datos.

### Hash Incorrecto (anterior):
```
jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
```

### Hash Correcto (actual):
```
jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
```

## ?? Cambios Realizados

### 1. Script SQL Actualizado
- ? `Database/CreateTables.sql` - Hash corregido
- ? `Database/UpdatePasswords.sql` - Script nuevo para actualizar contraseñas

### 2. Contraseñas en BD Actualizadas
```sql
-- Se ejecutó exitosamente:
UPDATE Usuarios SET Password = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI='
-- (5 rows affected)
```

### 3. Controller Mejorado
- ? Mejor manejo de errores en `Login()`
- ? Endpoints de diagnóstico agregados:
  - `/Account/TestHash?password=123456` - Verifica hash
  - `/Account/TestUsers` - Lista usuarios y sus hashes

## ?? Testing

### 1. Detener y Reiniciar la Aplicación
```bash
# Presiona Shift + F5 para detener
# Presiona F5 para iniciar de nuevo
```

### 2. Probar Login
Usa cualquiera de estos usuarios con contraseña **123456**:

| Email | Contraseña | Rol |
|-------|-----------|-----|
| supervisor@pepsico.cl | 123456 | Supervisor |
| juan.perez@pepsico.cl | 123456 | Chofer |
| carlos.rojas@pepsico.cl | 123456 | Mecánico |

### 3. Verificar Hash (Desarrollo)
Abre en el navegador:
```
https://localhost:XXXX/Account/TestHash?password=123456
```

Deberías ver:
```json
{
  "password": "123456",
  "generatedHash": "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=",
  "expectedHash": "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=",
  "match": true,
  "message": "? Hash correcto"
}
```

### 4. Verificar Usuarios en BD (Desarrollo)
```
https://localhost:XXXX/Account/TestUsers
```

## ?? Seguridad

### ?? IMPORTANTE - Antes de Producción:

**ELIMINAR** los siguientes endpoints de diagnóstico en `AccountController.cs`:
- `TestHash()` - Línea ~95
- `TestUsers()` - Línea ~108

Estos endpoints son **SOLO PARA DESARROLLO** y exponen información sensible.

## ?? Verificación del Hash SHA256

### Algoritmo Usado:
```csharp
using (var sha256 = SHA256.Create())
{
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
```

### Verificación en PowerShell:
```powershell
$password = "123456"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hash = $sha256.ComputeHash($bytes)
[Convert]::ToBase64String($hash)
# Output: jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
```

## ? Checklist Post-Fix

- [x] Hash corregido en `CreateTables.sql`
- [x] Script `UpdatePasswords.sql` creado
- [x] Contraseñas actualizadas en BD (5 usuarios)
- [x] Endpoints de diagnóstico agregados
- [x] Mejor manejo de errores en Login
- [ ] Reiniciar aplicación
- [ ] Probar login con todos los usuarios
- [ ] Verificar hash con endpoint `/TestHash`
- [ ] **ANTES DE PRODUCCIÓN**: Eliminar endpoints de test

## ?? Resultado Esperado

Después de estos cambios, deberías poder:
1. ? Iniciar sesión con **123456** como contraseña
2. ? Ver tu nombre y rol en el navbar
3. ? Acceder al dashboard según tu rol
4. ? Cerrar sesión correctamente

## ?? Si Continúa el Problema

1. Verifica la cadena de conexión en `appsettings.json`
2. Confirma que el script SQL se ejecutó: `(5 rows affected)`
3. Usa el endpoint `/Account/TestUsers` para verificar los hashes en BD
4. Revisa los logs de la aplicación para errores específicos

---

**Última actualización:** $(Get-Date)
**Hash verificado con:** SHA256 en .NET 8
