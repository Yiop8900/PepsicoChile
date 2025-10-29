using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    public class HomeController : Controller
    {
        public IActionResult Index()
      {
     // Simulación de usuario logueado - En producción vendría de la sesión/auth
var usuarioActual = ObtenerUsuarioActual();
     
       var model = new DashboardViewModel
          {
 NombreUsuario = usuarioActual.Nombre + " " + usuarioActual.Apellido,
 RolUsuario = usuarioActual.Rol,
VehiculosEnTaller = 12,
        VehiculosProgramados = 8,
        TareasPendientes = 15,
     TareasEnProceso = 7,
    IngresosRecientes = ObtenerIngresosRecientes(),
     TareasUrgentes = ObtenerTareasUrgentes()
       };

       return View(model);
      }

        public IActionResult CambiarRol(string rol)
    {
       // Simulación de cambio de rol para testing
  HttpContext.Session.SetString("RolActual", rol);
       HttpContext.Session.SetString("UsuarioRol", rol);
    return RedirectToAction("Index");
        }

        public IActionResult Error()
   {
     return View();
        }

     // Métodos auxiliares para datos de ejemplo
private Usuario ObtenerUsuarioActual()
        {
        var rol = HttpContext.Session.GetString("UsuarioRol") ?? "Supervisor";
            var nombre = HttpContext.Session.GetString("UsuarioNombre") ?? "Usuario";
    return new Usuario
  {
     Id = HttpContext.Session.GetInt32("UsuarioId") ?? 1,
        Nombre = nombre.Split(' ')[0],
 Apellido = nombre.Contains(' ') ? nombre.Split(' ')[1] : "",
         Rol = rol,
    Email = HttpContext.Session.GetString("UsuarioEmail") ?? "usuario@pepsico.cl"
    };
        }

     private List<IngresoTaller> ObtenerIngresosRecientes()
        {
        return new List<IngresoTaller>
 {
 new IngresoTaller
    {
         Id = 1,
           Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
            FechaProgramada = DateTime.Now,
           Estado = "En Proceso",
        MotivoIngreso = "Mantenimiento Preventivo"
     },
          new IngresoTaller
    {
        Id = 2,
     Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes", Modelo = "Actros" },
    FechaProgramada = DateTime.Now.AddDays(1),
   Estado = "Programado",
  MotivoIngreso = "Reparación"
       }
        };
        }

        private List<TareaTaller> ObtenerTareasUrgentes()
        {
            return new List<TareaTaller>
   {
     new TareaTaller
       {
Id = 1,
      Descripcion = "Cambio de frenos delanteros",
        Prioridad = "Alta",
         Estado = "En Proceso",
  FechaAsignacion = DateTime.Now.AddHours(-2)
         },
     new TareaTaller
      {
    Id = 2,
              Descripcion = "Revisión sistema eléctrico",
          Prioridad = "Alta",
  Estado = "Pendiente",
FechaAsignacion = DateTime.Now.AddHours(-1)
         }
         };
    }
    }
}
