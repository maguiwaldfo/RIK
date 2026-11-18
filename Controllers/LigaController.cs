using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
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

    // Obtener todas las ligas del usuario
    List<Liga> ligas = bd.ObtenerLigasDeUsuario(idUsuario);

    ViewBag.Ligas = ligas;

    // Seleccionar la liga que se quiere mostrar
    Liga ligaActual = null;

    if (idLigaAmigos != 0)
    {
        ligaActual = ligas.Find(l => l.Id == idLigaAmigos);
    }

    // Si no se seleccionó ninguna, mostrar la primera
    if (ligaActual == null && ligas.Count > 0)
    {
        ligaActual = ligas[0];
    }

    List<UsuarioRanking> leaderboardGlobal = bd.ObtenerLeaderboardGlobal();
    List<UsuarioRanking> leaderboardLiga = new List<UsuarioRanking>();

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

    string htmlLiga = "";
    string tituloLiga = "Liga de Amigos";

    if (ligaActual != null)
    {
        leaderboardLiga = bd.ObtenerLeaderboardLiga(ligaActual.Id);

        ViewBag.IdLiga = ligaActual.Id;
        ViewBag.AccionLiga = "Abandonar";
        ViewBag.TextoBotonLiga = "Abandonar liga";

        if (ligaActual.IdUsuarioCreador == idUsuario)
        {
            ViewBag.AccionLiga = "Eliminar";
            ViewBag.TextoBotonLiga = "Eliminar liga";

            // Solo el dueño puede compartir la liga y ver las solicitudes
            ViewBag.EsCreador = true;
            ViewBag.EsPrivada = ligaActual.Privacidad;
            ViewBag.LinkInvitacion = Url.Action("UnirseALiga", "Liga", new { codigo = ligaActual.CodigoInvitacion }, Request.Scheme);

            if (ligaActual.Privacidad)
            {
                ViewBag.CantidadSolicitudes = bd.ObtenerSolicitudesLiga(ligaActual.Id, idUsuario).Count;
            }
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
        ViewBag.IdLiga = 0;
        ViewBag.AccionLiga = "";
        ViewBag.TextoBotonLiga = "";

        htmlLiga = "<p>No hay ligas. Todavía no pertenecés a ninguna liga de amigos.</p>";
    }

    ViewBag.HtmlTablaGlobal = htmlGlobal;
    ViewBag.TituloLiga = tituloLiga;
    ViewBag.HtmlTablaLiga = htmlLiga;

    return View();
}

    // Busca ligas por nombre o muestra la liga de un link de invitación.
    // "link" es lo que el usuario pega; "codigo" llega cuando abre el link directamente.
    [HttpGet]
    public IActionResult UnirseALiga(string busqueda = "", string link = "", string codigo = "")
    {
        int idUsuario = ObtenerIdUsuarioActual();

        if (idUsuario == 0)
        {
            return RedirectToAction("Index", "Login");
        }

        busqueda = (busqueda ?? "").Trim();
        codigo = (codigo ?? "").Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(link))
        {
            codigo = ExtraerCodigoDeLink(link);

            if (codigo == "")
            {
                ViewBag.Error = "Ese link no es válido. Pedile a quien creó la liga que te lo vuelva a pasar.";
            }
        }

        LigaBusqueda ligaInvitacion = null;

        if (codigo != "")
        {
            if (EsCodigoValido(codigo))
            {
                ligaInvitacion = bd.ObtenerLigaPorCodigo(codigo, idUsuario);
            }

            if (ligaInvitacion == null)
            {
                ViewBag.Error = "No encontramos ninguna liga con ese link. Puede que la hayan eliminado.";
                codigo = "";
            }
        }

        List<LigaBusqueda> resultados = null;

        if (busqueda != "")
        {
            if (busqueda.Length > 50)
            {
                busqueda = busqueda.Substring(0, 50);
            }

            resultados = bd.BuscarLigas(busqueda, idUsuario);
        }

        ViewBag.Busqueda = busqueda;
        ViewBag.Codigo = codigo;
        ViewBag.LigaInvitacion = ligaInvitacion;
        ViewBag.Resultados = resultados;

        return View();
    }

    // Pública: entra directo. Privada: manda una solicitud (con la contraseña o el código del link).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SolicitarUnirse(int idLiga, string contraseña = "", string codigo = "", string busqueda = "")
    {
        int idUsuario = ObtenerIdUsuarioActual();

        if (idUsuario == 0)
        {
            return RedirectToAction("Index", "Login");
        }

        codigo = (codigo ?? "").Trim().ToUpperInvariant();

        if (!EsCodigoValido(codigo))
        {
            codigo = "";
        }

        int resultado = bd.SolicitarUnirseALiga(
            idLiga,
            idUsuario,
            string.IsNullOrEmpty(contraseña) ? null : contraseña,
            codigo == "" ? null : codigo
        );

        if (resultado == 1)
        {
            TempData["Mensaje"] = "¡Listo! Ya sos parte de la liga.";
            return RedirectToAction("Index", new { idLigaAmigos = idLiga });
        }

        if (resultado == 2)
        {
            TempData["Mensaje"] = "Solicitud enviada. Cuando el creador de la liga la acepte, vas a aparecer en el ranking.";
        }
        else
        {
            TempData["Error"] = MensajeErrorUnirse(resultado);
        }

        return RedirectToAction("UnirseALiga", new { busqueda = busqueda, codigo = codigo });
    }

    // Lista de solicitudes pendientes. Solo la puede ver el dueño de la liga.
    [HttpGet]
    public IActionResult Solicitudes(int idLiga)
    {
        int idUsuario = ObtenerIdUsuarioActual();

        if (idUsuario == 0)
        {
            return RedirectToAction("Index", "Login");
        }

        Liga liga = bd.ObtenerLigaPorId(idLiga);

        if (liga == null || liga.IdUsuarioCreador != idUsuario)
        {
            return RedirectToAction("Index");
        }

        ViewBag.Liga = liga;
        ViewBag.Solicitudes = bd.ObtenerSolicitudesLiga(idLiga, idUsuario);
        ViewBag.CantidadMiembros = bd.ObtenerLeaderboardLiga(idLiga).Count;

        return View();
    }

    // El dueño acepta o veta a quien pidió entrar.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResponderSolicitud(int idLiga, int idSolicitud, bool aceptar)
    {
        int idUsuario = ObtenerIdUsuarioActual();

        if (idUsuario == 0)
        {
            return RedirectToAction("Index", "Login");
        }

        int resultado = bd.ResponderSolicitudLiga(idSolicitud, idUsuario, aceptar);

        if (resultado == 1)
        {
            TempData["Mensaje"] = "Solicitud aceptada. Ya está en tu liga.";
        }
        else if (resultado == 2)
        {
            TempData["Mensaje"] = "Solicitud vetada.";
        }
        else if (resultado == -2)
        {
            TempData["Error"] = "La liga está llena. Para aceptar a alguien más tiene que liberarse un lugar.";
        }
        else
        {
            TempData["Error"] = "No encontramos esa solicitud. Puede que ya la hayas respondido.";
        }

        return RedirectToAction("Solicitudes", new { idLiga = idLiga });
    }

    private int ObtenerIdUsuarioActual()
    {
        string nombreUsuario = ObtenerUsuarioActual();

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return 0;
        }

        return bd.ObtenerIdUsuarioPorNombre(nombreUsuario);
    }

    // Acepta el link completo (https://.../Liga/UnirseALiga?codigo=ABC123) o solo el código.
    private string ExtraerCodigoDeLink(string link)
    {
        string texto = link.Trim();
        string codigo = texto;

        if (Uri.TryCreate(texto, UriKind.Absolute, out Uri uri))
        {
            Dictionary<string, StringValues> parametros = QueryHelpers.ParseQuery(uri.Query);
            codigo = "";

            if (parametros.ContainsKey("codigo"))
            {
                codigo = parametros["codigo"].ToString();
            }
        }

        codigo = codigo.Trim().ToUpperInvariant();

        if (!EsCodigoValido(codigo))
        {
            return "";
        }

        return codigo;
    }

    private bool EsCodigoValido(string codigo)
    {
        return Regex.IsMatch(codigo ?? "", "^[A-Z0-9]{1,12}$");
    }

    private string MensajeErrorUnirse(int resultado)
    {
        if (resultado == -1)
        {
            return "Ya sos parte de esa liga.";
        }

        if (resultado == -2)
        {
            return "La liga está llena.";
        }

        if (resultado == -3)
        {
            return "La contraseña no es correcta.";
        }

        if (resultado == -4)
        {
            return "Ya pediste entrar a esa liga. Esperá a que el creador te responda.";
        }

        return "No encontramos esa liga.";
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