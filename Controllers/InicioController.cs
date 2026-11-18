using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using RIK.Models;

namespace RIK.Controllers;

public class InicioController : Controller
{
    // Cantidad de niveles del mapa. Si se agregan niveles, alcanza con cambiar este número:
    // el camino se alarga solo.
    private const int TotalNiveles = 12;

    // Unidades del camino: 1000 = ancho (o alto) de la pantalla. "y" negativa = hacia arriba.

    // Primeros 5 niveles: son los de la primera pantalla del diseño.
    private static readonly int[][] PuntosIniciales = new int[][]
    {
        new int[] { 92, -187 },
        new int[] { 283, -474 },
        new int[] { 513, -655 },
        new int[] { 738, -772 },
        new int[] { 934, -836 }
    };

    // Curvas fijas que unen los primeros 5 niveles.
    private const string CaminoInicial =
        "M -30 -360 C 20 -310, 60 -250, 92 -187 " +
        "C 110 -390, 120 -520, 195 -542 C 240 -554, 262 -510, 283 -474 " +
        "C 330 -390, 400 -290, 445 -360 C 485 -425, 470 -580, 513 -655 " +
        "C 560 -600, 640 -470, 685 -520 C 720 -560, 700 -700, 738 -772 " +
        "C 790 -715, 850 -700, 885 -750 C 905 -780, 915 -810, 934 -836 ";

    // Del nivel 6 en adelante se van usando estos tramos, uno distinto por nivel
    // (recién se repiten cada 7 niveles). Cada tramo son dos curvas, con sus puntos
    // medidos desde la moneda anterior; los dos últimos números son la moneda siguiente.
    private static readonly int[][] Tramos = new int[][]
    {
        new int[] { 26, -124, 106, -204, 141, -154,   161, -129, 176, -114, 196, -104 },
        new int[] { 50, 30, 100, 120, 160, 50,        210, -10, 210, -140, 260, -200 },
        new int[] { 60, 20, 110, -10, 120, -80,       128, -130, 135, -180, 150, -220 },
        new int[] { 40, -100, 140, -140, 190, -100,   220, -80, 240, -65, 260, -50 },
        new int[] { 40, 30, 90, 50, 115, -10,         145, -80, 130, -170, 160, -230 },
        new int[] { 50, -80, 120, -60, 160, -10,      200, 40, 240, -20, 270, -70 },
        new int[] { 40, -60, 60, -30, 100, -50,       150, -75, 160, -140, 190, -180 }
    };

    public IActionResult Landing()
    {
        return View();
    }

    public IActionResult IndexInicio()
    {
        if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            int idUsuario = new BD().ObtenerIdUsuario(nombreUsuario);
            List<int> nivelesCompletados = new BD().ObtenerNivelesCompletados(idUsuario);

            ViewBag.NivelesCompletados = nivelesCompletados;
            ViewBag.Racha = new BD().ObtenerRacha(idUsuario);
            ViewBag.MaxNivelDesbloqueado = Math.Min(TotalNiveles, Math.Max(1, nivelesCompletados.Count + 1));
            ViewBag.TotalNiveles = TotalNiveles;

            List<int[]> puntos = ArmarPuntos(TotalNiveles);
            int[] ultimo = ObtenerUltimoPunto(puntos, TotalNiveles);
            int anchoCamino = CalcularAnchoCamino(ultimo);
            int altoCamino = CalcularAltoCamino(ultimo);

            ViewBag.Camino = ArmarCamino(puntos, TotalNiveles);
            ViewBag.AnchoCamino = anchoCamino;
            ViewBag.AltoCamino = altoCamino;
            ViewBag.TamanoLienzo = ArmarTamanoLienzo(anchoCamino, altoCamino);
            ViewBag.PosicionesNiveles = ArmarPosicionesNiveles(puntos, TotalNiveles, anchoCamino, altoCamino);

            return View();
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }

