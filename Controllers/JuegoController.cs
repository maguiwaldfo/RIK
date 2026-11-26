using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class JuegoController : Controller
{
    // Cantidad de preguntas de cada partida (está en Models/Nivel.cs).
    // Acá se escribe RIK.Models.Nivel porque dentro de este controller "Nivel" es la acción Nivel(...).
    private const int PreguntasPorPartida = RIK.Models.Nivel.PreguntasPorPartida;

    public IActionResult Teoria(int idNivel)
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        Nivel nivel = BD.ObtenerNivel(idNivel);

        if (nivel == null)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        int idUsuario = BD.ObtenerIdUsuario(nombreUsuario);
        List<int> completados = BD.ObtenerNivelesCompletados(idUsuario);

        if (!RIK.Models.Nivel.EstaDesbloqueado(idNivel, completados))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        HttpContext.Session.SetString("IdNivel", nivel.Id.ToString());

        ViewData["Title"] = nivel.Tema;
        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.IdNivel = nivel.Id;
        ViewBag.UrlComenzar = Url.Action("Nivel", "Juego", new { nivelElegido = nivel.Id });

        // "Comenzar desafío" solo se muestra si el nivel tiene preguntas suficientes para jugarlo.
        int cantidadPreguntas = BD.ContarPreguntasNivel(idNivel);
        ViewBag.OcultarComenzar = cantidadPreguntas < PreguntasPorPartida;

        CargarImagenesPersonajes();

        // Cada mensaje sabe si lo dice el chico o el perro
        List<CharlaTeoria> charla = BD.ObtenerCharlaTeoria(idNivel);

        foreach (CharlaTeoria mensaje in charla)
        {
            string personaje = "";

            if (mensaje.Personaje != null)
            {
                personaje = mensaje.Personaje.Trim().ToLower();
            }

            if (personaje == "chico")
            {
                mensaje.ClaseFila = "charla-fila charla-fila-chico";
                mensaje.CodigoPersonaje = "chico";
                mensaje.ImagenCara = ViewBag.CaraChico;
            }
            else
            {
                mensaje.ClaseFila = "charla-fila charla-fila-perro";
                mensaje.CodigoPersonaje = "perro";
                mensaje.ImagenCara = ViewBag.CaraPerro;
            }
        }

        ViewBag.Charla = charla;

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

        // Que el navegador no guarde esta página: así, al volver con "Atrás" desde otra
        // vista, se vuelve a pedir al servidor y la partida empieza de cero.
        Response.Headers["Cache-Control"] = "no-store";

        Nivel nivel = BD.ObtenerNivel(nivelElegido);

        int idUsuario = BD.ObtenerIdUsuario(nombreUsuario);

        List<int> completados = BD.ObtenerNivelesCompletados(idUsuario);

        if (nivel == null)
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        if (!RIK.Models.Nivel.EstaDesbloqueado(nivelElegido, completados))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }

        List<Pregunta> todasLasPreguntas = BD.ObtenerPreguntasNivel(nivelElegido);

        // Si se refrescó la página, se retoma la partida con las preguntas guardadas en la session.
        List<Pregunta> preguntas = PreguntasPartidaEnCurso(nivelElegido, todasLasPreguntas);

        if (preguntas == null)
        {
            preguntas = todasLasPreguntas;

            if (preguntas.Count < PreguntasPorPartida)
            {
                return RedirectToAction("IndexInicio", "Inicio");
            }

            Random random = new Random();

            // Se sacan preguntas al azar hasta que queden las de una partida
            while (preguntas.Count > PreguntasPorPartida)
            {
                int aleatorio = random.Next(0, preguntas.Count);
                preguntas.RemoveAt(aleatorio);
            }

            HttpContext.Session.SetString("NivelActual", nivelElegido.ToString());
            HttpContext.Session.SetString("PreguntaActual", "0");
            HttpContext.Session.SetString("Vidas", "3");
            HttpContext.Session.SetString("Puntos", "0");
            HttpContext.Session.SetString("EstadoPartida", "Jugando");
            HttpContext.Session.SetString("InicioPartida", DateTime.Now.ToString());
            HttpContext.Session.Remove("DuracionPartida");

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

        // id de cada pregunta en la vista ("pregunta-0", "pregunta-1"...): lo usa nivel.js
        for (int i = 0; i < preguntas.Count; i++)
        {
            preguntas[i].IdHtml = "pregunta-" + i;
        }

        ViewData["Title"] = nivel.Tema;
        ViewBag.TemaNivel = nivel.Tema;
        ViewBag.PreguntasNivel = preguntas;
        ViewBag.PreguntaActual = preguntaActual;
        ViewBag.EstiloBarra = "width: " + CalcularProgreso(preguntaActual, preguntas.Count) + "%;";
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

    // Durante la partida calcula los segundos; al terminar devuelve la duración guardada.
    private int SegundosDePartida()
    {
        if (HttpContext.Session.GetString("DuracionPartida") != null)
        {
            return LeerNumeroSession("DuracionPartida");
        }

        DateTime inicio;

        if (DateTime.TryParse(HttpContext.Session.GetString("InicioPartida"), out inicio))
        {
            TimeSpan duracion = DateTime.Now - inicio;
            return (int)duracion.TotalSeconds;
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
            posicion < 0 || posicion >= PreguntasPorPartida)
        {
            return null;
        }

        List<Pregunta> preguntas = new List<Pregunta>();

        for (int i = 0; i < PreguntasPorPartida; i++)
        {
            string idPregunta = HttpContext.Session.GetString("Pregunta" + i);
            Pregunta pregunta = BuscarPregunta(todasLasPreguntas, idPregunta);

            if (pregunta == null)
            {
                return null;
            }

            preguntas.Add(pregunta);
        }

        return preguntas;
    }

    // Busca en la lista la pregunta con ese id (guardado como texto en la session).
    // Si no está, devuelve null.
    private Pregunta BuscarPregunta(List<Pregunta> preguntas, string idPregunta)
    {
        Pregunta encontrada = null;

        foreach (Pregunta pregunta in preguntas)
        {
            if (pregunta.Id.ToString() == idPregunta)
            {
                encontrada = pregunta;
            }
        }

        return encontrada;
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

        if (!int.TryParse(posicionTexto, out posicion) || posicion < 0 || posicion >= PreguntasPorPartida)
        {
            return BadRequest("PreguntaInvalida");
        }

        string idEsperado = HttpContext.Session.GetString("Pregunta" + posicion);

        if (idEsperado != idPregunta.ToString())
        {
            return BadRequest("PreguntaInvalida");
        }

        Pregunta pregunta = BD.ObtenerPregunta(idPregunta);

        if (pregunta == null)
        {
            return BadRequest("PreguntaInvalida");
        }

        int vidas = 0;
        int puntos = 0;

        int.TryParse(HttpContext.Session.GetString("Vidas"), out vidas);
        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        if (BD.ComprobarOpcion(idPregunta, idOpcion))
        {
            puntos += 100;
            posicion++;

            HttpContext.Session.SetString("Puntos", puntos.ToString());
            HttpContext.Session.SetString("PreguntaActual", posicion.ToString());

            if (posicion == PreguntasPorPartida)
            {
                HttpContext.Session.SetString("DuracionPartida", SegundosDePartida().ToString());
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
                HttpContext.Session.SetString("DuracionPartida", SegundosDePartida().ToString());
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

        int idUsuario = BD.ObtenerIdUsuario(nombreUsuario);

        if (idUsuario == 0)
        {
            return BadRequest("UsuarioInvalido");
        }

        int puntos = 0;

        int.TryParse(HttpContext.Session.GetString("Puntos"), out puntos);

        BD.GuardarNivelCompletado(idUsuario, idNivel, puntos);
        BD.ActualizarRacha(idUsuario);

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

        bool gano = false;

        if (estado == "Guardado")
        {
            gano = true;
        }

        int puntos = LeerNumeroSession("Puntos");
        int vidas = LeerNumeroSession("Vidas");

        ViewBag.Gano = gano;
        ViewBag.Xp = CalcularXp(gano, puntos);
        ViewBag.TextoAciertos = CalcularAciertos(puntos, vidas) + "%";
        ViewBag.Tiempo = FormatearTiempo(SegundosDePartida());

        int idNivel = LeerNumeroSession("NivelActual");

        ViewData["Title"] = "Resultado";

        // Textos según si ganó o perdió
        if (gano)
        {
            ViewBag.ClaseFin = "fin fin-gano";
            ViewBag.MensajeFin = "¡Pudiste aprender un tema nuevo! ¡Felicidades!";
            ViewBag.AltMascota = "Riky festejando";
            ViewBag.ImagenMascota = Url.Content("~/imagenes/Ricky%20pantalla%20ganaste.png");
        }
        else
        {
            ViewBag.ClaseFin = "fin fin-perdio";
            ViewBag.MensajeFin = "¡Te quedaste sin vidas! Perdiste el nivel";
            ViewBag.AltMascota = "Riky triste";
            ViewBag.ImagenMascota = Url.Content("~/imagenes/Ricky%20pantalla%20perdiste.png");
        }

        // "Repasar tema nuevamente" solo aparece si perdió
        ViewBag.OcultarRepasar = gano || idNivel == 0;
        ViewBag.UrlRepasar = Url.Action("Teoria", "Juego", new { idNivel = idNivel });

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
        if (segundos < 60)
        {
            return segundos + " seg";
        }

         int minutos = segundos / 60;
         int resto = segundos % 60;

         return minutos + ":" + resto.ToString("00") + " min";
    }
}
