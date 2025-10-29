# PepsiCo Chile - Sistema de Gestión de Taller ??

Sistema web para gestionar el ingreso y mantenimiento de vehículos de la flota de PepsiCo Chile.

## ?? Características

- ? **Sistema de Login** con autenticación y sesiones
- ? **3 Roles de Usuario**: Chofer, Supervisor y Mecánico
- ? **Dashboard** personalizado según rol
- ? **Gestión de Ingresos** de vehículos al taller
- ? **Asignación de Tareas** a mecánicos
- ? **Control de Pausas** en los procesos
- ? **Subida de Documentos** y fotografías
- ? **Reportes** de tiempos, productividad y repuestos
- ? **Historial de Vehículos** y trazabilidad completa

## ?? Tecnologías Utilizadas

- **Backend**: ASP.NET Core 8.0 MVC
- **Base de Datos**: SQL Server Express
- **ORM**: Entity Framework Core 9.0
- **Frontend**: Bootstrap 5, Bootstrap Icons
- **Autenticación**: Sesiones + Hash SHA256

## ?? Requisitos Previos

- .NET 8.0 SDK
- SQL Server Express (instancia `TICA\SQLEXPRESS`)
- Visual Studio 2022 o VS Code

## ?? Instalación y Configuración

### 1. Clonar o abrir el proyecto

```bash
cd C:\Users\insec\source\repos\PepsicoChile\PepsicoChile
```

### 2. Restaurar paquetes NuGet

```bash
dotnet restore
```

### 3. Configurar la Base de Datos

La cadena de conexión ya está configurada en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TICA\\\\SQLEXPRESS;Database=pepsico_taller;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

### 4. Ejecutar el Script SQL

El script `Database/CreateTables.sql` ya fue ejecutado y creó:
- ? Base de datos `pepsico_taller`
- ? Todas las tablas necesarias
- ? 5 usuarios de prueba

### 5. Ejecutar la Aplicación

```bash
dotnet run
```

O presiona **F5** en Visual Studio.

La aplicación estará disponible en: `https://localhost:XXXX`

## ?? Usuarios de Prueba

Todos los usuarios tienen la contraseña: **123456**

| Email | Rol | Descripción |
|-------|-----|-------------|
| `supervisor@pepsico.cl` | Supervisor | Gestiona ingresos y asigna tareas |
| `juan.perez@pepsico.cl` | Chofer | Registra llegadas de vehículos |
| `pedro.gonzalez@pepsico.cl` | Chofer | Registra llegadas de vehículos |
| `carlos.rojas@pepsico.cl` | Mecánico | Ejecuta tareas de taller |
| `luis.munoz@pepsico.cl` | Mecánico | Ejecuta tareas de taller |

## ?? Funcionalidades por Rol

### ????? Chofer
- Registrar llegada de vehículos
- Ver mis ingresos históricos
- Consultar estado de vehículos

### ????? Supervisor
- Programar ingresos al taller
- Gestionar agenda de ingresos
- Asignar tareas a mecánicos
- Registrar pausas en procesos
- Ver monitoreo general
- Generar reportes

### ????? Mecánico
- Ver mis tareas asignadas
- Iniciar y finalizar tareas
- Subir documentos y fotografías
- Solicitar repuestos
- Agregar observaciones

## ?? Estructura del Proyecto

```
PepsicoChile/
??? Controllers/           # Controladores MVC
?   ??? AccountController.cs
?   ??? HomeController.cs
?   ??? ChoferController.cs
?   ??? SupervisorController.cs
?   ??? MecanicoController.cs
?   ??? VehiculosController.cs
?   ??? ReportesController.cs
??? Models/  # Modelos de datos
?   ??? Usuario.cs
?   ??? Vehiculo.cs
?   ??? IngresoTaller.cs
?   ??? TareaTaller.cs
?   ??? Pausa.cs
?   ??? Documento.cs
?   ??? Repuesto.cs
?   ??? ViewModels/      # ViewModels
??? Views/               # Vistas Razor
?   ??? Account/
?   ??? Home/
?   ??? Chofer/
?   ??? Supervisor/
?   ??? Mecanico/
?   ??? Vehiculos/
?   ??? Reportes/
??? Data/      # DbContext
?   ??? ApplicationDbContext.cs
??? Filters/ # Filtros de autorización
?   ??? AuthorizeSessionAttribute.cs
??? Database/   # Scripts SQL
    ??? CreateTables.sql
```

## ?? Seguridad

- Las contraseñas se almacenan hasheadas con SHA256
- Sistema de sesiones para mantener autenticación
- Filtros de autorización por rol
- Validación de permisos en cada acción

## ?? Base de Datos

### Tablas Principales

1. **Usuarios** - Información de usuarios y credenciales
2. **Vehiculos** - Flota de vehículos
3. **IngresosTaller** - Registros de ingresos al taller
4. **TareasTaller** - Tareas asignadas
5. **Pausas** - Pausas en los procesos
6. **Documentos** - Archivos y fotografías
7. **Repuestos** - Control de repuestos solicitados

## ?? Interfaz de Usuario

- **Diseño Responsivo** con Bootstrap 5
- **Iconos** de Bootstrap Icons
- **Colores**: Paleta corporativa de PepsiCo (azul #004B8D)
- **Navegación** adaptada según rol del usuario

## ?? Próximas Mejoras

- [ ] Implementación real de base de datos en controladores
- [ ] Integración con sistema ERP
- [ ] Módulo financiero de costos
- [ ] Aplicación móvil nativa
- [ ] Notificaciones push
- [ ] Reportes en PDF
- [ ] Dashboard con gráficos interactivos

## ?? Soporte

Para consultas o problemas, contactar al equipo de desarrollo.

## ?? Licencia

© 2025 PepsiCo Chile. Todos los derechos reservados.
