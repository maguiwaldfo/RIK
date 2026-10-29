using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class LoginController : Controller
{
    BD bd = new BD();

    private bool HayUsuarioLogueado()
    {
        return !string.IsNullOrWhiteSpace(Request.Cookies["NombreUsuario"]);
    }

    private void GuardarUsuarioCookie(string nombreUsuario)
    {
        Response.Cookies.Append("NombreUsuario", nombreUsuario, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Secure = false
        });
    }

    private void BorrarUsuarioCookie()
    {
        Response.Cookies.Delete("NombreUsuario");
    }

    public IActionResult Landing()
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        return RedirectToAction("Landing", "Inicio");
    }

    public IActionResult Index()
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        return View();
    }

    public IActionResult Registrarse()
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        return View();
    }

    public IActionResult IrAInicio()
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ValidarUsuario(string nombre, string email, string nombreUsuario, string contraseña)
    {
        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña, email);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña, usuario.Email))
        {
            ViewBag.ErrorMessage = "Los datos ingresados no son válidos.";
            return View("Registrarse");
        }

        if (bd.FijarseSiExisteUsuario(nombreUsuario))
        {
            ViewBag.ErrorMessage = "El nombre de usuario ya existe. Por favor, elija otro.";
            return View("Registrarse");
        }

        bd.AgregarUsuario(usuario);

        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Email", usuario.Email);
        GuardarUsuarioCookie(usuario.NombreUsuario);

        return RedirectToAction("IndexInicio", "Inicio");
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        Usuario usuario = bd.ObtenerUsuario(nombreUsuario, contraseña);

        if (usuario != null)
        {
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            HttpContext.Session.SetString("Email", usuario.Email);
            GuardarUsuarioCookie(usuario.NombreUsuario);

            return RedirectToAction("IndexInicio", "Inicio");
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }
}