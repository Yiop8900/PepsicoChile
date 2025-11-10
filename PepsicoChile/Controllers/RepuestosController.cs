using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "AsistenteRepuestos")]
    public class RepuestosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepuestosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gestión de Repuestos
        public async Task<IActionResult> Index()
        {
            var repuestos = await _context.Repuestos
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            var repuestosBajoStock = repuestos
                .Where(r => r.StockActual <= r.StockMinimo)
                .ToList();

            var model = new GestionRepuestosViewModel
            {
                Repuestos = repuestos,
                RepuestosBajoStock = repuestosBajoStock,
                TotalRepuestos = repuestos.Count,
                RepuestosBajoStockCount = repuestosBajoStock.Count,
                ValorTotalInventario = repuestos.Sum(r => (r.PrecioUnitario ?? 0) * r.StockActual)
            };

            return View(model);
        }

        // GET: Agregar Repuesto
        [HttpGet]
        public IActionResult Agregar()
        {
            return View(new AgregarRepuestoViewModel());
        }

        // POST: Agregar Repuesto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(AgregarRepuestoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que no exista un repuesto con el mismo nombre y proveedor
                    var repuestoExistente = await _context.Repuestos
                        .FirstOrDefaultAsync(r => r.Nombre == model.Nombre && 
                                                  r.Proveedor == model.Proveedor &&
                                                  r.Activo);

                    if (repuestoExistente != null)
                    {
                        ModelState.AddModelError("", $"Ya existe un repuesto con el nombre '{model.Nombre}' del proveedor '{model.Proveedor}'. ");
                        return View(model);
                    }

                    // Generar código automático
                    var codigoRepuesto = await GenerarCodigoRepuesto(model.Categoria);

                    var repuesto = new Repuesto
                    {
                        Nombre = model.Nombre,
                        CodigoRepuesto = codigoRepuesto,
                        Categoria = model.Categoria,
                        Descripcion = model.Descripcion,
                        StockActual = model.StockInicial,
                        StockMinimo = model.StockMinimo,
                        Ubicacion = model.Ubicacion,
                        PrecioUnitario = model.PrecioUnitario,
                        Proveedor = model.Proveedor,
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        FechaActualizacion = DateTime.Now
                    };

                    _context.Repuestos.Add(repuesto);
                    await _context.SaveChangesAsync();

                    // Registrar movimiento de entrada inicial
                    if (model.StockInicial > 0)
                    {
                        var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                        var movimiento = new MovimientoRepuesto
                        {
                            RepuestoId = repuesto.Id,
                            TipoMovimiento = "Entrada",
                            Cantidad = model.StockInicial,
                            StockAnterior = 0,
                            StockNuevo = model.StockInicial,
                            Motivo = "Stock inicial",
                            UsuarioRegistroId = usuarioId,
                            FechaMovimiento = DateTime.Now
                        };
                        _context.MovimientosRepuesto.Add(movimiento);
                        await _context.SaveChangesAsync();
                    }

                    TempData["Mensaje"] = $"Repuesto '{repuesto.Nombre}' agregado exitosamente con código {codigoRepuesto}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al agregar repuesto: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Editar Repuesto
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null)
            {
                return NotFound();
            }

            var model = new AgregarRepuestoViewModel
            {
                Nombre = repuesto.Nombre,
                CodigoRepuesto = repuesto.CodigoRepuesto,
                Categoria = repuesto.Categoria,
                Descripcion = repuesto.Descripcion,
                StockInicial = repuesto.StockActual,
                StockMinimo = repuesto.StockMinimo,
                Ubicacion = repuesto.Ubicacion,
                PrecioUnitario = repuesto.PrecioUnitario,
                Proveedor = repuesto.Proveedor
            };

            ViewBag.RepuestoId = id;
            ViewBag.IsEdit = true;
            return View("Agregar", model);
        }

        // POST: Editar Repuesto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AgregarRepuestoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var repuesto = await _context.Repuestos.FindAsync(id);
                    if (repuesto == null)
                    {
                        return NotFound();
                    }

                    // Validar que no exista otro repuesto con el mismo nombre y proveedor
                    var repuestoExistente = await _context.Repuestos
                        .FirstOrDefaultAsync(r => r.Nombre == model.Nombre && 
                                                  r.Proveedor == model.Proveedor &&
                                                  r.Activo &&
                                                  r.Id != id); // Excluir el repuesto actual

                    if (repuestoExistente != null)
                    {
                        ModelState.AddModelError("", $"Ya existe otro repuesto con el nombre '{model.Nombre}' del proveedor '{model.Proveedor}'.");
                        ViewBag.RepuestoId = id;
            ViewBag.IsEdit = true;
            return View("Agregar", model);
                    }

                    repuesto.Nombre = model.Nombre;
                    // No modificar el código si ya existe
                    if (string.IsNullOrEmpty(repuesto.CodigoRepuesto))
                    {
                        repuesto.CodigoRepuesto = await GenerarCodigoRepuesto(model.Categoria);
                    }
                    repuesto.Categoria = model.Categoria;
                    repuesto.Descripcion = model.Descripcion;
                    repuesto.StockMinimo = model.StockMinimo;
                    repuesto.Ubicacion = model.Ubicacion;
                    repuesto.PrecioUnitario = model.PrecioUnitario;
                    repuesto.Proveedor = model.Proveedor;
                    repuesto.FechaActualizacion = DateTime.Now;

                    _context.Repuestos.Update(repuesto);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = $"Repuesto '{repuesto.Nombre}' actualizado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar repuesto: {ex.Message}");
                }
            }

            ViewBag.RepuestoId = id;
            ViewBag.IsEdit = true;
            return View("Agregar", model);
        }

        // GET: Entregar Repuesto
        [HttpGet]
        public async Task<IActionResult> Entregar(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null)
            {
                return NotFound();
            }

            var model = new EntregarRepuestoViewModel
            {
                RepuestoId = id,
                MecanicosDisponibles = await _context.Usuarios
                    .Where(u => u.Rol == "Mecanico" && u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync(),
                VehiculosDisponibles = await _context.Vehiculos
                    .Where(v => v.Estado == "En Taller")
                    .OrderBy(v => v.Patente)
                    .ToListAsync()
            };

            ViewBag.Repuesto = repuesto;
            return View(model);
        }

        // POST: Entregar Repuesto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entregar(EntregarRepuestoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var repuesto = await _context.Repuestos.FindAsync(model.RepuestoId);
                    if (repuesto == null)
                    {
                        TempData["Error"] = "Repuesto no encontrado";
                        return RedirectToAction("Index");
                    }

                    if (repuesto.StockActual < model.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente. Stock actual: {repuesto.StockActual}";
                        return RedirectToAction("Entregar", new { id = model.RepuestoId });
                    }

                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                    // Registrar movimiento de salida
                    var movimiento = new MovimientoRepuesto
                    {
                        RepuestoId = repuesto.Id,
                        TipoMovimiento = "Salida",
                        Cantidad = model.Cantidad,
                        StockAnterior = repuesto.StockActual,
                        StockNuevo = repuesto.StockActual - model.Cantidad,
                        Motivo = model.Motivo ?? "Entrega a mecánico",
                        VehiculoId = model.VehiculoId,
                        TareaTallerId = model.TareaTallerId,
                        MecanicoId = model.MecanicoId,
                        UsuarioRegistroId = usuarioId,
                        FechaMovimiento = DateTime.Now
                    };

                    _context.MovimientosRepuesto.Add(movimiento);

                    // Actualizar stock
                    repuesto.StockActual -= model.Cantidad;
                    repuesto.FechaActualizacion = DateTime.Now;
                    _context.Repuestos.Update(repuesto);

                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = $"{model.Cantidad} unidad(es) de '{repuesto.Nombre}' entregadas exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al entregar repuesto: {ex.Message}");
                }
            }

            model.MecanicosDisponibles = await _context.Usuarios
                .Where(u => u.Rol == "Mecanico" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            model.VehiculosDisponibles = await _context.Vehiculos
                .Where(v => v.Estado == "En Taller")
                .OrderBy(v => v.Patente)
                .ToListAsync();

            return View(model);
        }

        // GET: Recibir Stock
        [HttpGet]
        public async Task<IActionResult> RecibirStock(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null)
            {
                return NotFound();
            }

            ViewBag.Repuesto = repuesto;
            return View();
        }

        // POST: Recibir Stock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecibirStock(int id, int cantidad, string? motivo)
        {
            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction("RecibirStock", new { id });
            }

            try
            {
                var repuesto = await _context.Repuestos.FindAsync(id);
                if (repuesto == null)
                {
                    TempData["Error"] = "Repuesto no encontrado";
                    return RedirectToAction("Index");
                }

                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                // Registrar movimiento de entrada
                var movimiento = new MovimientoRepuesto
                {
                    RepuestoId = repuesto.Id,
                    TipoMovimiento = "Entrada",
                    Cantidad = cantidad,
                    StockAnterior = repuesto.StockActual,
                    StockNuevo = repuesto.StockActual + cantidad,
                    Motivo = motivo ?? "Recepción de stock",
                    UsuarioRegistroId = usuarioId,
                    FechaMovimiento = DateTime.Now
                };

                _context.MovimientosRepuesto.Add(movimiento);

                // Actualizar stock
                repuesto.StockActual += cantidad;
                repuesto.FechaActualizacion = DateTime.Now;
                _context.Repuestos.Update(repuesto);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"{cantidad} unidad(es) de '{repuesto.Nombre}' recibidas exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al recibir stock: {ex.Message}";
                return RedirectToAction("RecibirStock", new { id });
            }
        }

        // GET: Historial de Movimientos
        public async Task<IActionResult> Historial(int? repuestoId)
        {
            var movimientos = await _context.MovimientosRepuesto
                .Include(m => m.Repuesto)
                .Include(m => m.Vehiculo)
                .Include(m => m.Mecanico)
                .Include(m => m.UsuarioRegistro)
                .Where(m => !repuestoId.HasValue || m.RepuestoId == repuestoId.Value)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(100)
                .ToListAsync();

            var model = new HistorialMovimientosViewModel
            {
                Movimientos = movimientos,
                RepuestoId = repuestoId
            };

            if (repuestoId.HasValue)
            {
                ViewBag.Repuesto = await _context.Repuestos.FindAsync(repuestoId.Value);
            }

            return View(model);
        }

        // GET: Detalle Repuesto
        public async Task<IActionResult> Detalle(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null)
            {
                return NotFound();
            }

            var movimientos = await _context.MovimientosRepuesto
                .Include(m => m.Vehiculo)
                .Include(m => m.Mecanico)
                .Include(m => m.UsuarioRegistro)
                .Where(m => m.RepuestoId == id)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(20)
                .ToListAsync();

            ViewBag.Movimientos = movimientos;
            return View(repuesto);
        }

        // Método privado para generar código automático
        private async Task<string> GenerarCodigoRepuesto(string? categoria)
        {
            // Obtener prefijo según categoría
            var prefijo = ObtenerPrefijoCategoria(categoria);
            
            // Obtener el último número usado para esta categoría
            var ultimoCodigo = await _context.Repuestos
                .Where(r => r.CodigoRepuesto.StartsWith(prefijo))
                .OrderByDescending(r => r.Id)
                .Select(r => r.CodigoRepuesto)
                .FirstOrDefaultAsync();

            int nuevoNumero = 1;

            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                // Extraer el número del código (ej: "MOT-0045" -> 45)
                var parteNumerica = ultimoCodigo.Substring(prefijo.Length);
                if (int.TryParse(parteNumerica, out int numeroActual))
                {
                    nuevoNumero = numeroActual + 1;
                }
            }

            // Generar código con formato: PREFIJO-NNNN (4 dígitos)
            return $"{prefijo}{nuevoNumero:D4}";
        }

        // Método privado para obtener prefijo según categoría
        private string ObtenerPrefijoCategoria(string? categoria)
        {
            return categoria?.ToUpper() switch
            {
                "MOTOR" => "MOT-",
                "TRANSMISIÓN" => "TRN-",
                "FRENOS" => "FRE-",
                "SUSPENSIÓN" => "SUS-",
                "ELÉCTRICO" => "ELE-",
                "NEUMÁTICOS" => "NEU-",
                "FILTROS" => "FIL-",
                "LUBRICANTES" => "LUB-",
                "REFRIGERACIÓN" => "REF-",
                "CARROCERÍA" => "CAR-",
                _ => "REP-" // Código genérico
            };
        }
    }
}
