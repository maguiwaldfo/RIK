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
    public IActionResult ValidarUsuario(string nombre, string email, string nombreUsuario, string contraseña)
    {
        Console.WriteLine("ValidarUsuario: nombre=" + nombre + ", email=" + email + ", nombreUsuario=" + nombreUsuario);

        Usuario usuario = new Usuario(nombre, nombreUsuario, contraseña, email);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña, usuario.Email))
        {
            Console.WriteLine("Validación de registro fallida");
            ViewBag.ErrorMessage = "Los datos ingresados no son válidos.";
            return View("Registrarse");
        }

        if (bd.FijarseSiExisteUsuario(nombreUsuario))
        {
            Console.WriteLine("Usuario duplicado: " + nombreUsuario);
            ViewBag.ErrorMessage = "El nombre de usuario ya existe. Por favor, elija otro.";
            return View("Registrarse");
        }

        bd.AgregarUsuario(usuario);
        Console.WriteLine("Usuario agregado correctamente: " + usuario.NombreUsuario);

        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        Console.WriteLine("Sesión creada en registro: " + HttpContext.Session.GetString("NombreUsuario"));

        return RedirectToAction("IndexInicio", "Inicio");
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        Console.WriteLine("IniciarSesion: nombreUsuario=" + nombreUsuario + ", contraseña=" + contraseña);

        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            Console.WriteLine("Ya hay sesión activa: " + HttpContext.Session.GetString("NombreUsuario"));
            return RedirectToAction("IndexInicio", "Inicio");
        }

        Usuario usuario = bd.ObtenerUsuario(nombreUsuario, contraseña);
        Console.WriteLine("Usuario encontrado: " + (usuario == null ? "null" : usuario.NombreUsuario));

        if (usuario != null)
        {
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            Console.WriteLine("Sesión seteada: " + HttpContext.Session.GetString("NombreUsuario"));
            return RedirectToAction("IndexInicio", "Inicio");
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }
}