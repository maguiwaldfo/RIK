using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class BibliotecaController : Controller
{
    private BD bd = new BD();

    private string ObtenerUsuarioActual()
    {
        return HttpContext.Session.GetString("NombreUsuario");
    }

    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(ObtenerUsuarioActual()))
        {
            return RedirectToAction("Index", "Login");
        }

        List<Teoria> teorias = bd.ObtenerTeorias();
        ViewBag.Teorias = teorias;

        return View();
    }

    public IActionResult TeoriaMoraleja(int idTeoria)
{
    if (string.IsNullOrEmpty(ObtenerUsuarioActual()))
    {
        return RedirectToAction("Index", "Login");
    }

    List<TeoriaMoraleja> lista = bd.ObtenerTeoriaMoraleja(idTeoria);
    TeoriaMoraleja detalle = null;

    if (lista.Count > 0)
    {
        detalle = lista[0];
    }

    ViewBag.TeoriaMoraleja = detalle;

    return View();
}
}