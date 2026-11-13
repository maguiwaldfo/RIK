using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class LigaController : Controller
{
    private BD bd = new BD();

    private string ObtenerUsuarioActual()
    {
        return HttpContext.Session.GetString("NombreUsuario");
    }

    public IActionResult Index(int idLigaAmigos = 0)
{
    string nombreUsuario = ObtenerUsuarioActual();

    if (string.IsNullOrEmpty(nombreUsuario))
    {
        return RedirectToAction("Index", "Login");
    }

    int idUsuario = bd.ObtenerIdUsuarioPorNombre(nombreUsuario);
    if (idUsuario == 0)
    {
        return RedirectToAction("Index", "Login");
    }

    int idLigaSeleccionada = idLigaAmigos;
    if (idLigaSeleccionada == 0)
    {
        idLigaSeleccionada = bd.ObtenerPrimeraLigaDeUsuario(idUsuario);
    }

    List<UsuarioRanking> leaderboardGlobal = bd.ObtenerLeaderboardGlobal();
    List<UsuarioRanking> leaderboardLiga = null;
    Liga ligaActual = null;

    if (idLigaSeleccionada != 0)
    {
        leaderboardLiga = bd.ObtenerLeaderboardLiga(idLigaSeleccionada);
        ligaActual = bd.ObtenerLigaPorId(idLigaSeleccionada);
    }

    // Construcción de la tabla Global en C#
    string htmlGlobal = "<table border='1'><thead><tr><th>Usuario</th><th>Puntaje Total</th></tr></thead><tbody>";
    if (leaderboardGlobal != null && leaderboardGlobal.Count > 0)
    {
        foreach (UsuarioRanking item in leaderboardGlobal)
        {
            htmlGlobal += "<tr><td>" + item.NombreUsuario + " (" + item.Nombre + ")</td><td>" + item.PuntajeTotal + " pts</td></tr>";
        }
    }
    else
    {
        htmlGlobal += "<tr><td colspan='2'>No hay datos registrados en el ranking global.</td></tr>";
    }
    htmlGlobal += "</tbody></table>";

    // Construcción de la tabla Liga y configuración de botones
    string htmlLiga = "";
    string tituloLiga = "Liga de Amigos";

    if (ligaActual != null)
    {
       
        ViewBag.IdLiga = ligaActual.Id; 
        ViewBag.AccionLiga = "Abandonar";
        ViewBag.TextoBotonLiga = "Abandonar liga";

        if (ligaActual.IdUsuarioCreador == idUsuario)
        {
            ViewBag.AccionLiga = "Eliminar";
            ViewBag.TextoBotonLiga = "Eliminar liga";
        }

        tituloLiga = "Liga: " + ligaActual.Nombre;
        htmlLiga = "<table border='1'><thead><tr><th>Usuario</th><th>Puntaje Total</th></tr></thead><tbody>";
        if (leaderboardLiga != null && leaderboardLiga.Count > 0)
        {
            foreach (UsuarioRanking itemLiga in leaderboardLiga)
            {
                htmlLiga += "<tr><td>" + itemLiga.NombreUsuario + " (" + itemLiga.Nombre + ")</td><td>" + itemLiga.PuntajeTotal + " pts</td></tr>";
            }
        }
        else
        {
            htmlLiga += "<tr><td colspan='2'>No hay participantes en esta liga.</td></tr>";
        }
        htmlLiga += "</tbody></table>";
    }
    else
    {
        // Por si la vista intenta leer ViewBag.IdLiga cuando no hay liga
        ViewBag.IdLiga = 0;

        htmlLiga = "<div class='sin-ligas'>" +
                   "<p>No hay ligas. Todavía no pertenecés a ninguna liga de amigos.</p>" +
                   "<a href='/Liga/Crear' class='btn-crear-liga'>Crear liga</a>" +
                   "</div>";
    }

    ViewBag.HtmlTablaGlobal = htmlGlobal;
    ViewBag.TituloLiga = tituloLiga;
    ViewBag.HtmlTablaLiga = htmlLiga;

    return View();
}

    public IActionResult UnirseALiga()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Crear()
    {
        if (string.IsNullOrEmpty(ObtenerUsuarioActual()))
        {
            return RedirectToAction("Index", "Login");
        }

        return View();
    }

    [HttpPost]
    public IActionResult Crear(Liga liga)
    {
        string nombreUsuario = ObtenerUsuarioActual();

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        if (liga.Contraseña == null)
        {
            liga.Contraseña = "";
        }

        int idUsuario = bd.ObtenerIdUsuarioPorNombre(nombreUsuario);
        int idLigaCreada = bd.CrearLiga(liga, idUsuario);
        

        return RedirectToAction("Index", new { idLigaAmigos = idLigaCreada });
    }

    [HttpPost]
public IActionResult Eliminar(int idLiga)
{
    string nombreUsuario = ObtenerUsuarioActual();

    if (nombreUsuario == null)
    {
        return RedirectToAction("Login", "Login");
    }

    int idUsuario = bd.ObtenerIdUsuarioPorNombre(nombreUsuario);

    Liga liga = bd.ObtenerLigaPorId(idLiga);

    if (liga == null || liga.IdUsuarioCreador != idUsuario)
    {
        return RedirectToAction("Index");
    }

    bd.EliminarLiga(idLiga, idUsuario);

    return RedirectToAction("Index");
}

[HttpPost]
public IActionResult Abandonar(int idLiga)
{
    string nombreUsuario = ObtenerUsuarioActual();

    if (nombreUsuario == null)
    {
        return RedirectToAction("Login", "Login");
    }

    int idUsuario = bd.ObtenerIdUsuarioPorNombre(nombreUsuario);

    Liga liga = bd.ObtenerLigaPorId(idLiga);

    if (liga == null || liga.IdUsuarioCreador == idUsuario)
    {
        return RedirectToAction("Index");
    }

    bd.AbandonarLiga(idLiga, idUsuario);

    return RedirectToAction("Index");
}
}