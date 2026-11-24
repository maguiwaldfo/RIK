using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class BibliotecaController : Controller
{
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

        List<Teoria> teorias = BD.ObtenerTeorias();

        // Link e imagen de cada tarjeta
        foreach (Teoria teoria in teorias)
        {
            teoria.Url = Url.Action("TeoriaMoraleja", "Biblioteca", new { idTeoria = teoria.Id });
            teoria.Imagen = "/imagenes/teoria/" + teoria.NombreImagen;
            teoria.OcultarBloqueada = !teoria.EsBloqueada;
        }

        ViewData["Title"] = "Biblioteca";
        ViewBag.Teorias = teorias;
        ViewBag.OcultarSinTeorias = teorias.Count > 0;

        return View();
    }

    public IActionResult TeoriaMoraleja(int idTeoria)
    {
        if (string.IsNullOrEmpty(ObtenerUsuarioActual()))
        {
            return RedirectToAction("Index", "Login");
        }

        List<TeoriaMoraleja> lista = BD.ObtenerTeoriaMoraleja(idTeoria);
        TeoriaMoraleja detalle = null;

        if (lista.Count > 0)
        {
            detalle = lista[0];
        }

        int idUsuario = BD.ObtenerIdUsuario(ObtenerUsuarioActual());
        List<int> nivelesCompletados = BD.ObtenerNivelesCompletados(idUsuario);

        // La teoría tiene el mismo número que su nivel
        bool estaDesbloqueado = Nivel.EstaDesbloqueado(idTeoria, nivelesCompletados);

        bool encontrada = false;

        if (detalle != null)
        {
            encontrada = true;
        }

        ViewData["Title"] = "Teoria y Moraleja";

        // Textos de la teoría (vacíos si no se encontró)
        ViewBag.Titulo = "";
        ViewBag.TituloTeoria = "";
        ViewBag.TextoTeoria = "";
        ViewBag.TituloHistoria = "";
        ViewBag.Historia = "";
        ViewBag.Moraleja = "";

        if (detalle != null)
        {
            ViewBag.Titulo = detalle.Titulo;
            ViewBag.TituloTeoria = detalle.TituloTeoria;
            ViewBag.TextoTeoria = detalle.Teoria;
            ViewBag.TituloHistoria = detalle.TituloHistoria;
            ViewBag.Historia = detalle.Historia;
            ViewBag.Moraleja = detalle.Moraleja;
        }

        // Qué se muestra: la teoría o el aviso de "no se encontró";
        // y el botón "Iniciar nivel" o el aviso de nivel bloqueado
        ViewBag.OcultarTeoria = !encontrada;
        ViewBag.OcultarNoEncontrada = encontrada;
        ViewBag.OcultarIniciarNivel = !(encontrada && estaDesbloqueado);
        ViewBag.OcultarBloqueado = !(encontrada && !estaDesbloqueado);
        ViewBag.UrlIniciarNivel = Url.Action("Teoria", "Juego", new { idNivel = idTeoria });

        return View();
    }
}