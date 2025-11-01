using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;

namespace PepsicoChile.Controllers
{
    public class DiagnosticoController : Controller
    {
 private readonly ApplicationDbContext _context;

        public DiagnosticoController(ApplicationDbContext context)
 {
  _context = context;
  }

 public async Task<IActionResult> Index()
  {
     var diagnostico = new
  {
   Timestamp = DateTime.Now,
  Tests = new List<object>()
    };

   var tests = new List<object>();

     // Test 1: Conexión a BD
  try
   {
    await _context.Database.CanConnectAsync();
       tests.Add(new { Test = "Conexión a BD", Status = "? OK", Message = "Conexión exitosa" });
  }
   catch (Exception ex)
   {
    tests.Add(new { Test = "Conexión a BD", Status = "? ERROR", Message = ex.Message });
   }

   // Test 2: Contar usuarios
   try
  {
  var count = await _context.Usuarios.CountAsync();
tests.Add(new { Test = "Contar Usuarios", Status = "? OK", Message = $"Total: {count} usuarios" });
   }
 catch (Exception ex)
   {
  tests.Add(new { Test = "Contar Usuarios", Status = "? ERROR", Message = ex.Message });
  }

   // Test 3: Verificar tabla Notificaciones
   try
   {
   var notifCount = await _context.Notificaciones.CountAsync();
tests.Add(new { Test = "Tabla Notificaciones", Status = "? OK", Message = $"Total: {notifCount} notificaciones" });
  }
  catch (Exception ex)
   {
   tests.Add(new { Test = "Tabla Notificaciones", Status = "?? WARNING", Message = $"Posible migración pendiente: {ex.Message}" });
   }

    // Test 4: Sesión
try
  {
    var userId = HttpContext.Session.GetInt32("UsuarioId");
       var userName = HttpContext.Session.GetString("UsuarioNombre");
    var sessionInfo = userId.HasValue ? $"Usuario ID: {userId}, Nombre: {userName}" : "No hay sesión activa";
 tests.Add(new { Test = "Sesión", Status = "? OK", Message = sessionInfo });
      }
 catch (Exception ex)
  {
tests.Add(new { Test = "Sesión", Status = "? ERROR", Message = ex.Message });
   }

  return Json(new { 
       Timestamp = DateTime.Now,
      DatabaseConnection = _context.Database.GetConnectionString(),
  Tests = tests 
     });
    }
    }
}
