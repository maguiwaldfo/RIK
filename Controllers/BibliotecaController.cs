using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class BibliotecaController : Controller
{
    BD bd = new BD();

    public IActionResult Index()
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        ViewBag.Teorias = bd.ObtenerTeorias();

        return View();
    }
    public IActionResult TeoriaMoraleja(int IdTeoria)
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        ViewBag.Teoria = bd.ObtenerTeoriaMoraleja(IdTeoria);

        return View();
    }
}