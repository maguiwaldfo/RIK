using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
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

    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy Policy";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Id del pedido que falló, para poder buscarlo (viene de la plantilla de Visual Studio)
        string requestId = HttpContext.TraceIdentifier;

        if (Activity.Current != null)
        {
            requestId = Activity.Current.Id;
        }

        ViewData["Title"] = "Error";
        ViewBag.RequestId = requestId;
        ViewBag.OcultarRequestId = string.IsNullOrEmpty(requestId);

        return View();
    }
}
