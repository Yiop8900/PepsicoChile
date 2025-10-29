using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PepsicoChile.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
    {
      var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");

      if (!usuarioId.HasValue)
       {
    // Si no hay sesión, redirigir al login
             context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
    }

            base.OnActionExecuting(context);
   }
    }

    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

    public AuthorizeRoleAttribute(params string[] roles)
        {
       _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");
     var usuarioRol = context.HttpContext.Session.GetString("UsuarioRol");

    if (!usuarioId.HasValue)
            {
                // Si no hay sesión, redirigir al login
   context.Result = new RedirectToActionResult("Login", "Account", null);
    return;
            }

 if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(usuarioRol))
     {
    // Si el rol no está permitido, redirigir al dashboard con mensaje de error
      context.HttpContext.Session.SetString("ErrorMensaje", "No tienes permisos para acceder a esta página");
        context.Result = new RedirectToActionResult("Index", "Home", null);
            }

   base.OnActionExecuting(context);
        }
    }
}
