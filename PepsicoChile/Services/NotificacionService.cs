using PepsicoChile.Data;
using PepsicoChile.Models;
using Microsoft.EntityFrameworkCore;

namespace PepsicoChile.Services
{
    public interface INotificacionService
    {
        Task CrearNotificacion(int usuarioId, string titulo, string mensaje, string tipo = "Info",
          int? tareaId = null, int? ingresoId = null, int? vehiculoId = null, string? urlAccion = null);

        Task<List<Notificacion>> ObtenerNotificacionesUsuario(int usuarioId, bool soloNoLeidas = false);

        Task MarcarComoLeida(int notificacionId);

        Task MarcarTodasComoLeidas(int usuarioId);

        Task<int> ContarNoLeidas(int usuarioId);

        Task EliminarNotificacion(int notificacionId);
    }

    public class NotificacionService : INotificacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;

        public NotificacionService(ApplicationDbContext context, IWhatsAppService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        public async Task CrearNotificacion(int usuarioId, string titulo, string mensaje, string tipo = "Info",
    int? tareaId = null, int? ingresoId = null, int? vehiculoId = null, string? urlAccion = null)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                TareaId = tareaId,
                IngresoId = ingresoId,
                VehiculoId = vehiculoId,
                UrlAccion = urlAccion,
                FechaCreacion = DateTime.Now,
                Leida = false
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            // Enviar WhatsApp si es urgente o es una tarea
            if (tipo == "Urgente" || tipo == "Tarea")
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario != null && !string.IsNullOrEmpty(usuario.Telefono))
                {
                    try
                    {
                        var whatsAppSid = await _whatsAppService.EnviarMensaje(
                    usuario.Telefono,
                        $"*{titulo}*\n\n{mensaje}"
                         );

                        notificacion.EnviadoWhatsApp = true;
                        notificacion.FechaEnvioWhatsApp = DateTime.Now;
                        notificacion.WhatsAppSid = whatsAppSid;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log error pero no fallar la notificación
                        Console.WriteLine($"Error enviando WhatsApp: {ex.Message}");
                    }
                }
            }
        }

        public async Task<List<Notificacion>> ObtenerNotificacionesUsuario(int usuarioId, bool soloNoLeidas = false)
        {
            var query = _context.Notificaciones
          .Where(n => n.UsuarioId == usuarioId);

            if (soloNoLeidas)
            {
                query = query.Where(n => !n.Leida);
            }

            return await query
                  .OrderByDescending(n => n.FechaCreacion)
         .Take(50)
                .ToListAsync();
        }

        public async Task MarcarComoLeida(int notificacionId)
        {
            var notificacion = await _context.Notificaciones.FindAsync(notificacionId);
            if (notificacion != null && !notificacion.Leida)
            {
                notificacion.Leida = true;
                notificacion.FechaLeida = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarcarTodasComoLeidas(int usuarioId)
        {
            var notificaciones = await _context.Notificaciones
              .Where(n => n.UsuarioId == usuarioId && !n.Leida)
              .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.Leida = true;
                notificacion.FechaLeida = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarNoLeidas(int usuarioId)
        {
            return await _context.Notificaciones
         .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
        }

        public async Task EliminarNotificacion(int notificacionId)
        {
            var notificacion = await _context.Notificaciones.FindAsync(notificacionId);
            if (notificacion != null)
            {
                _context.Notificaciones.Remove(notificacion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
