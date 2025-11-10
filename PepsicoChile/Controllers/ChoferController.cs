using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;
using Microsoft.AspNetCore.Hosting;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "Recepcionista", "JefeTaller", "GuardiaAcceso", "Supervisor")]
    public class ChoferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ChoferController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        // VER VEHÍCULO ASIGNADO
        [AuthorizeRole("GuardiaAcceso")]
        public async Task<IActionResult> MiVehiculo()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener asignación activa del chofer/guardia
            var asignacion = await _context.AsignacionesVehiculo
                .Include(a => a.Vehiculo)
                .Include(a => a.AsignadoPor)
                .FirstOrDefaultAsync(a => a.ChoferId == usuarioId.Value && a.Activa);

            if (asignacion == null)
            {
                ViewBag.SinVehiculo = true;
                return View();
            }

            // Obtener documentos del vehículo
            var documentos = await _context.DocumentosVehiculo
                .Where(d => d.VehiculoId == asignacion.VehiculoId && d.Activo)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            // Obtener últimos ingresos del vehículo
            var ingresos = await _context.IngresosTaller
                .Where(i => i.VehiculoId == asignacion.VehiculoId && i.ChoferId == usuarioId.Value)
                .Include(i => i.MecanicoAsignado)
                .OrderByDescending(i => i.FechaProgramada)
                .Take(5)
                .ToListAsync();

            ViewBag.Documentos = documentos;
            ViewBag.Ingresos = ingresos;
            ViewBag.Asignacion = asignacion;

            return View(asignacion.Vehiculo);
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarLlegada()
        {
            // Obtener solo vehículos que tienen un ingreso programado
            var vehiculosProgramados = await _context.IngresosTaller
                .Where(i => i.Estado == "Programado" && i.FechaProgramada.Date <= DateTime.Now.Date.AddDays(1))
                .Include(i => i.Vehiculo)
                .Select(i => i.Vehiculo)
                .Distinct()
                .OrderBy(v => v.Patente)
                .ToListAsync();

            var model = new RegistroLlegadaViewModel
            {
                VehiculosDisponibles = vehiculosProgramados,
                FechaHoraLlegada = DateTime.Now
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarLlegada(RegistroLlegadaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                    // Buscar el ingreso programado para este vehículo
                    var ingresoProgramado = await _context.IngresosTaller
                        .Include(i => i.Vehiculo)
                        .FirstOrDefaultAsync(i => i.VehiculoId == model.VehiculoId && i.Estado == "Programado");

                    if (ingresoProgramado == null)
                    {
                        TempData["Error"] = "No se encontró una programación activa para este vehículo";
                        model.VehiculosDisponibles = await _context.IngresosTaller
                            .Where(i => i.Estado == "Programado" && i.FechaProgramada.Date <= DateTime.Now.Date.AddDays(1))
                            .Include(i => i.Vehiculo)
                            .Select(i => i.Vehiculo)
                            .Distinct()
                            .OrderBy(v => v.Patente)
                            .ToListAsync();
                        return View(model);
                    }

                    // Actualizar el ingreso con la información de llegada real
                    ingresoProgramado.FechaIngresoReal = model.FechaHoraLlegada;
                    ingresoProgramado.KilometrajeIngreso = model.KilometrajeIngreso;
                    ingresoProgramado.ObservacionesChofer = model.ObservacionesChofer;
                    ingresoProgramado.Estado = "En Proceso";

                    // Actualizar estado del vehículo
                    if (ingresoProgramado.Vehiculo != null)
                    {
                        ingresoProgramado.Vehiculo.Estado = "En Taller";
                        ingresoProgramado.Vehiculo.FechaActualizacion = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    // Guardar imágenes si se subieron
                    if (model.ImagenesVehiculo != null && model.ImagenesVehiculo.Any())
                    {
                        await GuardarImagenesIngreso(ingresoProgramado.Id, model.ImagenesVehiculo, usuarioId);
                    }

                    TempData["Mensaje"] = $"Llegada del vehículo {ingresoProgramado.Vehiculo?.Patente} registrada exitosamente con {model.ImagenesVehiculo?.Count ?? 0} imagen(es)";
                    return RedirectToAction("MisIngresos");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al registrar la llegada: {ex.Message}");
                }
            }

            // Recargar vehículos en caso de error
            model.VehiculosDisponibles = await _context.IngresosTaller
                .Where(i => i.Estado == "Programado" && i.FechaProgramada.Date <= DateTime.Now.Date.AddDays(1))
                .Include(i => i.Vehiculo)
                .Select(i => i.Vehiculo)
                .Distinct()
                .OrderBy(v => v.Patente)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> MisIngresos()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener ingresos del guardia/chofer actual
            var ingresos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.ChoferId == usuarioId.Value)
                .OrderByDescending(i => i.FechaProgramada)
                .ToListAsync();

            return View(ingresos);
        }

        public async Task<IActionResult> DetalleIngreso(int id)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            return View(ingreso);
        }

        // Método auxiliar para guardar imágenes del ingreso
        private async Task GuardarImagenesIngreso(int ingresoId, List<IFormFile> imagenes, int? usuarioId)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "ingresos", ingresoId.ToString());
            Directory.CreateDirectory(uploadsPath);

            foreach (var imagen in imagenes)
            {
                // Validar que sea una imagen
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    continue; // Saltar archivos que no sean imágenes
                }

                // Validar tamaño (máximo 5MB por imagen)
                if (imagen.Length > 5 * 1024 * 1024)
                {
                    continue; // Saltar imágenes muy grandes
                }

                // Generar nombre único
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Guardar el archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // Guardar ruta relativa en BD
                var rutaRelativa = $"/uploads/ingresos/{ingresoId}/{fileName}";

                var imagenIngreso = new ImagenIngreso
                {
                    IngresoTallerId = ingresoId,
                    NombreArchivo = imagen.FileName,
                    RutaArchivo = rutaRelativa,
                    FechaSubida = DateTime.Now,
                    UsuarioSubidaId = usuarioId,
                    TamañoBytes = imagen.Length,
                    TipoImagen = "General",
                    Descripcion = "Imagen del vehículo al momento de la llegada"
                };

                _context.ImagenesIngreso.Add(imagenIngreso);
            }

            await _context.SaveChangesAsync();
        }
    }
}
