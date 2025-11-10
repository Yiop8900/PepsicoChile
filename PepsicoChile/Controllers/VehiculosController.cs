using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "JefeTaller", "CoordinadorZona", "Recepcionista", "GuardiaAcceso", "AsistenteRepuestos", "Supervisor", "EncargadoLlaves", "Mecanico")]
    public class VehiculosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VehiculosController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                 .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();

            return View(vehiculos);
        }

        [HttpGet]
        public IActionResult Agregar()
        {
            var model = new AgregarVehiculoViewModel
            {
                Año = DateTime.Now.Year
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(AgregarVehiculoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si la patente ya existe
                    var existePatente = await _context.Vehiculos.AnyAsync(v => v.Patente == model.Patente);
                    if (existePatente)
                    {
                        ModelState.AddModelError("Patente", "Esta patente ya está registrada");
                        return View(model);
                    }

                    // Crear el vehículo
                    var vehiculo = new Vehiculo
                    {
                        Patente = model.Patente.ToUpper(),
                        Marca = model.Marca,
                        Modelo = model.Modelo,
                        Año = model.Año,
                        TipoVehiculo = model.TipoVehiculo,
                        NumeroFlota = model.NumeroFlota,
                        Color = model.Color,
                        Combustible = model.Combustible,
                        Motor = model.Motor,
                        Chasis = model.Chasis,
                        KilometrajeActual = model.KilometrajeActual,
                        CapacidadCarga = model.CapacidadCarga,
                        UltimoMantenimiento = model.UltimoMantenimiento,
                        ProximoMantenimiento = model.ProximoMantenimiento,
                        Observaciones = model.Observaciones,
                        Estado = "Disponible",
                        FechaCreacion = DateTime.Now,
                        FechaActualizacion = DateTime.Now
                    };

                    _context.Vehiculos.Add(vehiculo);
                    await _context.SaveChangesAsync();

                    // Guardar documentos
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                    await GuardarDocumentos(vehiculo.Id, model, usuarioId);

                    TempData["Mensaje"] = $"Vehículo {vehiculo.Patente} registrado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al guardar el vehículo: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            var model = new AgregarVehiculoViewModel
            {
                Patente = vehiculo.Patente,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Año = vehiculo.Año,
                TipoVehiculo = vehiculo.TipoVehiculo,
                NumeroFlota = vehiculo.NumeroFlota,
                Color = vehiculo.Color,
                Combustible = vehiculo.Combustible,
                Motor = vehiculo.Motor,
                Chasis = vehiculo.Chasis,
                KilometrajeActual = vehiculo.KilometrajeActual,
                CapacidadCarga = vehiculo.CapacidadCarga,
                UltimoMantenimiento = vehiculo.UltimoMantenimiento,
                ProximoMantenimiento = vehiculo.ProximoMantenimiento,
                Observaciones = vehiculo.Observaciones
            };

            ViewBag.VehiculoId = id;
            ViewBag.IsEdit = true;
            return View("Agregar", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AgregarVehiculoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vehiculo = await _context.Vehiculos.FindAsync(id);
                    if (vehiculo == null)
                    {
                        return NotFound();
                    }

                    // Verificar si la patente cambió y ya existe
                    if (vehiculo.Patente != model.Patente.ToUpper())
                    {
                        var existePatente = await _context.Vehiculos.AnyAsync(v => v.Patente == model.Patente && v.Id != id);
                        if (existePatente)
                        {
                            ModelState.AddModelError("Patente", "Esta patente ya está registrada");
                            ViewBag.VehiculoId = id;
                            ViewBag.IsEdit = true;
                            return View("Agregar", model);
                        }
                    }

                    // Actualizar datos del vehículo
                    vehiculo.Patente = model.Patente.ToUpper();
                    vehiculo.Marca = model.Marca;
                    vehiculo.Modelo = model.Modelo;
                    vehiculo.Año = model.Año;
                    vehiculo.TipoVehiculo = model.TipoVehiculo;
                    vehiculo.NumeroFlota = model.NumeroFlota;
                    vehiculo.Color = model.Color;
                    vehiculo.Combustible = model.Combustible;
                    vehiculo.Motor = model.Motor;
                    vehiculo.Chasis = model.Chasis;
                    vehiculo.KilometrajeActual = model.KilometrajeActual;
                    vehiculo.CapacidadCarga = model.CapacidadCarga;
                    vehiculo.UltimoMantenimiento = model.UltimoMantenimiento;
                    vehiculo.ProximoMantenimiento = model.ProximoMantenimiento;
                    vehiculo.Observaciones = model.Observaciones;
                    vehiculo.FechaActualizacion = DateTime.Now;

                    _context.Vehiculos.Update(vehiculo);
                    await _context.SaveChangesAsync();

                    // Guardar nuevos documentos si se subieron
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                    await GuardarDocumentos(vehiculo.Id, model, usuarioId);

                    TempData["Mensaje"] = $"Vehículo {vehiculo.Patente} actualizado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el vehículo: {ex.Message}");
                }
            }

            ViewBag.VehiculoId = id;
            ViewBag.IsEdit = true;
            return View("Agregar", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                vehiculo.Estado = estado;
                vehiculo.FechaActualizacion = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Estado del vehículo actualizado a: {estado}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            var documentos = await _context.DocumentosVehiculo
                .Where(d => d.VehiculoId == id && d.Activo)
                .Include(d => d.UsuarioSubida)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            var ingresos = await _context.IngresosTaller
                .Where(i => i.VehiculoId == id)
                .Include(i => i.Chofer)
                .OrderByDescending(i => i.FechaProgramada)
                .Take(5)
                .ToListAsync();

            // Obtener asignación activa actual
            var asignacionActual = await _context.AsignacionesVehiculo
                .Include(a => a.Chofer)
                .Include(a => a.AsignadoPor)
                .FirstOrDefaultAsync(a => a.VehiculoId == id && a.Activa);

            ViewBag.Documentos = documentos;
            ViewBag.Ingresos = ingresos;
            ViewBag.AsignacionActual = asignacionActual;

            return View(vehiculo);
        }

        [HttpGet]
        public async Task<IActionResult> VerDocumentos(int id)
        {
            var documentos = await _context.DocumentosVehiculo
            .Where(d => d.VehiculoId == id && d.Activo)
                .OrderByDescending(d => d.FechaSubida)
              .ToListAsync();

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            ViewBag.Vehiculo = vehiculo;

            return View(documentos);
        }

        // ASIGNACIÓN DE VEHÍCULOS A CHOFERES
        [HttpGet]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "CoordinadorZona")]
        public async Task<IActionResult> AsignarChofer(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                TempData["Error"] = "Vehículo no encontrado";
                return RedirectToAction("Index");
            }

            // Obtener asignación activa actual si existe
            var asignacionActual = await _context.AsignacionesVehiculo
                .Include(a => a.Chofer)
                .FirstOrDefaultAsync(a => a.VehiculoId == id && a.Activa);

            // Obtener lista de choferes activos
            var choferes = await _context.Usuarios
                .Where(u => u.Rol == "Chofer" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            var model = new AsignarVehiculoViewModel
            {
                VehiculoId = id,
                Patente = vehiculo.Patente,
                MarcaModelo = $"{vehiculo.Marca} {vehiculo.Modelo}",
                ChoferActualId = asignacionActual?.ChoferId,
                ChoferActualNombre = asignacionActual != null 
                    ? $"{asignacionActual.Chofer?.Nombre} {asignacionActual.Chofer?.Apellido}" 
                    : null,
                Choferes = choferes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "CoordinadorZona")]
        public async Task<IActionResult> AsignarChofer(AsignarVehiculoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                    // Desactivar asignación actual si existe
                    var asignacionActual = await _context.AsignacionesVehiculo
                        .FirstOrDefaultAsync(a => a.VehiculoId == model.VehiculoId && a.Activa);

                    if (asignacionActual != null)
                    {
                        asignacionActual.Activa = false;
                        asignacionActual.FechaDesasignacion = DateTime.Now;
                        _context.Update(asignacionActual);
                    }

                    // Crear nueva asignación
                    var nuevaAsignacion = new AsignacionVehiculo
                    {
                        VehiculoId = model.VehiculoId,
                        ChoferId = model.ChoferSeleccionadoId,
                        FechaAsignacion = DateTime.Now,
                        Activa = true,
                        Observaciones = model.Observaciones,
                        AsignadoPorId = usuarioId
                    };

                    _context.AsignacionesVehiculo.Add(nuevaAsignacion);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = "Vehículo asignado exitosamente";
                    return RedirectToAction("Detalle", new { id = model.VehiculoId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al asignar vehículo: {ex.Message}");
                }
            }

            // Recargar lista de choferes en caso de error
            model.Choferes = await _context.Usuarios
                .Where(u => u.Rol == "Chofer" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "CoordinadorZona")]
        public async Task<IActionResult> DesasignarChofer(int vehiculoId)
        {
            var asignacionActual = await _context.AsignacionesVehiculo
                .FirstOrDefaultAsync(a => a.VehiculoId == vehiculoId && a.Activa);

            if (asignacionActual != null)
            {
                asignacionActual.Activa = false;
                asignacionActual.FechaDesasignacion = DateTime.Now;
                _context.Update(asignacionActual);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Vehículo desasignado exitosamente";
            }

            return RedirectToAction("Detalle", new { id = vehiculoId });
        }

        [HttpGet]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "CoordinadorZona")]
        public async Task<IActionResult> HistorialAsignaciones(int vehiculoId)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
            if (vehiculo == null)
            {
                return NotFound();
            }

            var historial = await _context.AsignacionesVehiculo
                .Include(a => a.Chofer)
                .Include(a => a.AsignadoPor)
                .Where(a => a.VehiculoId == vehiculoId)
                .OrderByDescending(a => a.FechaAsignacion)
                .ToListAsync();

            ViewBag.Vehiculo = vehiculo;
            return View(historial);
        }

        [HttpGet]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "CoordinadorZona")]
        public async Task<IActionResult> VehiculosPorChofer()
        {
            var asignaciones = await _context.AsignacionesVehiculo
                .Include(a => a.Vehiculo)
                .Include(a => a.Chofer)
                .Where(a => a.Activa)
                .OrderBy(a => a.Chofer!.Nombre)
                .ToListAsync();

            return View(asignaciones);
        }

        private async Task GuardarDocumentos(int vehiculoId, AgregarVehiculoViewModel model, int? usuarioId)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "vehiculos", vehiculoId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Revisión Técnica
            if (model.RevisionTecnica != null)
            {
                await GuardarDocumento(vehiculoId, model.RevisionTecnica, "RevisionTecnica",
                model.FechaVencimientoRevTecnica, uploadsPath, usuarioId);
            }

            // Seguro Obligatorio
            if (model.SeguroObligatorio != null)
            {
                await GuardarDocumento(vehiculoId, model.SeguroObligatorio, "SeguroObligatorio",
            model.FechaVencimientoSOAP, uploadsPath, usuarioId);
            }

            // Permiso de Circulación
            if (model.PermisoCirculacion != null)
            {
                await GuardarDocumento(vehiculoId, model.PermisoCirculacion, "PermisoCirculacion",
                     model.FechaVencimientoPermiso, uploadsPath, usuarioId);
            }

            // Contrato
            if (model.Contrato != null)
            {
                await GuardarDocumento(vehiculoId, model.Contrato, "Contrato", null, uploadsPath, usuarioId);
            }

            // Manual
            if (model.Manual != null)
            {
                await GuardarDocumento(vehiculoId, model.Manual, "Manual", null, uploadsPath, usuarioId);
            }

            // Otros documentos
            if (model.OtrosDocumentos != null)
            {
                foreach (var doc in model.OtrosDocumentos)
                {
                    await GuardarDocumento(vehiculoId, doc, "Otro", null, uploadsPath, usuarioId);
                }
            }
        }

        private async Task GuardarDocumento(int vehiculoId, IFormFile archivo, string tipoDocumento,
             DateTime? fechaVencimiento, string uploadsPath, int? usuarioId)
        {
            var fileName = $"{tipoDocumento}_{DateTime.Now:yyyyMMddHHmmss}_{archivo.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Guardar ruta relativa en lugar de absoluta
            var rutaRelativa = $"/uploads/vehiculos/{vehiculoId}/{fileName}";

            var documento = new DocumentoVehiculo
            {
                VehiculoId = vehiculoId,
                TipoDocumento = tipoDocumento,
                NombreArchivo = archivo.FileName,
                RutaArchivo = rutaRelativa,
                FechaSubida = DateTime.Now,
                FechaVencimiento = fechaVencimiento,
                UsuarioSubidaId = usuarioId,
                TamañoBytes = archivo.Length,
                Activo = true
            };

            _context.DocumentosVehiculo.Add(documento);
            await _context.SaveChangesAsync();
        }
    }
}
