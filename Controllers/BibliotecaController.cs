using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class BibliotecaController : Controller
{
    public IActionResult Index()
    {
        if(string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("Landing", "Login");
        }
        else
        {
            return View();
        }
    }

    
}