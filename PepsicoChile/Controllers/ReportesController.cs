using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Filters;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Models;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "JefeTaller", "CoordinadorZona", "Supervisor", "AsistenteRepuestos")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TiemposProceso()
        {
            // Obtener ingresos completados con sus datos relacionados
            var ingresosCompletados = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.MecanicoAsignado)
                .Include(i => i.Chofer)
                .Where(i => i.Estado == "Completado" && 
                           i.FechaIngresoReal.HasValue && 
                           i.FechaSalidaReal.HasValue)
                .ToListAsync();

            // Calcular métricas de tiempo
            var tiempos = ingresosCompletados.Select(i => new
            {
                Ingreso = i,
                TiempoTotal = (i.FechaSalidaReal!.Value - i.FechaIngresoReal!.Value).TotalHours,
                DiasTranscurridos = (i.FechaSalidaReal!.Value - i.FechaIngresoReal!.Value).Days
            }).ToList();

            // Obtener pausas de los ingresos completados
            var pausas = await _context.Pausas
                .Where(p => ingresosCompletados.Select(i => i.Id).Contains(p.IngresoTallerId) && 
                           p.FechaFin.HasValue)
                .ToListAsync();

            // Calcular tiempo total de pausas por ingreso
            var tiemposPausasPorIngreso = pausas
                .GroupBy(p => p.IngresoTallerId)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                );

            // Calcular tiempo efectivo (sin pausas)
            var tiemposEfectivos = tiempos.Select(t => new IngresoDetalladoDto
            {
                Ingreso = t.Ingreso,
                TiempoTotal = t.TiempoTotal,
                DiasTranscurridos = t.DiasTranscurridos,
                TiempoPausas = tiemposPausasPorIngreso.ContainsKey(t.Ingreso.Id) 
                    ? tiemposPausasPorIngreso[t.Ingreso.Id] 
                    : 0,
                TiempoEfectivo = t.TiempoTotal - (tiemposPausasPorIngreso.ContainsKey(t.Ingreso.Id) 
                    ? tiemposPausasPorIngreso[t.Ingreso.Id] 
                    : 0)
            }).ToList();

            // Métricas generales
            var viewModel = new TiempoProcesoViewModel
            {
                TotalIngresos = ingresosCompletados.Count,
                
                // Tiempos promedio
                TiempoPromedioTotal = tiemposEfectivos.Any() 
                    ? tiemposEfectivos.Average(t => t.TiempoTotal) 
                    : 0,
                    
                TiempoPromedioEfectivo = tiemposEfectivos.Any() 
                    ? tiemposEfectivos.Average(t => t.TiempoEfectivo) 
                    : 0,
                    
                TiempoPromedioPausas = tiemposEfectivos.Any() 
                    ? tiemposEfectivos.Average(t => t.TiempoPausas) 
                    : 0,

                // Por tipo de mantenimiento
                TiempoPromedioMantenimiento = tiemposEfectivos
                    .Where(t => t.Ingreso.MotivoIngreso == "Mantenimiento")
                    .Any() 
                    ? tiemposEfectivos
                        .Where(t => t.Ingreso.MotivoIngreso == "Mantenimiento")
                        .Average(t => t.TiempoEfectivo) 
                    : 0,
                    
                TiempoPromedioReparacion = tiemposEfectivos
                    .Where(t => t.Ingreso.MotivoIngreso == "Reparación")
                    .Any() 
                    ? tiemposEfectivos
                        .Where(t => t.Ingreso.MotivoIngreso == "Reparación")
                        .Average(t => t.TiempoEfectivo) 
                    : 0,

                // Estadísticas adicionales
                TiempoMinimo = tiemposEfectivos.Any() 
                    ? tiemposEfectivos.Min(t => t.TiempoEfectivo) 
                    : 0,
                    
                TiempoMaximo = tiemposEfectivos.Any() 
                    ? tiemposEfectivos.Max(t => t.TiempoEfectivo) 
                    : 0,

                // Detalles de ingresos
                IngresosDetallados = tiemposEfectivos.OrderByDescending(t => t.TiempoTotal).ToList(),
                
                // Total de pausas
                TotalPausas = pausas.Count,
                TiempoTotalPausas = pausas.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours),

                // Pausas por motivo
                PausasPorMotivo = pausas
                    .GroupBy(p => p.Motivo)
                    .Select(g => new PausaPorMotivoDto
                    {
                        Motivo = g.Key,
                        Cantidad = g.Count(),
                        TiempoTotal = g.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                    })
                    .OrderByDescending(p => p.Cantidad)
                    .ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Productividad()
        {
            // Obtener todas las tareas
            var todasTareas = await _context.TareasTaller
                .Include(t => t.MecanicoAsignado)
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .ToListAsync();

            // Contar por estado
            var tareasCompletadas = todasTareas.Count(t => t.Estado == "Completada");
            var tareasEnProceso = todasTareas.Count(t => t.Estado == "En Proceso");
            var tareasPendientes = todasTareas.Count(t => t.Estado == "Pendiente");

            // Productividad por mecánico
            var productividadMecanicos = await _context.Usuarios
                .Where(u => u.Rol == "Mecanico" && u.Activo)
                .Select(m => new ProductividadMecanicoDto
                {
                    MecanicoId = m.Id,
                    NombreCompleto = m.Nombre + " " + m.Apellido,
                    TareasCompletadas = _context.TareasTaller.Count(t => t.MecanicoAsignadoId == m.Id && t.Estado == "Completada"),
                    TareasEnProceso = _context.TareasTaller.Count(t => t.MecanicoAsignadoId == m.Id && t.Estado == "En Proceso"),
                    TareasPendientes = _context.TareasTaller.Count(t => t.MecanicoAsignadoId == m.Id && t.Estado == "Pendiente"),
                    TareasTotal = _context.TareasTaller.Count(t => t.MecanicoAsignadoId == m.Id)
                })
                .Where(m => m.TareasTotal > 0)
                .ToListAsync();

            // Calcular eficiencia de cada mecánico
            foreach (var mecanico in productividadMecanicos)
            {
                if (mecanico.TareasTotal > 0)
                {
                    mecanico.PorcentajeEficiencia = (double)mecanico.TareasCompletadas / mecanico.TareasTotal * 100;
                }
            }

            // Eficiencia promedio
            var eficienciaPromedio = productividadMecanicos.Any() 
                ? productividadMecanicos.Average(m => m.PorcentajeEficiencia) 
                : 0;

            // Tareas por prioridad
            var tareasPorPrioridad = todasTareas
                .GroupBy(t => t.Prioridad)
                .Select(g => new TareaPorPrioridadDto
                {
                    Prioridad = g.Key,
                    Completadas = g.Count(t => t.Estado == "Completada"),
                    EnProceso = g.Count(t => t.Estado == "En Proceso"),
                    Pendientes = g.Count(t => t.Estado == "Pendiente"),
                    Total = g.Count()
                })
                .OrderBy(t => t.Prioridad == "Alta" ? 1 : t.Prioridad == "Media" ? 2 : 3)
                .ToList();

            // Tareas completadas en los últimos 7 días
            var hace7Dias = DateTime.Now.AddDays(-7);
            var tareasRecientes = todasTareas
                .Where(t => t.Estado == "Completada" && 
                           t.FechaFinalizacion.HasValue && 
                           t.FechaFinalizacion.Value >= hace7Dias)
                .OrderByDescending(t => t.FechaFinalizacion)
                .Take(10)
                .ToList();

            // Tiempo promedio de completación (solo para tareas completadas)
            var tareasConTiempo = todasTareas
                .Where(t => t.Estado == "Completada" && 
                           t.FechaFinalizacion.HasValue)
                .Select(t => (t.FechaFinalizacion!.Value - t.FechaAsignacion).TotalHours)
                .ToList();

            var tiempoPromedioCompletacion = tareasConTiempo.Any() 
                ? tareasConTiempo.Average() 
                : 0;

            var viewModel = new ProductividadViewModel
            {
                // Totales
                TareasCompletadas = tareasCompletadas,
                TareasEnProceso = tareasEnProceso,
                TareasPendientes = tareasPendientes,
                TotalTareas = todasTareas.Count,

                // Eficiencia
                EficienciaPromedio = eficienciaPromedio,
                TiempoPromedioCompletacion = tiempoPromedioCompletacion,

                // Por mecánico
                ProductividadMecanicos = productividadMecanicos.OrderByDescending(m => m.PorcentajeEficiencia).ToList(),

                // Por prioridad
                TareasPorPrioridad = tareasPorPrioridad,

                // Tareas recientes
                TareasRecientes = tareasRecientes,

                // Estadísticas adicionales
                MecanicoMasProductivo = productividadMecanicos.Any() 
                    ? productividadMecanicos.OrderByDescending(m => m.TareasCompletadas).First().NombreCompleto 
                    : "N/A",

                TareasAltaPrioridad = todasTareas.Count(t => t.Prioridad == "Alta"),
                TareasAltaPrioridadCompletadas = todasTareas.Count(t => t.Prioridad == "Alta" && t.Estado == "Completada")
            };

            return View(viewModel);
        }

        [AuthorizeRole("AsistenteRepuestos")]
        public IActionResult Repuestos()
        {
       // Datos de ejemplo para reporte de repuestos
        ViewBag.RepuestosSolicitados = 32;
          ViewBag.RepuestosRecibidos = 28;
       ViewBag.RepuestosPendientes = 4;
     return View();
        }

    public async Task<IActionResult> VehiculosEstado()
    {
        // Obtener todos los vehículos
     var vehiculos = await _context.Vehiculos
          .OrderBy(v => v.Patente)
            .ToListAsync();

   // Estadísticas generales
   var estadisticas = new
      {
    Total = vehiculos.Count,
  Disponibles = vehiculos.Count(v => v.Estado == "Disponible"),
   EnRuta = vehiculos.Count(v => v.Estado == "En Ruta"),
       EnTaller = vehiculos.Count(v => v.Estado == "En Taller"),
      Programados = vehiculos.Count(v => v.Estado == "Programado"),
   PromedioKilometraje = vehiculos.Any() && vehiculos.Any(v => v.KilometrajeActual.HasValue)
   ? (int)vehiculos.Where(v => v.KilometrajeActual.HasValue).Average(v => v.KilometrajeActual.Value) 
    : 0
  };

        ViewBag.Estadisticas = estadisticas;
            
       return View(vehiculos);
 }

   public async Task<IActionResult> HistoricoPausas()
        {
            // Obtener todas las pausas con sus ingresos relacionados
            var todasPausas = await _context.Pausas
                .Include(p => p.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Include(p => p.IngresoTaller)
                    .ThenInclude(i => i!.MecanicoAsignado)
                .Include(p => p.UsuarioRegistro)
                .ToListAsync();

            // Filtrar solo pausas finalizadas para análisis
            var pausasFinalizadas = todasPausas.Where(p => p.FechaFin.HasValue).ToList();

            // Calcular métricas
            var totalPausas = todasPausas.Count;
            var pausasActivas = todasPausas.Count(p => !p.FechaFin.HasValue);
            
            var tiempoTotalHoras = pausasFinalizadas
                .Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours);

            var tiempoPromedioHoras = pausasFinalizadas.Any() 
                ? pausasFinalizadas.Average(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours) 
                : 0;

            // Pausas por motivo
            var pausasPorMotivo = pausasFinalizadas
                .GroupBy(p => p.Motivo)
                .Select(g => new PausaPorMotivoDetalladaDto
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    TiempoTotal = g.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours),
                    TiempoPromedio = g.Average(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours),
                    TiempoMinimo = g.Min(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours),
                    TiempoMaximo = g.Max(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                })
                .OrderByDescending(p => p.TiempoTotal)
                .ToList();

            // Pausas por vehículo (top 10)
            var pausasPorVehiculo = pausasFinalizadas
                .Where(p => p.IngresoTaller?.Vehiculo != null)
                .GroupBy(p => new { 
                    VehiculoId = p.IngresoTaller!.Vehiculo!.Id,
                    Patente = p.IngresoTaller.Vehiculo.Patente,
                    Marca = p.IngresoTaller.Vehiculo.Marca,
                    Modelo = p.IngresoTaller.Vehiculo.Modelo
                })
                .Select(g => new PausaPorVehiculoDto
                {
                    VehiculoId = g.Key.VehiculoId,
                    Patente = g.Key.Patente,
                    MarcaModelo = $"{g.Key.Marca} {g.Key.Modelo}",
                    CantidadPausas = g.Count(),
                    TiempoTotal = g.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                })
                .OrderByDescending(p => p.CantidadPausas)
                .Take(10)
                .ToList();

            // Pausas por mes (últimos 6 meses)
            var hace6Meses = DateTime.Now.AddMonths(-6);
            var pausasPorMes = pausasFinalizadas
                .Where(p => p.FechaInicio >= hace6Meses)
                .GroupBy(p => new { 
                    Año = p.FechaInicio.Year, 
                    Mes = p.FechaInicio.Month 
                })
                .Select(g => new PausaPorMesDto
                {
                    Año = g.Key.Año,
                    Mes = g.Key.Mes,
                    Cantidad = g.Count(),
                    TiempoTotal = g.Sum(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                })
                .OrderBy(p => p.Año)
                .ThenBy(p => p.Mes)
                .ToList();

            // Pausas recientes (últimas 15)
            var pausasRecientes = todasPausas
                .OrderByDescending(p => p.FechaInicio)
                .Take(15)
                .ToList();

            // Calcular duración promedio por tipo de vehículo
            var duracionPorTipoVehiculo = pausasFinalizadas
                .Where(p => p.IngresoTaller?.Vehiculo != null)
                .GroupBy(p => p.IngresoTaller!.Vehiculo!.TipoVehiculo)
                .Select(g => new
                {
                    TipoVehiculo = g.Key,
                    TiempoPromedio = g.Average(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours)
                })
                .OrderByDescending(x => x.TiempoPromedio)
                .Take(5)
                .ToDictionary(x => x.TipoVehiculo, x => x.TiempoPromedio);

            var viewModel = new HistoricoPausasViewModel
            {
                // Totales
                TotalPausas = totalPausas,
                PausasActivas = pausasActivas,
                PausasFinalizadas = pausasFinalizadas.Count,
                TiempoTotalHoras = tiempoTotalHoras,
                TiempoPromedioHoras = tiempoPromedioHoras,

                // Por motivo
                PausasPorMotivo = pausasPorMotivo,

                // Por vehículo
                PausasPorVehiculo = pausasPorVehiculo,

                // Tendencia temporal
                PausasPorMes = pausasPorMes,

                // Pausas recientes
                PausasRecientes = pausasRecientes,

                // Estadísticas adicionales
                MotivoPrincipal = pausasPorMotivo.FirstOrDefault()?.Motivo ?? "N/A",
                TiempoPausaMasLarga = pausasFinalizadas.Any() 
                    ? pausasFinalizadas.Max(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours) 
                    : 0,
                TiempoPausaMasCorta = pausasFinalizadas.Any() 
                    ? pausasFinalizadas.Min(p => (p.FechaFin!.Value - p.FechaInicio).TotalHours) 
                    : 0,
                    
                DuracionPorTipoVehiculo = duracionPorTipoVehiculo
            };

            return View(viewModel);
        }
  }
}
