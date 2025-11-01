using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using System.Text;

namespace PepsicoChile.Controllers
{
    public class SetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SetupController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
{
     var sb = new StringBuilder();
   sb.AppendLine("=== DIAGNOSTICO DE BASE DE DATOS ===\n");

            // 1. Verificar conexión
            sb.AppendLine("1. PRUEBA DE CONEXION:");
            try
    {
                var canConnect = await _context.Database.CanConnectAsync();
        sb.AppendLine($"   ? Conexion: {(canConnect ? "EXITOSA" : "FALLIDA")}");
              
    if (canConnect)
  {
     sb.AppendLine($"   ? Base de datos accesible");
             }
            }
         catch (Exception ex)
            {
     sb.AppendLine($"   ? Error: {ex.Message}");
  }
          sb.AppendLine();

   // 2. Verificar migraciones pendientes
       sb.AppendLine("2. MIGRACIONES:");
    try
         {
       var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
      var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
     
         sb.AppendLine($"   Aplicadas: {appliedMigrations.Count()}");
    sb.AppendLine($"   Pendientes: {pendingMigrations.Count()}");
      
      if (pendingMigrations.Any())
       {
       sb.AppendLine("\n   ?? HAY MIGRACIONES PENDIENTES:");
      foreach (var migration in pendingMigrations)
 {
           sb.AppendLine($"      - {migration}");
   }
   }
         }
   catch (Exception ex)
      {
        sb.AppendLine($"   ? Error: {ex.Message}");
            }
      sb.AppendLine();

       // 3. Verificar tablas
            sb.AppendLine("3. TABLAS:");
  try
            {
        var usuariosCount = await _context.Usuarios.CountAsync();
            sb.AppendLine($"   ? Usuarios: {usuariosCount} registros");
   }
    catch (Exception ex)
   {
        sb.AppendLine($"   ? Tabla Usuarios: {ex.Message}");
    }

  try
            {
   var vehiculosCount = await _context.Vehiculos.CountAsync();
 sb.AppendLine($"   ? Vehiculos: {vehiculosCount} registros");
            }
            catch (Exception ex)
            {
   sb.AppendLine($"   ? Tabla Vehiculos: {ex.Message}");
            }

       try
            {
                var notificacionesCount = await _context.Notificaciones.CountAsync();
                sb.AppendLine($"   ? Notificaciones: {notificacionesCount} registros");
 }
  catch (Exception ex)
{
sb.AppendLine($"   ? Tabla Notificaciones: {ex.Message}");
     }
            sb.AppendLine();

     // 4. Cadena de conexión
        sb.AppendLine("4. CONFIGURACION:");
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
    sb.AppendLine($"   Connection String: {connectionString}");
   sb.AppendLine();

            // 5. Recomendaciones
    sb.AppendLine("5. ACCIONES RECOMENDADAS:");
            sb.AppendLine("   Si hay errores, ejecuta en Package Manager Console:");
     sb.AppendLine(" 1. Update-Database");
            sb.AppendLine("   O si no existe la BD:");
            sb.AppendLine("   1. Add-Migration InitialCreate");
            sb.AppendLine("   2. Update-Database");

            ViewBag.DiagnosticoTexto = sb.ToString();
       return View();
        }

        [HttpPost]
        public async Task<IActionResult> AplicarMigraciones()
  {
     try
            {
await _context.Database.MigrateAsync();
        TempData["Mensaje"] = "? Migraciones aplicadas exitosamente";
      }
        catch (Exception ex)
         {
  TempData["Error"] = $"? Error al aplicar migraciones: {ex.Message}";
 }

 return RedirectToAction("Index");
      }

        [HttpPost]
        public async Task<IActionResult> CrearUsuarioDemo()
        {
        try
            {
          var existeAdmin = await _context.Usuarios.AnyAsync(u => u.Email == "admin@pepsico.cl");
      
  if (!existeAdmin)
         {
            var usuario = new Models.Usuario
  {
  Nombre = "Administrador",
            Apellido = "Sistema",
             Email = "admin@pepsico.cl",
     Telefono = "123456789",
      Rut = "11111111-1",
            Rol = "Administrador",
Password = HashPassword("123456"),
         Activo = true
        };

                 _context.Usuarios.Add(usuario);
         await _context.SaveChangesAsync();

   TempData["Mensaje"] = "? Usuario demo creado: admin@pepsico.cl / 123456";
     }
          else
           {
              TempData["Mensaje"] = "?? El usuario admin ya existe";
        }
            }
            catch (Exception ex)
      {
     TempData["Error"] = $"? Error: {ex.Message}";
            }

     return RedirectToAction("Index");
 }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
 {
     var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
       return Convert.ToBase64String(hashedBytes);
   }
        }
  }
}
