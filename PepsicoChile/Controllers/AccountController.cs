using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;
using System.Security.Cryptography;
using System.Text;

namespace PepsicoChile.Controllers
{
    public class AccountController : Controller
    {
    private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
   _context = context;
        }

 [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
         ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
        try
    {
    var hashedPassword = HashPassword(model.Password);

              var usuario = await _context.Usuarios
  .FirstOrDefaultAsync(u => u.Email == model.Email
    && u.Password == hashedPassword
        && u.Activo);

      if (usuario != null)
          {
   HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
   HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre + " " + usuario.Apellido);
        HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);

        TempData["Mensaje"] = $"¡Bienvenido {usuario.Nombre}!";

    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
         {
    return Redirect(returnUrl);
         }
     else
    {
     return RedirectToAction("Index", "Home");
            }
         }
         else
             {
   ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos, o usuario inactivo");
    }
        }
       catch (Exception ex)
 {
           ModelState.AddModelError(string.Empty, $"Error al iniciar sesión: {ex.Message}");
  Console.WriteLine($"Error en Login: {ex}");
              }
        
         }

            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
    HttpContext.Session.Clear();
            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
     {
      using (var sha256 = SHA256.Create())
          {
  var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
            }
        }

        [HttpGet]
   public IActionResult TestHash(string password = "123456")
        {
        var hash = HashPassword(password);
  var expectedHash = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=";
            
    return Json(new
      {
       Password = password,
           GeneratedHash = hash,
   ExpectedHash = expectedHash,
    Match = hash == expectedHash,
    Message = hash == expectedHash ? "? Hash correcto" : "? Hash incorrecto"
    });
        }

        [HttpGet]
        public async Task<IActionResult> TestUsers()
        {
         var usuarios = await _context.Usuarios
                .Select(u => new
          {
         u.Id,
u.Email,
  u.Nombre,
 u.Apellido,
             u.Rol,
     u.Activo,
     PasswordHash = u.Password.Substring(0, 20) + "..." // Solo mostrar parte del hash
    })
       .ToListAsync();

            return Json(new
            {
       TotalUsuarios = usuarios.Count,
      Usuarios = usuarios,
         HashEsperado = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI="
       });
     }

        // SOLO ADMINISTRADOR
   [HttpGet]
        [AuthorizeSession]
        [AuthorizeRole("Administrador")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
     [ValidateAntiForgeryToken]
        [AuthorizeSession]
        [AuthorizeRole("Administrador")]
 public async Task<IActionResult> Register(RegisterViewModel model)
{
            if (ModelState.IsValid)
      {
  var existeEmail = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
    var existeRut = await _context.Usuarios.AnyAsync(u => u.Rut == model.Rut);

      if (existeEmail)
    {
        ModelState.AddModelError("Email", "Este email ya está registrado");
      return View(model);
         }

  if (existeRut)
        {
   ModelState.AddModelError("Rut", "Este RUT ya está registrado");
        return View(model);
     }

     var usuario = new Models.Usuario
       {
        Nombre = model.Nombre,
      Apellido = model.Apellido,
     Email = model.Email,
        Telefono = model.Telefono ?? string.Empty,
    Rut = model.Rut,
      Rol = model.Rol,
        Password = HashPassword(model.Password),
               Activo = true
            };

       _context.Usuarios.Add(usuario);
   await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Usuario registrado exitosamente";
     return RedirectToAction("ListarUsuarios");
    }

 return View(model);
        }

[HttpGet]
 [AuthorizeSession]
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> ListarUsuarios()
        {
       var usuarios = await _context.Usuarios
    .OrderByDescending(u => u.Id)
     .ToListAsync();
            
        return View(usuarios);
        }

        [HttpPost]
        [AuthorizeSession]
        [AuthorizeRole("Administrador")]
    public async Task<IActionResult> ActivarDesactivar(int id)
        {
 var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null && usuario.Rol != "Administrador")
            {
                usuario.Activo = !usuario.Activo;
     await _context.SaveChangesAsync();
          TempData["Mensaje"] = $"Usuario {(usuario.Activo ? "activado" : "desactivado")} correctamente";
   }
     
  return RedirectToAction("ListarUsuarios");
        }
    }
}
