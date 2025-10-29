using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;

namespace PepsicoChile.Controllers
{
    public class VehiculosController : Controller
    {
  public IActionResult Index()
        {
        var vehiculos = ObtenerTodosLosVehiculos();
     return View(vehiculos);
 }

  public IActionResult Detalle(int id)
        {
     var vehiculo = ObtenerVehiculoPorId(id);
      return View(vehiculo);
        }

   public IActionResult Historial(int id)
        {
   var ingresos = ObtenerHistorialVehiculo(id);
    ViewBag.Vehiculo = ObtenerVehiculoPorId(id);
            return View(ingresos);
 }

      public IActionResult DocumentosVehiculo(int id)
      {
            var documentos = ObtenerDocumentosVehiculo(id);
         ViewBag.Vehiculo = ObtenerVehiculoPorId(id);
   return View(documentos);
        }

     // Métodos auxiliares
        private List<Vehiculo> ObtenerTodosLosVehiculos()
        {
   return new List<Vehiculo>
            {
             new Vehiculo 
     { 
         Id = 1, 
           Patente = "ABCD-12", 
    Marca = "Volvo", 
          Modelo = "FH16",
       Año = 2020,
  TipoVehiculo = "Camión",
      Estado = "En Taller",
          KilometrajeActual = 125000
        },
                new Vehiculo 
{ 
  Id = 2, 
  Patente = "EFGH-34", 
      Marca = "Mercedes", 
          Modelo = "Actros",
            Año = 2019,
    TipoVehiculo = "Camión",
           Estado = "En Ruta",
          KilometrajeActual = 98000
      },
      new Vehiculo 
          { 
        Id = 3, 
           Patente = "IJKL-56", 
                Marca = "Scania", 
      Modelo = "R450",
   Año = 2021,
            TipoVehiculo = "Camión",
    Estado = "Disponible",
        KilometrajeActual = 75000
    },
        new Vehiculo 
                { 
        Id = 4, 
      Patente = "MNOP-78", 
           Marca = "Toyota", 
      Modelo = "Hilux",
     Año = 2022,
         TipoVehiculo = "Camioneta",
          Estado = "Disponible",
           KilometrajeActual = 45000
          }
  };
        }

      private Vehiculo ObtenerVehiculoPorId(int id)
        {
    return new Vehiculo 
            { 
                Id = id, 
        Patente = "ABCD-12", 
       Marca = "Volvo", 
     Modelo = "FH16",
   Año = 2020,
         TipoVehiculo = "Camión",
      Estado = "En Taller",
      KilometrajeActual = 125000
            };
    }

      private List<IngresoTaller> ObtenerHistorialVehiculo(int vehiculoId)
        {
       return new List<IngresoTaller>
    {
   new IngresoTaller
    {
     Id = 1,
       FechaProgramada = DateTime.Now.AddDays(-30),
         FechaIngresoReal = DateTime.Now.AddDays(-30),
     FechaSalidaReal = DateTime.Now.AddDays(-28),
         Estado = "Completado",
           MotivoIngreso = "Mantenimiento Preventivo"
    },
 new IngresoTaller
  {
   Id = 2,
          FechaProgramada = DateTime.Now.AddDays(-60),
           FechaIngresoReal = DateTime.Now.AddDays(-60),
          FechaSalidaReal = DateTime.Now.AddDays(-59),
   Estado = "Completado",
       MotivoIngreso = "Cambio de neumáticos"
           },
         new IngresoTaller
            {
             Id = 3,
           FechaProgramada = DateTime.Now,
             FechaIngresoReal = DateTime.Now,
          Estado = "En Proceso",
        MotivoIngreso = "Reparación sistema eléctrico"
     }
       };
        }

        private List<Documento> ObtenerDocumentosVehiculo(int vehiculoId)
    {
       return new List<Documento>
      {
                new Documento
     {
        Id = 1,
      TipoDocumento = "Informe",
          NombreArchivo = "mantenimiento_preventivo_50k.pdf",
          FechaSubida = DateTime.Now.AddDays(-30)
   },
  new Documento
 {
 Id = 2,
      TipoDocumento = "Foto",
 NombreArchivo = "neumaticos_nuevos.jpg",
             FechaSubida = DateTime.Now.AddDays(-60)
       }
    };
        }
    }
}
