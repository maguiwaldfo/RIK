using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class LoginController : Controller
{
    BD bd = new BD();

    public IActionResult Index()
    {
        if(/*si la sesión está activa*/)
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
        return View();
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
    public IActionResult ValidarUsuario(string nombre, string apellido, string nombreUsuario, string contraseña, string tipoUsuario)
    {
        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña, apellido, tipoUsuario);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.Apellido, usuario.NombreUsuario, usuario.Contraseña, usuario.TipoUsuario))
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
        HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Apellido", usuario.Apellido);

        return RedirectToAction("IndexInicio", "Inicio");
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        Usuario usuario = bd.ObtenerUsuario(nombreUsuario, contraseña);

        if (usuario != null)
        {
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            HttpContext.Session.SetString("Apellido", usuario.Apellido);
            return RedirectToAction(nameof(Bienvenida));
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }

}