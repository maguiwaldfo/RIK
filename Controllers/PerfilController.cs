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
    public IActionResult ModificarDatos(string nombre, string email, string nombreUsuario, string contraseña)
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
}