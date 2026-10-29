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
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Inicio");
    }

}