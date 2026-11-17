using Microsoft.AspNetCore.Mvc.Filters;

namespace RIK.Models;

// Si el jugador sale de un nivel sin terminarlo y va a cualquier otra vista,
// la partida se borra de la session: al volver al nivel empieza de cero.
// Refrescar la página del nivel no la borra, porque vuelve a pedir Juego/Nivel.
public class ReiniciarPartidaFilter : IActionFilter
{
    // Acciones que forman parte de la partida: no la reinician.
    private static readonly string[] accionesDelNivel = { "Nivel", "ComprobarRespuesta", "FinalizarNivel" };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        string controlador = context.RouteData.Values["controller"]?.ToString();
        string accion = context.RouteData.Values["action"]?.ToString();

        if (controlador == "Juego" && accionesDelNivel.Contains(accion))
        {
            return;
        }

        ISession session = context.HttpContext.Session;
        string estado = session.GetString("EstadoPartida");

        // "Jugando" y "Ganado" son partidas sin guardar. "Perdido" y "Guardado"
        // ya terminaron y Resultado todavía las necesita.
        if (estado == "Jugando" || estado == "Ganado")
        {
            ReiniciarPartida(session);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public static void ReiniciarPartida(ISession session)
    {
        session.Remove("EstadoPartida");
        session.Remove("NivelActual");
        session.Remove("PreguntaActual");
        session.Remove("Vidas");
        session.Remove("Puntos");
        session.Remove("InicioPartida");

        for (int i = 0; i < 8; i++)
        {
            session.Remove("Pregunta" + i);
        }
    }
}
