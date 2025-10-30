using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("JefeTaller")]
    public class SupervisorController : Controller
    {
   public IActionResult Index()
        {
       return RedirectToAction("Agenda");
      }

        public IActionResult Agenda()
      {
   var ingresosHoy = ObtenerIngresosProgramados(DateTime.Today);
       return View(ingresosHoy);
        }

    [HttpGet]
  public IActionResult ProgramarIngreso()
        {
     var model = new ProgramarIngresoViewModel
{
      VehiculosDisponibles = ObtenerVehiculosFlota(),
       ChofersDisponibles = ObtenerChofersActivos(),
   FechaProgramada = DateTime.Now.AddDays(1)
  };
  return View(model);
}

        [HttpPost]
public IActionResult ProgramarIngreso(ProgramarIngresoViewModel model)
 {
if (ModelState.IsValid)
{
   // Guardar en base de datos
    TempData["Mensaje"] = "Ingreso programado exitosamente";
   return RedirectToAction("Agenda");
  }

   model.VehiculosDisponibles = ObtenerVehiculosFlota();
      model.ChofersDisponibles = ObtenerChofersActivos();
  return View(model);
  }

  public IActionResult MonitoreoGeneral()
{
     var ingresos = ObtenerTodosIngresos();
    return View(ingresos);
        }

public IActionResult GestionarIngreso(int id)
  {
     var ingreso = ObtenerIngresoPorId(id);
   return View(ingreso);
  }

      [HttpPost]
    public IActionResult AsignarTarea(int ingresoId, string descripcion, int mecanicoId)
   {
   // Lógica para asignar tarea
            TempData["Mensaje"] = "Tarea asignada correctamente";
 return RedirectToAction("GestionarIngreso", new { id = ingresoId });
}

        public IActionResult RegistrarPausa(int ingresoId)
        {
       ViewBag.IngresoId = ingresoId;
            return View();
     }

        [HttpPost]
     public IActionResult RegistrarPausa(int ingresoId, string motivo, string descripcion)
   {
  // Guardar pausa
TempData["Mensaje"] = "Pausa registrada exitosamente";
            return RedirectToAction("GestionarIngreso", new { id = ingresoId });
        }

      // Métodos auxiliares
  private List<IngresoTaller> ObtenerIngresosProgramados(DateTime fecha)
{
   return new List<IngresoTaller>
      {
           new IngresoTaller
   {
       Id = 1,
    Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
 Chofer = new Usuario { Nombre = "Juan", Apellido = "Pérez" },
   FechaProgramada = fecha.AddHours(9),
  Estado = "Programado",
     MotivoIngreso = "Mantenimiento Preventivo"
      }
   };
   }

        private List<IngresoTaller> ObtenerTodosIngresos()
        {
         return new List<IngresoTaller>
     {
     new IngresoTaller
       {
     Id = 1,
   Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo" },
     FechaProgramada = DateTime.Now,
       Estado = "En Proceso",
       MotivoIngreso = "Mantenimiento"
 },
      new IngresoTaller
     {
         Id = 2,
 Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes" },
    FechaProgramada = DateTime.Now.AddDays(-1),
  Estado = "Completado",
         MotivoIngreso = "Reparación"
      }
        };
  }

        private IngresoTaller ObtenerIngresoPorId(int id)
  {
return new IngresoTaller
       {
    Id = id,
     Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
    Chofer = new Usuario { Nombre = "Juan", Apellido = "Pérez" },
    FechaProgramada = DateTime.Now,
       FechaIngresoReal = DateTime.Now,
  Estado = "En Proceso",
        MotivoIngreso = "Mantenimiento Preventivo",
    ObservacionesChofer = "Ruido extraño en el motor"
  };
        }

        private List<Vehiculo> ObtenerVehiculosFlota()
     {
 return new List<Vehiculo>
  {
            new Vehiculo { Id = 1, Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
    new Vehiculo { Id = 2, Patente = "EFGH-34", Marca = "Mercedes", Modelo = "Actros" }
 };
      }

        private List<Usuario> ObtenerChofersActivos()
 {
   return new List<Usuario>
    {
  new Usuario { Id = 1, Nombre = "Juan", Apellido = "Pérez" },
new Usuario { Id = 2, Nombre = "Pedro", Apellido = "González" }
       };
  }
    }
}
