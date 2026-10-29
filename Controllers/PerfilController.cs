using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class PerfilController : Controller
{
    public IActionResult Index()
    {
        if(string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("Landing", "Inicio");
        }
        else
        {
            return View();
        }
    }

    public IActionResult Ajustes()
    {
       
            if(string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
            {
                return RedirectToAction("Landing", "Inicio");
            }
            else
            {
                return View();
            }
        
    }
    
    public IActionResult ModificarMisDatos()
    {
        if(string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("Landing", "Inicio");
        }
        else
        {
            return View();
        }
    }


    [HttpPost]
    public IActionResult ModificarMisDatos(string nombre, string email, string nombreUsuario, string contraseña)
    {
        string nombreUsuarioActual = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuarioActual))
        {
            return RedirectToAction("Landing", "Inicio");
        }

        email = email?.Trim();
        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña, email);

        // La contraseña es opcional: si se deja vacía se mantiene la actual
        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña, usuario.Email, contraseñaOpcional: true))
        {
            ViewBag.ErrorMessage = "Los datos ingresados no son válidos.";
            return View();
        }

        BD bd = new BD();
        int idUsuario = bd.ObtenerIdUsuario(nombreUsuarioActual);

        if (!bd.ModificarUsuario(idUsuario, usuario))
        {
            ViewBag.ErrorMessage = "El nombre de usuario ya existe. Por favor, elija otro.";
            return View();
        }

        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Email", usuario.Email);
        // Misma cookie que guarda LoginController al iniciar sesión (la usa el layout para mostrar el nombre)
        Response.Cookies.Append("NombreUsuario", usuario.NombreUsuario, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Secure = false
        });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("NombreUsuario"); // la guarda LoginController al iniciar sesión
        return RedirectToAction("Index", "Inicio");
    }
}