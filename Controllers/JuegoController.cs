using System.Globalization;
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

        HttpContext.Session.SetString("IdNivel", nivel.Id.ToString());

        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.IdNivel = nivel.Id;
        ViewBag.Nivel = nivel;
        ViewBag.Charla = bd.ObtenerCharlaTeoria(idNivel) ?? new List<CharlaTeoria>();

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

        int idUsuario = bd.ObtenerIdUsuario(nombreUsuario);

        List<int> completados = bd.ObtenerNivelesCompletados(idUsuario);

        if (nivel == null)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        if (nivelElegido > 1 && !completados.Contains(nivelElegido - 1))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        List<Pregunta> todasLasPreguntas = bd.ObtenerPreguntasNivel(nivelElegido);

        // Si se refrescó la página, se retoma la partida con las preguntas guardadas en la session.
        List<Pregunta> preguntas = PreguntasPartidaEnCurso(nivelElegido, todasLasPreguntas);

        if (preguntas == null)
        {
            preguntas = todasLasPreguntas;

            if (preguntas.Count < 8)
            {
                return RedirectToAction("IndexInicio", "Inicio");
            }

            Random random = new Random();

            while (preguntas.Count > 8)
            {
                int aleatorio = random.Next(0, preguntas.Count);
                preguntas.RemoveAt(aleatorio);
            }

            HttpContext.Session.SetString("NivelActual", nivelElegido.ToString());
            HttpContext.Session.SetString("PreguntaActual", "0");
            HttpContext.Session.SetString("Vidas", "3");
            HttpContext.Session.SetString("Puntos", "0");
            HttpContext.Session.SetString("EstadoPartida", "Jugando");
            HttpContext.Session.SetString("InicioPartida", DateTime.UtcNow.ToString("O"));

            for (int i = 0; i < preguntas.Count; i++)
            {
                HttpContext.Session.SetString(
                    "Pregunta" + i,
                    preguntas[i].Id.ToString()
                );
            }
        }

        int preguntaActual = 0;
        int vidas = 3;
        int puntos = 0;
        int segundos = 0;

        int.TryParse(HttpContext.Session.GetString("PreguntaActual"), out preguntaActual);
        int.TryParse(HttpContext.Session.GetString("Vidas"), out vidas);
        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        DateTime inicio;

        if (DateTime.TryParse(
                HttpContext.Session.GetString("InicioPartida"),
                null,
                DateTimeStyles.RoundtripKind,
                out inicio))
        {
            segundos = (int)(DateTime.UtcNow - inicio).TotalSeconds;
        }

        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.PreguntasNivel = preguntas;
        ViewBag.PreguntaActual = preguntaActual;
        ViewBag.Vidas = vidas;
        ViewBag.Puntos = puntos;
        ViewBag.Segundos = segundos;

        return View();
    }

    // Devuelve las preguntas de la partida en curso de este nivel, en el mismo orden
    // en que se guardaron en la session. Si no hay partida para retomar, devuelve null.
    private List<Pregunta> PreguntasPartidaEnCurso(int idNivel, List<Pregunta> todasLasPreguntas)
    {
        if (HttpContext.Session.GetString("EstadoPartida") != "Jugando" ||
            HttpContext.Session.GetString("NivelActual") != idNivel.ToString())
        {
            return null;
        }

        int posicion = 0;

        if (!int.TryParse(HttpContext.Session.GetString("PreguntaActual"), out posicion) ||
            posicion < 0 || posicion >= 8)
        {
            return null;
        }

        List<Pregunta> preguntas = new List<Pregunta>();

        for (int i = 0; i < 8; i++)
        {
            string idPregunta = HttpContext.Session.GetString("Pregunta" + i);
            Pregunta pregunta = todasLasPreguntas.Find(p => p.Id.ToString() == idPregunta);

            if (pregunta == null)
            {
                return null;
            }

            preguntas.Add(pregunta);
        }

        return preguntas;
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

        if (!int.TryParse(posicionTexto, out posicion) || posicion < 0 || posicion >= 8)
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

        int.TryParse(
            HttpContext.Session.GetString("NivelActual"),
            out idNivel
        );

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

        if (estado == "Guardado")
        {
            ViewBag.TituloResultado = "¡Nivel completado!";
            ViewBag.MensajeResultado = "¡Muy bien! Terminaste el desafío.";
        }
        else
        {
            ViewBag.TituloResultado = "¡Te quedaste sin vidas!";
            ViewBag.MensajeResultado = "Podés volver a intentarlo.";
        }

        return View();
    }

    public IActionResult JugarNivel(int idNivel)
    {
         List<Pregunta> todasLasPreguntas = bd.ObtenerPreguntasNivel(idNivel);

         Nivel nivel = new Nivel();
         List<Pregunta> preguntasSeleccionadas = nivel.AgarrarPreguntas(todasLasPreguntas);

        return View(preguntasSeleccionadas);
    }
}
