using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class JuegoController : Controller
{
    BD bd = new BD();

 public IActionResult Teoria(int idNivel)
{
    Nivel nivel = bd.ObtenerNivel(idNivel);

    if (nivel == null)
    {
        return RedirectToAction("IndexInicio", "Inicio");
    }

    List<CharlaTeoria> charla = bd.ObtenerCharlaTeoria(idNivel);

    ViewBag.Nivel = nivel;
    ViewBag.Charla = charla;

    return View();
}

    public IActionResult Nivel(int nivelElegido)
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        Nivel nivel = bd.ObtenerNivel(nivelElegido);

        if (nivel == null)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        List<Pregunta> preguntas = nivel.agarrarPreguntas(
            bd.ObtenerPreguntasNivel(nivelElegido)
        );

        if (preguntas.Count != 8)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        HttpContext.Session.SetInt32("NivelActual", nivelElegido);
        HttpContext.Session.SetInt32("PreguntaActual", 0);

        for (int i = 0; i < preguntas.Count; i++)
        {
            HttpContext.Session.SetInt32("Pregunta" + i, preguntas[i].Id);
        }

        ViewBag.NumeroNivel = nivel;
        ViewBag.PreguntasNivel = preguntas;

        return View();
    }

    [HttpPost]
    public IActionResult ComprobarRespuesta(int idPregunta, string respuestaElegida)
    {
        int posicion = HttpContext.Session.GetInt32("PreguntaActual").GetValueOrDefault();
        int preguntaEsperada = HttpContext.Session.GetInt32("Pregunta" + posicion).GetValueOrDefault();

        if (preguntaEsperada == 0 || idPregunta != preguntaEsperada)
        {
            return BadRequest("PreguntaInvalida");
        }

        if (respuestaElegida != "1" &&
            respuestaElegida != "2" &&
            respuestaElegida != "3" &&
            respuestaElegida != "4")
        {
            return BadRequest("RespuestaInvalida");
        }

        Pregunta pregunta = bd.ObtenerPregunta(idPregunta);

        if (pregunta == null)
        {
            return BadRequest("PreguntaInvalida");
        }

        HttpContext.Session.SetInt32("PreguntaActual", posicion + 1);

        if (pregunta.Respuesta.ToString() == respuestaElegida)
        {
            return Ok("Correcta");
        }

        return Ok("Incorrecta");
    }

    [HttpPost]
    public IActionResult FinalizarNivel()
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
        int respondidas = HttpContext.Session.GetInt32("PreguntaActual").GetValueOrDefault();
        int idNivel = HttpContext.Session.GetInt32("NivelActual").GetValueOrDefault();

        if (string.IsNullOrEmpty(nombreUsuario) || respondidas != 8 || idNivel == 0)
        {
            return BadRequest("PartidaIncompleta");
        }

        int idUsuario = bd.ObtenerIdUsuario(nombreUsuario);

        if (idUsuario == 0)
        {
            return BadRequest("UsuarioInvalido");
        }

        bd.GuardarNivelCompletado(idUsuario, idNivel);

        HttpContext.Session.Remove("NivelActual");
        HttpContext.Session.Remove("PreguntaActual");

        for (int i = 0; i < 8; i++)
        {
            HttpContext.Session.Remove("Pregunta" + i);
        }

        return Ok("Completado");
    }
}
