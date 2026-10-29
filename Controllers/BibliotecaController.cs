using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class BibliotecaController : Controller
{
    BD bd = new BD();
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult verTeoria(int idNivel)
    {
        Nivel nivel = bd.ObtenerNivel(idNivel);

        if (nivel == null)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        
        ViewBag.Charla = bd.ObtenerCharlaTeoria(idNivel);

        return View();
    }
    
}