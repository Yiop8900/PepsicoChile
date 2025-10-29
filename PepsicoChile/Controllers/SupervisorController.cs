using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;

namespace PepsicoChile.Controllers
{
    public class SupervisorController : Controller
    {
  public IActionResult Index()
        {
         return View();
        }

        public IActionResult Agenda()
        {
            var ingresos = ObtenerIngresosProgramados();
 return View(ingresos);
   }

        public IActionResult ProgramarIngreso()
        {
    ViewBag.Vehiculos = ObtenerVehiculos();
            ViewBag.Choferes = ObtenerChoferes();
       return View();
        }

        [HttpPost]
    public IActionResult ProgramarIngreso(IngresoTaller model)
        {
      if (ModelState.IsValid)
            {
     TempData["Mensaje"] = "Ingreso programado exitosamente";
      return RedirectToAction("Agenda");
            }
            ViewBag.Vehiculos = ObtenerVehiculos();
 ViewBag.Choferes = ObtenerChoferes();
            return View(model);
        }

        public IActionResult GestionarIngreso(int id)
        {
  var model = new GestionIngresoViewModel
      {
                Ingreso = ObtenerIngresoPorId(id),
       Tareas = ObtenerTareasPorIngreso(id),
      Pausas = ObtenerPausasPorIngreso(id),
            Documentos = ObtenerDocumentosPorIngreso(id),
         MecanicosDisponibles = ObtenerMecanicos()
            };
        return View(model);
 }

      public IActionResult AsignarTarea(int ingresoId)
        {
          ViewBag.IngresoId = ingresoId;
       ViewBag.Mecanicos = ObtenerMecanicos();
      return View();
        }

 [HttpPost]
        public IActionResult AsignarTarea(TareaTaller model)
        {
  if (ModelState.IsValid)
            {
         TempData["Mensaje"] = "Tarea asignada exitosamente";
       return RedirectToAction("GestionarIngreso", new { id = model.IngresoTallerId });
            }
   ViewBag.Mecanicos = ObtenerMecanicos();
   return View(model);
      }

   public IActionResult MonitoreoGeneral()
        {
   var ingresos = ObtenerTodosLosIngresos();
      return View(ingresos);
        }

        public IActionResult RegistrarPausa(int ingresoId)
        {
      ViewBag.IngresoId = ingresoId;
         return View();
        }

        [HttpPost]
        public IActionResult RegistrarPausa(Pausa model)
        {
     if (ModelState.IsValid)
 {
         TempData["Mensaje"] = "Pausa registrada exitosamente";
         return RedirectToAction("GestionarIngreso", new { id = model.IngresoTallerId });
      }
  return View(model);
        }

        // Métodos auxiliares
        private List<IngresoTaller> ObtenerIngresosProgramados()
        {
   return new List<IngresoTaller>
            {
       new IngresoTaller
  {
       Id = 1,
        Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
       Chofer = new Usuario { Nombre = "Juan", Apellido = "Pérez" },
         FechaProgramada = DateTime.Now.AddDays(1),
        Estado = "Programado",
       MotivoIngreso = "Mantenimiento Preventivo"
        },
             new IngresoTaller
  {
            Id = 2,
   Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes", Modelo = "Actros" },
           Chofer = new Usuario { Nombre = "Pedro", Apellido = "González" },
  FechaProgramada = DateTime.Now.AddDays(2),
   Estado = "Programado",
                MotivoIngreso = "Reparación"
    }
    };
        }

   private List<IngresoTaller> ObtenerTodosLosIngresos()
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
    MotivoIngreso = "Mantenimiento"
        },
          new IngresoTaller
                {
 Id = 2,
    Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes" },
         FechaProgramada = DateTime.Now,
          Estado = "Pausado",
  MotivoIngreso = "Reparación"
       },
        new IngresoTaller
   {
          Id = 3,
          Vehiculo = new Vehiculo { Patente = "IJKL-56", Marca = "Scania" },
    FechaProgramada = DateTime.Now.AddDays(1),
        Estado = "Programado",
        MotivoIngreso = "Inspección"
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
       Supervisor = new Usuario { Nombre = "María", Apellido = "López" },
      FechaProgramada = DateTime.Now,
         FechaIngresoReal = DateTime.Now,
      Estado = "En Proceso",
              MotivoIngreso = "Mantenimiento Preventivo",
      DescripcionProblema = "Mantenimiento programado de 50,000 km"
     };
        }

     private List<TareaTaller> ObtenerTareasPorIngreso(int ingresoId)
        {
            return new List<TareaTaller>
       {
   new TareaTaller
    {
    Id = 1,
   Descripcion = "Cambio de aceite y filtros",
            MecanicoAsignado = new Usuario { Nombre = "Carlos", Apellido = "Rojas" },
 Estado = "Completada",
    Prioridad = "Media"
          },
              new TareaTaller
    {
            Id = 2,
  Descripcion = "Revisión sistema de frenos",
    MecanicoAsignado = new Usuario { Nombre = "Luis", Apellido = "Muñoz" },
        Estado = "En Proceso",
     Prioridad = "Alta"
   }
     };
        }

        private List<Pausa> ObtenerPausasPorIngreso(int ingresoId)
      {
         return new List<Pausa>
            {
     new Pausa
       {
      Id = 1,
               FechaInicio = DateTime.Now.AddHours(-2),
     FechaFin = DateTime.Now.AddHours(-1),
      Motivo = "Espera de repuestos",
      Activa = false
            }
     };
        }

        private List<Documento> ObtenerDocumentosPorIngreso(int ingresoId)
  {
     return new List<Documento>
          {
        new Documento
      {
         Id = 1,
TipoDocumento = "Foto",
      NombreArchivo = "frenos_antes.jpg",
              FechaSubida = DateTime.Now.AddHours(-1)
     }
            };
  }

        private List<Vehiculo> ObtenerVehiculos()
        {
     return new List<Vehiculo>
     {
             new Vehiculo { Id = 1, Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
            new Vehiculo { Id = 2, Patente = "EFGH-34", Marca = "Mercedes", Modelo = "Actros" },
     new Vehiculo { Id = 3, Patente = "IJKL-56", Marca = "Scania", Modelo = "R450" }
        };
}

        private List<Usuario> ObtenerChoferes()
 {
return new List<Usuario>
     {
       new Usuario { Id = 1, Nombre = "Juan", Apellido = "Pérez", Rol = "Chofer" },
       new Usuario { Id = 2, Nombre = "Pedro", Apellido = "González", Rol = "Chofer" },
        new Usuario { Id = 3, Nombre = "Diego", Apellido = "Silva", Rol = "Chofer" }
            };
      }

    private List<Usuario> ObtenerMecanicos()
   {
            return new List<Usuario>
          {
 new Usuario { Id = 1, Nombre = "Carlos", Apellido = "Rojas", Rol = "Mecanico" },
                new Usuario { Id = 2, Nombre = "Luis", Apellido = "Muñoz", Rol = "Mecanico" },
   new Usuario { Id = 3, Nombre = "Jorge", Apellido = "Vargas", Rol = "Mecanico" }
        };
        }
    }
}
