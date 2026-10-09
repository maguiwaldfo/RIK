using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class LoginController : Controller
{
    BD bd = new BD();

    public IActionResult Landing()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            return RedirectToAction("Landing", "Inicio");
        }
    }

    public IActionResult Index()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            return View();
        }
    }

    public IActionResult Registrarse()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            return View();
        }
    }

    public IActionResult IrAInicio()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }
    }
    
    /*ESTE CIERRA SESIÓN (VA EN LA CONFIGURACIÓN)*/
    [HttpPost]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ValidarUsuario(string nombre, string nombreUsuario, string contraseña)
    {
        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña))
        {
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

        return RedirectToAction("IndexInicio", "Inicio");
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        Usuario usuario = bd.ObtenerUsuario(nombreUsuario, contraseña);

        if (usuario != null)
        {
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            return RedirectToAction("IndexInicio", "Inicio");
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }

}