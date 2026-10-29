using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class InicioController : Controller
{
    public IActionResult Landing()
    {
        return View();
    }

    public IActionResult Index()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return View();
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }

    public IActionResult IndexInicio()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            int idUsuario = new BD().ObtenerIdUsuario(nombreUsuario);
            List<int> nivelesCompletados = new BD().ObtenerNivelesCompletados(idUsuario);

            ViewBag.NivelesCompletados = nivelesCompletados;
            ViewBag.MaxNivelDesbloqueado = Math.Min(12, Math.Max(1, nivelesCompletados.Count + 1));

            return View();
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }
}