
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

        ViewBag.Nivel = nivel;
        ViewBag.Charla = bd.ObtenerCharlaTeoria(idNivel);

        return View();
    }

    public IActionResult Nivel(int nivelElegido)
    {
        Response.Cookies.Delete("IdNivel");

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

        int idUsuario = bd.ObtenerIdUsuario(nombreUsuario);
        List<int> completados = bd.ObtenerNivelesCompletados(idUsuario);

        if (nivelElegido > 1 && !completados.Contains(nivelElegido - 1))
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

        HttpContext.Session.SetString("NivelActual", nivelElegido.ToString());
        HttpContext.Session.SetString("PreguntaActual", "0");
        HttpContext.Session.SetString("Vidas", "3");
        HttpContext.Session.SetString("Puntos", "0");
        HttpContext.Session.SetString("EstadoPartida", "Jugando");
        HttpContext.Session.SetString("InicioPartida", DateTime.UtcNow.ToString("O"));

        for (int i = 0; i < preguntas.Count; i++)
        {
            HttpContext.Session.SetString("Pregunta" + i, preguntas[i].Id.ToString());
        }

        ViewBag.NumeroNivel = nivel;
        ViewBag.PreguntasNivel = preguntas;

        return View();
    }

    [HttpPost]
    public IActionResult ComprobarRespuesta(int idPregunta, int idOpcion)
    {
        string estado = HttpContext.Session.GetString("EstadoPartida");

        if (estado != "Jugando")
        {
            return BadRequest("PartidaInexistente");
        }

        string posicionTexto = HttpContext.Session.GetString("PreguntaActual");
        int posicion = 0;

        if (!int.TryParse(posicionTexto, out posicion) || posicion >= 8)
        {
            return BadRequest("PreguntaInvalida");
        }

        string idEsperado = HttpContext.Session.GetString("Pregunta" + posicion);

        if (idEsperado != idPregunta.ToString())
        {
            return BadRequest("PreguntaInvalida");
        }

        Pregunta pregunta = bd.ObtenerPregunta(idPregunta);

        if (pregunta == null)
        {
            return BadRequest("PreguntaInvalida");
        }

        int vidas = 0;
        int puntos = 0;

        int.TryParse(HttpContext.Session.GetString("Vidas"), out vidas);
        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        if (bd.ComprobarOpcion(idPregunta, idOpcion))
        {
            puntos += 100;
            posicion++;

            HttpContext.Session.SetString("Puntos", puntos.ToString());
            HttpContext.Session.SetString("PreguntaActual", posicion.ToString());

            if (posicion == 8)
            {
                HttpContext.Session.SetString("EstadoPartida", "Ganado");
            }

            return Ok("Correcta");
        }
        else
        {
            vidas--;

            HttpContext.Session.SetString("Vidas", vidas.ToString());

            if (vidas == 0)
            {
                HttpContext.Session.SetString("EstadoPartida", "Perdido");
                return Ok("SinVidas");
            }

            return Ok("Incorrecta");
        }
    }

    [HttpPost]
    public IActionResult FinalizarNivel()
    {
        string estado = HttpContext.Session.GetString("EstadoPartida");
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");

        if (estado != "Ganado" || string.IsNullOrEmpty(nombreUsuario))
        {
            return BadRequest("PartidaIncompleta");
        }

        int idNivel = 0;

        foreach (Nivel nivel in bd.ObtenerNiveles())
        {
            if (nivel.Id.ToString() == HttpContext.Session.GetString("NivelActual"))
            {
                idNivel = nivel.Id;
            }
        }

        if (idNivel == 0)
        {
            return BadRequest("NivelInvalido");
        }

        int idUsuario = bd.ObtenerIdUsuario(nombreUsuario);

        if (idUsuario == 0)
        {
            return BadRequest("UsuarioInvalido");
        }

        int puntos = 0;
        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        bd.GuardarNivelCompletado(idUsuario, idNivel, puntos);

        HttpContext.Session.SetString("EstadoPartida", "Guardado");

        return Ok("Completado");
    }

    public IActionResult Resultado()
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
        string estado = HttpContext.Session.GetString("EstadoPartida");

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        if (estado != "Guardado" && estado != "Perdido")
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        DateTime inicio = DateTime.UtcNow;

        DateTime.TryParse(
            HttpContext.Session.GetString("InicioPartida"),
            out inicio
        );

        TimeSpan tiempo = DateTime.UtcNow - inicio;

        ViewBag.Tiempo = tiempo.ToString(@"mm\:ss");
        ViewBag.Vidas = HttpContext.Session.GetString("Vidas");
        ViewBag.Puntos = HttpContext.Session.GetString("Puntos");
        ViewBag.Estado = estado;

        return View();
    }
}
