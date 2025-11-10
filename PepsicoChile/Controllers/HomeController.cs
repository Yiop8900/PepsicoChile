using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

            var model = new DashboardViewModel
            {
                UsuarioId = usuarioId ?? 0,
                NombreUsuario = usuarioNombre ?? "Usuario",
                RolUsuario = usuarioRol ?? "Sin Rol"
            };

            // Datos comunes
            model.VehiculosEnTaller = await _context.Vehiculos.CountAsync(v => v.Estado == "En Taller");
            model.VehiculosDisponibles = await _context.Vehiculos.CountAsync(v => v.Estado == "Disponible");

            // Datos específicos por rol
            switch (usuarioRol)
            {
                case "Administrador":
                case "JefeTaller":
                    await CargarDashboardAdministrador(model);
                    break;

                case "Supervisor":
                case "CoordinadorZona":
                    await CargarDashboardSupervisor(model);
                    break;

                case "Mecanico":
                    await CargarDashboardMecanico(model, usuarioId.Value);
                    break;

                case "Chofer":
                    await CargarDashboardChofer(model, usuarioId.Value);
                    break;

                case "AsistenteRepuestos":
                    await CargarDashboardAsistenteRepuestos(model, usuarioId.Value);
                    break;

                case "GuardiaAcceso":
                    await CargarDashboardGuardiaAcceso(model, usuarioId.Value);
                    break;

                default:
                    await CargarDashboardGeneral(model);
                    break;
            }

            return View(model);
        }

        private async Task CargarDashboardAdministrador(DashboardViewModel model)
        {
            // Estadísticas generales del taller
            model.VehiculosProgramados = await _context.IngresosTaller
                .CountAsync(i => i.Estado == "Programado");
            
            model.IngresosEnProceso = await _context.IngresosTaller
                .CountAsync(i => i.Estado == "En Proceso");
            
            model.TareasPendientes = await _context.TareasTaller
                .CountAsync(t => t.Estado == "Pendiente");
            
            model.TareasEnProceso = await _context.TareasTaller
                .CountAsync(t => t.Estado == "En Proceso");

            // Ingresos activos
            model.IngresosRecientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Include(i => i.Supervisor)
                .Where(i => i.Estado != "Completado" && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .Take(10)
                .ToListAsync();

            // Tareas urgentes
            model.TareasUrgentes = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Include(t => t.MecanicoAsignado)
                .Where(t => t.Estado != "Completada" && t.Prioridad == "Alta")
                .OrderBy(t => t.FechaAsignacion)
                .Take(10)
                .ToListAsync();

            // Notificaciones no leídas
            model.NotificacionesNoLeidas = await _context.Notificaciones
                .Where(n => !n.Leida)
                .CountAsync();

            // Pausas activas
            model.PausasActivas = await _context.Pausas
                .Where(p => p.FechaFin == null)
                .CountAsync();
        }

        private async Task CargarDashboardSupervisor(DashboardViewModel model)
        {
            // Similar a administrador pero puede enfocarse en sus ingresos
            model.VehiculosProgramados = await _context.IngresosTaller
                .CountAsync(i => i.Estado == "Programado");
            
            model.IngresosEnProceso = await _context.IngresosTaller
                .CountAsync(i => i.Estado == "En Proceso");
            
            model.TareasPendientes = await _context.TareasTaller
                .CountAsync(t => t.Estado == "Pendiente");

            model.IngresosRecientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.Estado != "Completado" && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .Take(10)
                .ToListAsync();

            model.TareasUrgentes = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Include(t => t.MecanicoAsignado)
                .Where(t => t.Estado != "Completada" && t.Prioridad == "Alta")
                .OrderBy(t => t.FechaAsignacion)
                .Take(8)
                .ToListAsync();
        }

        private async Task CargarDashboardMecanico(DashboardViewModel model, int mecanicoId)
        {
            // Solo las tareas del mecánico
            model.TareasPendientes = await _context.TareasTaller
                .CountAsync(t => t.MecanicoAsignadoId == mecanicoId && t.Estado == "Pendiente");
            
            model.TareasEnProceso = await _context.TareasTaller
                .CountAsync(t => t.MecanicoAsignadoId == mecanicoId && t.Estado == "En Proceso");
            
            model.TareasCompletadasHoy = await _context.TareasTaller
                .CountAsync(t => t.MecanicoAsignadoId == mecanicoId 
                    && t.Estado == "Completada"
                    && t.FechaFinalizacion.HasValue
                    && t.FechaFinalizacion.Value.Date == DateTime.Today);

            // Tareas del mecánico
            model.TareasUrgentes = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Where(t => t.MecanicoAsignadoId == mecanicoId 
                    && t.Estado != "Completada" 
                    && t.Prioridad == "Alta")
                .OrderBy(t => t.FechaAsignacion)
                .Take(5)
                .ToListAsync();

            // Ingresos donde está asignado
            model.IngresosRecientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Where(i => i.MecanicoAsignadoId == mecanicoId 
                    && i.Estado != "Completado" 
                    && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .Take(5)
                .ToListAsync();

            // Notificaciones del mecánico
            model.NotificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == mecanicoId && !n.Leida)
                .CountAsync();
        }

        private async Task CargarDashboardChofer(DashboardViewModel model, int choferId)
        {
            // Ingresos del chofer
            model.IngresosRecientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.MecanicoAsignado)
                .Include(i => i.Supervisor)
                .Where(i => i.ChoferId == choferId 
                    && i.Estado != "Completado" 
                    && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .Take(10)
                .ToListAsync();

            model.VehiculosProgramados = await _context.IngresosTaller
                .CountAsync(i => i.ChoferId == choferId && i.Estado == "Programado");
            
            model.IngresosEnProceso = await _context.IngresosTaller
                .CountAsync(i => i.ChoferId == choferId && i.Estado == "En Proceso");
        }

        private async Task CargarDashboardAsistenteRepuestos(DashboardViewModel model, int usuarioId)
        {
            // Estadísticas de Repuestos
            var totalRepuestos = await _context.Repuestos.CountAsync(r => r.Activo);
            var repuestosBajoStock = await _context.Repuestos
                .CountAsync(r => r.Activo && r.StockActual <= r.StockMinimo);
            var repuestosSinStock = await _context.Repuestos
                .CountAsync(r => r.Activo && r.StockActual == 0);

            // Estadísticas de Solicitudes
            var solicitudesPendientes = await _context.SolicitudesRepuesto
                .CountAsync(s => s.Estado == "Solicitado");
            var solicitudesEnTransito = await _context.SolicitudesRepuesto
                .CountAsync(s => s.Estado == "En Tránsito");
            var solicitudesEntregadas = await _context.SolicitudesRepuesto
                .CountAsync(s => s.Estado == "Entregado");

            // Usar campos existentes del ViewModel de manera creativa
            model.VehiculosEnTaller = repuestosBajoStock; // Repuestos con bajo stock
            model.VehiculosDisponibles = totalRepuestos; // Total de repuestos
            model.VehiculosProgramados = solicitudesPendientes; // Solicitudes pendientes
            model.IngresosEnProceso = solicitudesEnTransito; // Solicitudes en tránsito
            model.TareasPendientes = repuestosSinStock; // Repuestos sin stock
            model.TareasEnProceso = solicitudesEntregadas; // Solicitudes entregadas
            
            // Movimientos recientes (últimos 10)
            var movimientosRecientes = await _context.MovimientosRepuesto
                .Include(m => m.Repuesto)
                .Include(m => m.Vehiculo)
                .Include(m => m.UsuarioRegistro)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(10)
                .ToListAsync();

            // Notificaciones no leídas
            model.NotificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .CountAsync();

            // Solicitudes urgentes (solicitadas en las últimas 24 horas)
            var hace24Horas = DateTime.Now.AddHours(-24);
            var solicitudesUrgentes = await _context.SolicitudesRepuesto
                .Include(s => s.TareaTaller)
                    .ThenInclude(t => t!.IngresoTaller)
                        .ThenInclude(i => i!.Vehiculo)
                .Include(s => s.SolicitadoPor)
                .Where(s => s.Estado == "Solicitado" && s.FechaSolicitud >= hace24Horas)
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(10)
                .ToListAsync();

            // Repuestos con stock crítico
            var repuestosCriticos = await _context.Repuestos
                .Where(r => r.Activo && r.StockActual <= r.StockMinimo)
                .OrderBy(r => r.StockActual)
                .Take(10)
                .ToListAsync();

            // Guardar datos adicionales en ViewBag para la vista
            ViewBag.MovimientosRecientes = movimientosRecientes;
            ViewBag.SolicitudesUrgentes = solicitudesUrgentes;
            ViewBag.RepuestosCriticos = repuestosCriticos;
            ViewBag.TotalRepuestos = totalRepuestos;
            ViewBag.RepuestosBajoStock = repuestosBajoStock;
            ViewBag.RepuestosSinStock = repuestosSinStock;
            ViewBag.SolicitudesPendientes = solicitudesPendientes;
            ViewBag.SolicitudesEnTransito = solicitudesEnTransito;
            ViewBag.SolicitudesEntregadas = solicitudesEntregadas;
        }

        private async Task CargarDashboardGuardiaAcceso(DashboardViewModel model, int usuarioId)
        {
            // Estadísticas de llegadas del día
            var hoy = DateTime.Today;
            var llegadasHoy = await _context.IngresosTaller
                .Where(i => i.FechaIngresoReal.HasValue && i.FechaIngresoReal.Value.Date == hoy)
                .CountAsync();

            // Vehículos programados para hoy
            var programadosHoy = await _context.IngresosTaller
                .Where(i => i.FechaProgramada.Date == hoy && i.Estado == "Programado")
                .CountAsync();

            // Vehículos actualmente en taller
            var vehiculosEnTaller = await _context.IngresosTaller
                .CountAsync(i => i.Estado == "En Proceso" || i.Estado == "Pausado");

            // Vehículos esperando entrada
            var esperandoEntrada = await _context.IngresosTaller
                .Where(i => i.Estado == "Programado" && 
                           i.FechaProgramada.Date <= DateTime.Today &&
                           !i.FechaIngresoReal.HasValue)
                .CountAsync();

            // Usar campos del ViewModel
            model.VehiculosEnTaller = vehiculosEnTaller;
            model.VehiculosProgramados = programadosHoy;
            model.IngresosEnProceso = llegadasHoy;
            model.VehiculosDisponibles = esperandoEntrada;

            // Notificaciones no leídas
            model.NotificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .CountAsync();

            // Últimas llegadas registradas (hoy)
            var ultimasLlegadas = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Where(i => i.FechaIngresoReal.HasValue && i.FechaIngresoReal.Value.Date == hoy)
                .OrderByDescending(i => i.FechaIngresoReal)
                .Take(10)
                .ToListAsync();

            // Vehículos programados pendientes de llegada
            var programadosPendientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Where(i => i.Estado == "Programado" && 
                           i.FechaProgramada.Date <= DateTime.Today.AddDays(1) &&
                           !i.FechaIngresoReal.HasValue)
                .OrderBy(i => i.FechaProgramada)
                .Take(15)
                .ToListAsync();

            // Estadísticas por turno (mañana/tarde/noche)
            var horaActual = DateTime.Now.Hour;
            var turnoActual = horaActual < 12 ? "Mañana" : horaActual < 18 ? "Tarde" : "Noche";
            
            var llegadasPorTurno = await _context.IngresosTaller
                .Where(i => i.FechaIngresoReal.HasValue && i.FechaIngresoReal.Value.Date == hoy)
                .ToListAsync();

            var llegadasManana = llegadasPorTurno.Count(i => i.FechaIngresoReal!.Value.Hour < 12);
            var llegadasTarde = llegadasPorTurno.Count(i => i.FechaIngresoReal!.Value.Hour >= 12 && i.FechaIngresoReal!.Value.Hour < 18);
            var llegadasNoche = llegadasPorTurno.Count(i => i.FechaIngresoReal!.Value.Hour >= 18);

            // Guardar datos en ViewBag
            ViewBag.UltimasLlegadas = ultimasLlegadas;
            ViewBag.ProgramadosPendientes = programadosPendientes;
            ViewBag.LlegadasHoy = llegadasHoy;
            ViewBag.ProgramadosHoy = programadosHoy;
            ViewBag.VehiculosEnTaller = vehiculosEnTaller;
            ViewBag.EsperandoEntrada = esperandoEntrada;
            ViewBag.TurnoActual = turnoActual;
            ViewBag.LlegadasManana = llegadasManana;
            ViewBag.LlegadasTarde = llegadasTarde;
            ViewBag.LlegadasNoche = llegadasNoche;
        }

        private async Task CargarDashboardGeneral(DashboardViewModel model)
        {
            // Dashboard básico para roles no específicos
            model.IngresosRecientes = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Where(i => i.Estado != "Completado" && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .Take(5)
                .ToListAsync();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
