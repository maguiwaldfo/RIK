using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class LoginController : Controller
{
    private bool HayUsuarioLogueado()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("NombreUsuario"));
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

    [HttpPost]
    public IActionResult ValidarUsuario(string nombre, string email, string nombreUsuario, string contraseña)
    {
        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña, email);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña, usuario.Email))
        {
            ViewBag.ErrorMessage = "Los datos ingresados no son válidos.";
            return View("Registrarse");
        }

        if (BD.FijarseSiExisteUsuario(nombreUsuario))
        {
            ViewBag.ErrorMessage = "El nombre de usuario ya existe. Por favor, elija otro.";
            return View("Registrarse");
        }

        BD.AgregarUsuario(usuario);

        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Email", usuario.Email);

        return RedirectToAction("IndexInicio", "Inicio");
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        if (HayUsuarioLogueado())
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        Usuario usuario = BD.ObtenerUsuario(nombreUsuario, contraseña);

        if (usuario != null)
        {
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            HttpContext.Session.SetString("Email", usuario.Email);

            return RedirectToAction("IndexInicio", "Inicio");
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }
}
