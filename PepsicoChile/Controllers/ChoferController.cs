using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "Recepcionista", "JefeTaller")]
    public class ChoferController : Controller
    {
        public IActionResult Index()
   {
        return View();
  }

     public IActionResult RegistrarLlegada()
    {
  var model = new RegistroLlegadaViewModel
        {
  VehiculosDisponibles = ObtenerVehiculosFlota(),
     FechaHoraLlegada = DateTime.Now
   };
       return View(model);
    }

[HttpPost]
        public IActionResult RegistrarLlegada(RegistroLlegadaViewModel model)
{
if (ModelState.IsValid)
       {
    // Aquí se guardaría en la base de datos
   TempData["Mensaje"] = "Llegada registrada exitosamente";
  return RedirectToAction("MisIngresos");
     }
   return View(model);
    }

   public IActionResult MisIngresos()
  {
   var ingresos = ObtenerIngresosPorChofer();
       return View(ingresos);
  }

  public IActionResult DetalleIngreso(int id)
  {
      var ingreso = ObtenerIngresoPorId(id);
  return View(ingreso);
      }

      // Métodos auxiliares
     private List<Vehiculo> ObtenerVehiculosFlota()
    {
return new List<Vehiculo>
 {
   new Vehiculo { Id = 1, Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16", TipoVehiculo = "Camión" },
 new Vehiculo { Id = 2, Patente = "EFGH-34", Marca = "Mercedes", Modelo = "Actros", TipoVehiculo = "Camión" },
 new Vehiculo { Id = 3, Patente = "IJKL-56", Marca = "Scania", Modelo = "R450", TipoVehiculo = "Camión" },
        new Vehiculo { Id = 4, Patente = "MNOP-78", Marca = "Toyota", Modelo = "Hilux", TipoVehiculo = "Camioneta" }
 };
}

  private List<IngresoTaller> ObtenerIngresosPorChofer()
        {
 return new List<IngresoTaller>
    {
   new IngresoTaller
     {
Id = 1,
   Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo" },
 FechaProgramada = DateTime.Now,
FechaIngresoReal = DateTime.Now,
      Estado = "En Proceso",
  MotivoIngreso = "Mantenimiento Preventivo",
    ObservacionesChofer = "Ruido extraño en el motor"
    },
       new IngresoTaller
          {
        Id = 2,
   Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes" },
  FechaProgramada = DateTime.Now.AddDays(-2),
  FechaIngresoReal = DateTime.Now.AddDays(-2),
         FechaSalidaReal = DateTime.Now.AddDays(-1),
  Estado = "Completado",
     MotivoIngreso = "Reparación",
      ObservacionesChofer = "Falla en frenos"
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
ObservacionesChofer = "Ruido extraño en el motor",
     KilometrajeIngreso = 125000
       };
   }
}
}
