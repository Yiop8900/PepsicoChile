using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Filters;
using PepsicoChile.Services;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    public class NotificacionesController : Controller
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionesController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var notificaciones = await _notificacionService.ObtenerNotificacionesUsuario(usuarioId);
            return View(notificaciones);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _notificacionService.MarcarComoLeida(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            await _notificacionService.MarcarTodasComoLeidas(usuarioId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _notificacionService.EliminarNotificacion(id);
            return RedirectToAction("Index");
        }

        // API para obtener cantidad de no leídas (para el badge en el menú)
        [HttpGet]
        public async Task<IActionResult> ContarNoLeidas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var count = await _notificacionService.ContarNoLeidas(usuarioId);
            return Json(new { count });
        }
    }
}