    // Devuelve la posición (x, y) de la moneda de cada nivel.
    private List<int[]> ArmarPuntos(int totalNiveles)
    {
        List<int[]> puntos = new List<int[]>();
        foreach (int[] punto in PuntosIniciales)
        {
            puntos.Add(new int[] { punto[0], punto[1] });
        }

        for (int nivel = 6; nivel <= totalNiveles; nivel++)
        {
            int[] desde = puntos[puntos.Count - 1];
            int[] tramo = ObtenerTramo(nivel);
            puntos.Add(new int[] { desde[0] + tramo[10], desde[1] + tramo[11] });
        }

        return puntos;
    }

    // Arma el atributo "d" del <path> del SVG que dibuja el camino.
    private string ArmarCamino(List<int[]> puntos, int totalNiveles)
    {
        StringBuilder camino = new StringBuilder();
        camino.Append(CaminoInicial);

        for (int nivel = 6; nivel <= totalNiveles; nivel++)
        {
            // La moneda anterior al nivel actual.
            int[] desde = puntos[nivel - 2];
            int[] t = ObtenerTramo(nivel);

            camino.Append($"C {desde[0] + t[0]} {desde[1] + t[1]}, {desde[0] + t[2]} {desde[1] + t[3]}, {desde[0] + t[4]} {desde[1] + t[5]} ");
            camino.Append($"C {desde[0] + t[6]} {desde[1] + t[7]}, {desde[0] + t[8]} {desde[1] + t[9]}, {desde[0] + t[10]} {desde[1] + t[11]} ");
        }

        // Después de la última moneda el camino sigue un poco y sale de la pantalla.
        if (totalNiveles >= 5)
        {
            int[] ultimo = ObtenerUltimoPunto(puntos, totalNiveles);
            camino.Append($"C {ultimo[0] + 50} {ultimo[1] - 60}, {ultimo[0] + 120} {ultimo[1] - 50}, {ultimo[0] + 170} {ultimo[1] - 110} ");
            camino.Append($"C {ultimo[0] + 190} {ultimo[1] - 140}, {ultimo[0] + 210} {ultimo[1] - 170}, {ultimo[0] + 230} {ultimo[1] - 200}");
        }

        return camino.ToString();
    }

    private int[] ObtenerTramo(int nivel)
    {
        return Tramos[(nivel - 6) % Tramos.Length];
    }

    private int[] ObtenerUltimoPunto(List<int[]> puntos, int totalNiveles)
    {
        return puntos[Math.Min(totalNiveles, puntos.Count) - 1];
    }

    // Tamaño de la capa que se mueve: crece con la cantidad de niveles.
    private int CalcularAnchoCamino(int[] ultimo)
    {
        return Math.Max(1000, ultimo[0] + 180);
    }

    private int CalcularAltoCamino(int[] ultimo)
    {
        return Math.Max(1000, -ultimo[1] + 110);
    }

    private string ArmarTamanoLienzo(int anchoCamino, int altoCamino)
    {
        CultureInfo cultura = CultureInfo.InvariantCulture;
        string ancho = (anchoCamino / 10.0).ToString("0.##", cultura);
        string alto = (altoCamino / 10.0).ToString("0.##", cultura);
        return $"width: {ancho}%; height: {alto}%;";
    }

    // Devuelve el estilo "left/top" (en %) de cada moneda, en orden de nivel.
    private List<string> ArmarPosicionesNiveles(List<int[]> puntos, int totalNiveles, int anchoCamino, int altoCamino)
    {
        CultureInfo cultura = CultureInfo.InvariantCulture;
        List<string> posiciones = new List<string>();

        for (int nivel = 1; nivel <= totalNiveles; nivel++)
        {
            int[] punto = puntos[nivel - 1];
            string izquierda = (punto[0] * 100.0 / anchoCamino).ToString("0.##", cultura);
            string arriba = ((altoCamino + punto[1]) * 100.0 / altoCamino).ToString("0.##", cultura);
            posiciones.Add($"left: {izquierda}%; top: {arriba}%;");
        }

        return posiciones;
    }
}
