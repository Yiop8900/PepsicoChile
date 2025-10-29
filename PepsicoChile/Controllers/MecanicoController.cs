using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;

namespace PepsicoChile.Controllers
{
    public class MecanicoController : Controller
    {
     public IActionResult Index()
 {
            return View();
   }

        public IActionResult MisTareas()
{
            var tareas = ObtenerTareasPorMecanico();
        return View(tareas);
        }

  public IActionResult DetalleTarea(int id)
        {
            var tarea = ObtenerTareaPorId(id);
            return View(tarea);
        }

        public IActionResult IniciarTarea(int id)
        {
// Lógica para iniciar tarea
        TempData["Mensaje"] = "Tarea iniciada";
   return RedirectToAction("DetalleTarea", new { id });
        }

        public IActionResult FinalizarTarea(int id)
 {
 var tarea = ObtenerTareaPorId(id);
            return View(tarea);
        }

        [HttpPost]
        public IActionResult FinalizarTarea(TareaTaller model)
     {
            if (ModelState.IsValid)
     {
   TempData["Mensaje"] = "Tarea finalizada exitosamente";
      return RedirectToAction("MisTareas");
   }
   return View(model);
        }

        public IActionResult SubirDocumento(int ingresoId)
    {
            ViewBag.IngresoId = ingresoId;
       return View();
      }

    [HttpPost]
        public IActionResult SubirDocumento(Documento model, IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
          // Simulación de guardado de archivo
       model.NombreArchivo = archivo.FileName;
 model.TamañoBytes = archivo.Length;
       model.FechaSubida = DateTime.Now;
   
       TempData["Mensaje"] = "Documento subido exitosamente";
      return RedirectToAction("VerDocumentos", new { ingresoId = model.IngresoTallerId });
          }
       return View(model);
    }

        public IActionResult VerDocumentos(int ingresoId)
        {
  var documentos = ObtenerDocumentosPorIngreso(ingresoId);
      ViewBag.IngresoId = ingresoId;
    return View(documentos);
        }

        public IActionResult SolicitarRepuesto(int tareaId)
      {
          ViewBag.TareaId = tareaId;
 return View();
    }

   [HttpPost]
        public IActionResult SolicitarRepuesto(Repuesto model)
        {
            if (ModelState.IsValid)
            {
        TempData["Mensaje"] = "Repuesto solicitado exitosamente";
      return RedirectToAction("DetalleTarea", new { id = model.TareaTallerId });
            }
    return View(model);
        }

        public IActionResult RegistrarObservacion(int tareaId)
   {
            ViewBag.TareaId = tareaId;
        return View();
 }

        [HttpPost]
        public IActionResult RegistrarObservacion(int tareaId, string observacion)
        {
TempData["Mensaje"] = "Observación registrada exitosamente";
       return RedirectToAction("DetalleTarea", new { id = tareaId });
   }

        // Métodos auxiliares
        private List<TareaTaller> ObtenerTareasPorMecanico()
        {
    return new List<TareaTaller>
   {
      new TareaTaller
          {
     Id = 1,
        Descripcion = "Cambio de aceite y filtros",
    IngresoTaller = new IngresoTaller 
               { 
  Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo" }
       },
             Estado = "En Proceso",
     Prioridad = "Media",
 FechaAsignacion = DateTime.Now.AddHours(-2),
              TiempoEstimadoHoras = 3
       },
         new TareaTaller
            {
         Id = 2,
    Descripcion = "Revisión sistema de frenos",
   IngresoTaller = new IngresoTaller 
 { 
            Vehiculo = new Vehiculo { Patente = "EFGH-34", Marca = "Mercedes" }
  },
        Estado = "Pendiente",
      Prioridad = "Alta",
            FechaAsignacion = DateTime.Now,
  TiempoEstimadoHoras = 5
            },
                new TareaTaller
     {
                 Id = 3,
    Descripcion = "Cambio de neumáticos",
        IngresoTaller = new IngresoTaller 
     { 
       Vehiculo = new Vehiculo { Patente = "IJKL-56", Marca = "Scania" }
     },
           Estado = "Completada",
   Prioridad = "Baja",
   FechaAsignacion = DateTime.Now.AddDays(-1),
FechaFinalizacion = DateTime.Now.AddHours(-2),
     TiempoEstimadoHoras = 2,
   TiempoRealHoras = 2
                }
};
      }

        private TareaTaller ObtenerTareaPorId(int id)
    {
      return new TareaTaller
            {
     Id = id,
    Descripcion = "Cambio de aceite y filtros",
    IngresoTaller = new IngresoTaller 
              { 
         Id = 1,
  Vehiculo = new Vehiculo { Patente = "ABCD-12", Marca = "Volvo", Modelo = "FH16" },
      MotivoIngreso = "Mantenimiento Preventivo"
                },
      MecanicoAsignado = new Usuario { Nombre = "Carlos", Apellido = "Rojas" },
      Estado = "En Proceso",
         Prioridad = "Media",
    FechaAsignacion = DateTime.Now.AddHours(-2),
           FechaInicio = DateTime.Now.AddHours(-1),
      TiempoEstimadoHoras = 3,
      Observaciones = "Filtro de aire muy sucio, recomendar limpieza adicional"
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
       NombreArchivo = "motor_antes.jpg",
         FechaSubida = DateTime.Now.AddHours(-2),
          UsuarioSubida = new Usuario { Nombre = "Carlos", Apellido = "Rojas" }
                },
       new Documento
   {
       Id = 2,
     TipoDocumento = "Informe",
   NombreArchivo = "diagnostico_inicial.pdf",
   FechaSubida = DateTime.Now.AddHours(-1),
                    UsuarioSubida = new Usuario { Nombre = "Carlos", Apellido = "Rojas" }
                }
     };
 }
    }
}
