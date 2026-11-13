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

        ViewData["Title"] = nivel.Tema;
        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.IdNivel = nivel.Id;
        ViewBag.Nivel = nivel;
        ViewBag.Charla = bd.ObtenerCharlaTeoria(idNivel) ?? new List<CharlaTeoria>();

        CargarImagenesPersonajes();

        return View();
    }

    // Imágenes de los personajes: caras para el resumen final, medio cuerpo para el paso a paso.
    private void CargarImagenesPersonajes()
    {
        ViewBag.CaraChico = Url.Content("~/imagenes/Cara%20chico.png");
        ViewBag.CaraPerro = Url.Content("~/imagenes/Cara%20riky.png");
        ViewBag.CuerpoChico = Url.Content("~/imagenes/Chico%20confundido%20con%20gesto%20explicativo%203.png");
        ViewBag.CuerpoPerro = Url.Content("~/imagenes/Perrito_Shiba_sonriente_saludando-removebg-preview%201.png");
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

        int.TryParse(HttpContext.Session.GetString("PreguntaActual"), out preguntaActual);
        int.TryParse(HttpContext.Session.GetString("Vidas"), out vidas);
        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.PreguntasNivel = preguntas;
        ViewBag.PreguntaActual = preguntaActual;
        ViewBag.Progreso = CalcularProgreso(preguntaActual, preguntas.Count);
        ViewBag.Vidas = vidas;
        ViewBag.Puntos = puntos;
        ViewBag.Segundos = SegundosDePartida();

        return View();
    }

    // Porcentaje de la barra de progreso: preguntas ya acertadas sobre el total.
    private int CalcularProgreso(int preguntaActual, int totalPreguntas)
    {
        if (totalPreguntas == 0)
        {
            return 0;
        }

        return preguntaActual * 100 / totalPreguntas;
    }

    // Segundos que pasaron desde que empezó la partida (se guardó en la session "InicioPartida").
    private int SegundosDePartida()
    {
        DateTime inicio;

        if (DateTime.TryParse(
                HttpContext.Session.GetString("InicioPartida"),
                null,
                DateTimeStyles.RoundtripKind,
                out inicio))
        {
            return (int)(DateTime.UtcNow - inicio).TotalSeconds;
        }

        return 0;
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

        bool gano = estado == "Guardado";

        int puntos = LeerNumeroSession("Puntos");
        int vidas = LeerNumeroSession("Vidas");

        ViewBag.Gano = gano;
        ViewBag.ImagenMascota = gano
            ? Url.Content("~/imagenes/Ricky%20pantalla%20ganaste.png")
            : Url.Content("~/imagenes/Ricky%20pantalla%20perdiste.png");
        ViewBag.Xp = CalcularXp(gano, puntos);
        ViewBag.Aciertos = CalcularAciertos(puntos, vidas);
        ViewBag.Tiempo = FormatearTiempo(SegundosDePartida());
        ViewBag.IdNivel = LeerNumeroSession("NivelActual");

        return View();
    }

    // Lee un número guardado en la session. Si no está o no es un número, devuelve 0.
    private int LeerNumeroSession(string clave)
    {
        int numero = 0;

        int.TryParse(HttpContext.Session.GetString(clave), out numero);

        return numero;
    }

    // Si se pierde el nivel no se guarda, así que no se gana XP.
    private int CalcularXp(bool gano, int puntos)
    {
        if (gano)
        {
            return puntos;
        }

        return 0;
    }

    // Aciertos: respuestas correctas sobre todos los intentos
    // (cada acierto suma 100 puntos y cada fallo saca una de las 3 vidas).
    private int CalcularAciertos(int puntos, int vidas)
    {
        int correctas = puntos / 100;
        int fallos = Math.Max(0, 3 - vidas);
        int intentos = correctas + fallos;

        if (intentos == 0)
        {
            return 0;
        }

        return (int)Math.Round(correctas * 100.0 / intentos);
    }

    // Pasa los segundos a "m:ss", sin el cero de adelante (por ejemplo "1:05").
    private string FormatearTiempo(int segundos)
    {
        int minutos = segundos / 60;
        int resto = segundos % 60;

        return minutos + ":" + resto.ToString("00");
    }

    public IActionResult JugarNivel(int idNivel)
    {
         List<Pregunta> todasLasPreguntas = bd.ObtenerPreguntasNivel(idNivel);

         Nivel nivel = new Nivel();
         List<Pregunta> preguntasSeleccionadas = nivel.AgarrarPreguntas(todasLasPreguntas);

        return View(preguntasSeleccionadas);
    }
}
