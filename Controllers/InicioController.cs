using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class InicioController : Controller
{
    public IActionResult Landing()
    {
        return View();
    }

    public IActionResult IndexInicio()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            int idUsuario = BD.ObtenerIdUsuario(nombreUsuario);
            List<int> nivelesCompletados = BD.ObtenerNivelesCompletados(idUsuario);

            // Los niveles que existen en la base, ordenados por Id
            List<Nivel> niveles = BD.ObtenerNiveles();

            int totalNiveles = niveles.Count;
            int maxNivelDesbloqueado = Math.Min(totalNiveles, Math.Max(1, nivelesCompletados.Count + 1));

            ViewData["Title"] = "Inicio";
            ViewBag.Racha = BD.ObtenerRacha(idUsuario);
            ViewBag.CantidadCompletados = nivelesCompletados.Count;

            // Las cuentas del camino están en Models/MapaNiveles.cs
            List<int[]> puntos = MapaNiveles.ArmarPuntos(totalNiveles);
            int[] ultimo = MapaNiveles.ObtenerUltimoPunto(puntos, Math.Max(1, totalNiveles));
            int anchoCamino = MapaNiveles.CalcularAnchoCamino(ultimo);
            int altoCamino = MapaNiveles.CalcularAltoCamino(ultimo);
            List<string> posiciones = MapaNiveles.ArmarPosicionesNiveles(puntos, totalNiveles, anchoCamino, altoCamino);

            ViewBag.Camino = MapaNiveles.ArmarCamino(puntos, totalNiveles);
            ViewBag.ViewBoxCamino = "0 -" + altoCamino + " " + anchoCamino + " " + altoCamino;
            ViewBag.TamanoLienzo = MapaNiveles.ArmarTamanoLienzo(anchoCamino, altoCamino);
            ViewBag.Nodos = ArmarNodos(niveles, nivelesCompletados, maxNivelDesbloqueado, posiciones);

            return View();
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }

    // Prepara cada moneda del mapa (antes estas decisiones estaban en la vista IndexInicio).
    // Se puede entrar a los niveles completados y al actual; el resto se ve bloqueado.
    private List<NodoMapa> ArmarNodos(List<Nivel> niveles, List<int> completados, int maxNivelDesbloqueado, List<string> posiciones)
    {
        List<NodoMapa> nodos = new List<NodoMapa>();

        for (int i = 0; i < niveles.Count; i++)
        {
            int numero = i + 1;   // posición del nivel en el camino
            Nivel nivel = niveles[i];

            bool estaCompletado = completados.Contains(nivel.Id);
            bool estaDesbloqueado = numero <= maxNivelDesbloqueado;
            bool esActual = numero == maxNivelDesbloqueado;

            NodoMapa nodo = new NodoMapa();
            nodo.Estilo = posiciones[i];

            if (estaDesbloqueado && (estaCompletado || esActual))
            {
                nodo.Clase = "inicio-nodo inicio-nodo-desbloqueado";

                if (esActual)
                {
                    nodo.Clase = "inicio-nodo inicio-nodo-desbloqueado inicio-nodo-actual";
                }

                nodo.Url = Url.Action("Teoria", "Juego", new { idNivel = nivel.Id });
                nodo.Imagen = Url.Content("~/imagenes/monedaNivel.png");
                nodo.Alt = "Nivel " + numero;
            }
            else if (estaDesbloqueado)
            {
                nodo.Clase = "inicio-nodo";
                nodo.Url = null;
                nodo.Imagen = Url.Content("~/imagenes/monedaNivel.png");
                nodo.Alt = "Nivel bloqueado";
            }
            else
            {
                nodo.Clase = "inicio-nodo inicio-nodo-bloqueado";
                nodo.Url = null;
                nodo.Imagen = Url.Content("~/imagenes/monedaBloqueada.png");
                nodo.Alt = "Nivel bloqueado";
            }

            nodos.Add(nodo);
        }

        return nodos;
    }
}
