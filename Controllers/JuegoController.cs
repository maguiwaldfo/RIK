using Microsoft.AspNetCore.Mvc;
using RIK.Models;
using System.Text.Json;

namespace RIK.Controllers;

public class JuegoController : Controller
{
    BD bd = new BD();
    
    public IActionResult Teoria(int teoriaElegida)
    {
        ViewBag teoriaNivel = bd.TeoriaNivel(teoriaElegida);
        return View();
    }

    public IActionResult Nivel(int nivelElegido)
    {
        ViewBag NumeroNivel = bd.ObtenerNivel(nivelElegido);
        return View();
    }

}
