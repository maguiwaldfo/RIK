using Microsoft.AspNetCore.Mvc;
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
            CargarDatosUsuario();
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
            ViewBag.ErrorMessage = "";
            ViewBag.OcultarError = true;
            return View();
        }
    }

    // Datos del usuario para el formulario de "Mis datos"
    private void CargarDatosUsuario()
    {
        ViewBag.Nombre = HttpContext.Session.GetString("Nombre");
        ViewBag.Email = HttpContext.Session.GetString("Email");
        ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");
    }


    [HttpPost]
    public IActionResult ModificarDatos(string nombre, string email, string nombreUsuario)
    {
        string nombreUsuarioActual = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuarioActual))
        {
            return RedirectToAction("Landing", "Inicio");
        }

        // Si hay un error se vuelve a mostrar el formulario con los datos actuales
        CargarDatosUsuario();

        if (email != null)
        {
            email = email.Trim();
        }
        Usuario usuario = new Usuario(nombre, nombreUsuario, null, email);

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.NombreUsuario, usuario.Contraseña, usuario.Email, contraseñaOpcional: true))
        {
            ViewBag.ErrorMessage = "Los datos ingresados no son válidos.";
            return View("ModificarMisDatos");
        }

        int idUsuario = BD.ObtenerIdUsuario(nombreUsuarioActual);

        int resultado = BD.ModificarUsuario(idUsuario, usuario);

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

        return RedirectToAction("Index");
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

        // Si esta acción vuelve a mostrar la vista es porque hubo un error: el mensaje se muestra
        ViewBag.OcultarError = false;

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

        if (nuevaContrasena.Length < 8 || nuevaContrasena.Length > 50)
        {
            ViewBag.ErrorMessage = "La nueva contraseña debe tener entre 8 y 50 caracteres.";
            return View();
        }

        if (nuevaContrasena != repetirContrasena)
        {
            ViewBag.ErrorMessage = "La nueva contraseña y la confirmación no coinciden.";
            return View();
        }

        int idUsuario = BD.ObtenerIdUsuario(nombreUsuarioActual);

        if (idUsuario == 0)
        {
            ViewBag.ErrorMessage = "No se encontró el usuario.";
            return View();
        }

        // El procedure comprueba la contraseña actual: devuelve 0 si es incorrecta
        int resultado = BD.CambiarContrasena(idUsuario, contrasenaActual, nuevaContrasena, repetirContrasena);

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

        return RedirectToAction("Index");
    }
}
