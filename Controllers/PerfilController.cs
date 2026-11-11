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

    public IActionResult CambiarContrasena()
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
    public IActionResult ModificarDatos(string nombre, string email, string nombreUsuario)
    {
        string nombreUsuarioActual = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuarioActual))
        {
            return RedirectToAction("Landing", "Inicio");
        }

        email = email?.Trim();
        Usuario usuario = new Usuario(nombre, nombreUsuario, null, email);

        BD bd = new BD();
        int idUsuario = bd.ObtenerIdUsuario(nombreUsuarioActual);

        int resultado = bd.ModificarUsuario(idUsuario, usuario);

        if (resultado == 0)
        {
            ViewBag.ErrorMessage = "El nombre de usuario ya existe.";
            return View("ModificarMisDatos");
        }

        if (resultado == -1)
        {
           ViewBag.ErrorMessage = "No se encontró el usuario.";
           return View("ModificarMisDatos");
        }

        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Email", usuario.Email);
        TempData["Mensaje"] = "Tus datos fueron modificados correctamente.";

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Landing", "Inicio");
    }

    [HttpPost]
    public IActionResult CambiarContrasena(string contrasenaActual, string nuevaContrasena, string repetirContrasena)
    {
        string nombreUsuarioActual = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuarioActual))
        {
            return RedirectToAction("Landing", "Inicio");
        }

        if (string.IsNullOrWhiteSpace(contrasenaActual) ||
            string.IsNullOrWhiteSpace(nuevaContrasena) ||
            string.IsNullOrWhiteSpace(repetirContrasena))
        {
            ViewBag.ErrorMessage = "Debés completar la contraseña actual, la nueva y repetirla.";
            return View();
        }

        if (nuevaContrasena.Length < 8)
        {
            ViewBag.ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.";
            return View();
        }

        if (!string.Equals(nuevaContrasena, repetirContrasena, StringComparison.Ordinal))
        {
            ViewBag.ErrorMessage = "La nueva contraseña y la confirmación no coinciden.";
            return View();
        }

        BD bd = new BD();
        int idUsuario = bd.ObtenerIdUsuario(nombreUsuarioActual);

        if (idUsuario == 0)
        {
            ViewBag.ErrorMessage = "No se encontró el usuario.";
            return View();
        }

        Usuario usuarioActual = bd.ObtenerUsuario(nombreUsuarioActual, contrasenaActual);

        if (usuarioActual == null)
        {
            ViewBag.ErrorMessage = "La contraseña actual es incorrecta.";
            return View();
        }

        int resultado = bd.CambiarContrasena(idUsuario, contrasenaActual, nuevaContrasena, repetirContrasena);

        if (resultado == 0)
        {
            ViewBag.ErrorMessage = "La contraseña actual es incorrecta.";
            return View();
        }

        if (resultado == -1)
        {
            ViewBag.ErrorMessage = "La nueva contraseña y la confirmación no coinciden.";
            return View();
        }

        TempData["Mensaje"] = "Tu contraseña fue modificada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}