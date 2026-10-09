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
}