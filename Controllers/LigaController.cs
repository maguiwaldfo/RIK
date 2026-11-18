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
        string htmlGlobal = GenerarHtmlLeaderboardFigma(leaderboardGlobal, "No hay datos registrados en el ranking global.");

        string htmlLiga = "";
        string tituloLiga = "Liga de Amigos";

        if (ligaActual != null)
        {
            List<UsuarioRanking> leaderboardLiga = bd.ObtenerLeaderboardLiga(ligaActual.Id);

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
            htmlLiga = GenerarHtmlLeaderboardFigma(leaderboardLiga, "No hay participantes en esta liga.");
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

    private string GenerarHtmlLeaderboardFigma(List<UsuarioRanking> lista, string mensajeVacio)
    {
        if (lista == null || lista.Count == 0)
        {
            return "<p>" + mensajeVacio + "</p>";
        }

        string html = "<div style=\"width: 484px; padding: 28px; position: relative; background: #0A4D28; border-radius: 40px; outline: 4px rgba(6, 56, 29, 0.30) solid; outline-offset: -4px; flex-direction: column; justify-content: flex-start; align-items: flex-start; gap: 14px; display: inline-flex; font-family: 'Baloo 2', sans-serif;\">";

        int posicion = 1;

        foreach (UsuarioRanking item in lista)
        {
            string nombreMostrar = item.NombreUsuario;
            int puntos = item.PuntajeTotal;

            if (posicion == 1)
            {
                html += $@"
                <div style=""align-self: stretch; padding: 10px; position: relative; background: linear-gradient(90deg, #FFE566 0%, #FFD700 50%, #F5C300 100%); border-radius: 9999px; box-shadow: 0px 4px 6px -4px rgba(0, 0, 0, 0.10), 0px 10px 15px -3px rgba(0, 0, 0, 0.10), 0px 0px 0px 4px rgba(255, 215, 0, 0.70); justify-content: space-between; align-items: center; display: inline-flex;"">
                    <div style=""padding-left: 4px; justify-content: flex-start; align-items: center; gap: 12px; display: flex;"">
                        <div style=""width: 36px; height: 36px; background: white; box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.05); border-radius: 9999px; outline: 2px #FFD700 solid; outline-offset: -2px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""color: #1C1917; font-size: 16px; font-weight: 800;"">1°</div>
                        </div>
                        <div style=""width: 44px; height: 44px; background: #1C1917; box-shadow: 0px 0px 0px 2px white; border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""width: 16px; height: 16px; background: #FFD700; border-radius: 2px;""></div>
                        </div>
                        <div style=""flex-direction: column; display: inline-flex;"">
                            <div style=""color: #0C0A09; font-size: 24px; font-weight: 800; line-height: 32px;"">{nombreMostrar}</div>
                            <div style=""color: #0A4D28; font-size: 12px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.60px;"">🥇 CAMPEÓN</div>
                        </div>
                    </div>
                    <div style=""padding-right: 4px;"">
                        <div style=""padding: 4px 20px; background: #0A4D28; border-radius: 9999px; outline: 1px rgba(255, 255, 255, 0.20) solid; box-shadow: 0px 4px 6px -1px rgba(0, 0, 0, 0.10);"">
                            <div style=""color: #FFD700; font-size: 18px; font-weight: 800; line-height: 28px;"">{puntos} pts</div>
                        </div>
                    </div>
                </div>";
            }
            else if (posicion == 2)
            {
                html += $@"
                <div style=""align-self: stretch; padding: 10px; position: relative; background: #F1F5F9; border-radius: 9999px; box-shadow: 0px 2px 4px -2px rgba(0, 0, 0, 0.10), 0px 4px 6px -1px rgba(0, 0, 0, 0.10), 0px 0px 0px 2px #CBD5E1; justify-content: space-between; align-items: center; display: inline-flex;"">
                    <div style=""padding-left: 4px; justify-content: flex-start; align-items: center; gap: 12px; display: flex;"">
                        <div style=""width: 36px; height: 36px; background: #E2E8F0; box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.05) inset; border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""color: #292524; font-size: 16px; font-weight: 800;"">2°</div>
                        </div>
                        <div style=""width: 44px; height: 44px; background: #1C1917; box-shadow: 0px 0px 0px 2px #CBD5E1; border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""width: 16px; height: 16px; background: #E2E8F0; border-radius: 2px;""></div>
                        </div>
                        <div style=""flex-direction: column; display: inline-flex;"">
                            <div style=""color: #0C0A09; font-size: 24px; font-weight: 700; line-height: 32px;"">{nombreMostrar}</div>
                            <div style=""color: #475569; font-size: 12px; font-weight: 700;"">🥈 2° Lugar</div>
                        </div>
                    </div>
                    <div style=""padding-right: 4px;"">
                        <div style=""padding: 4px 20px; background: white; border-radius: 9999px; outline: 1px #E2E8F0 solid; box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.05);"">
                            <div style=""color: #1C1917; font-size: 18px; font-weight: 800; line-height: 28px;"">{puntos} pts</div>
                        </div>
                    </div>
                </div>";
            }
            else if (posicion == 3)
            {
                html += $@"
                <div style=""align-self: stretch; padding: 10px; position: relative; background: #FFE9D6; border-radius: 9999px; box-shadow: 0px 2px 4px -2px rgba(0, 0, 0, 0.10), 0px 4px 6px -1px rgba(0, 0, 0, 0.10), 0px 0px 0px 2px #E6A877; justify-content: space-between; align-items: center; display: inline-flex;"">
                    <div style=""padding-left: 4px; justify-content: flex-start; align-items: center; gap: 12px; display: flex;"">
                        <div style=""width: 36px; height: 36px; background: #F4CAA5; box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.05) inset; border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""color: #743C11; font-size: 16px; font-weight: 800;"">3°</div>
                        </div>
                        <div style=""width: 44px; height: 44px; background: #1C1917; box-shadow: 0px 0px 0px 2px #E6A877; border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""width: 16px; height: 16px; background: #F4CAA5; border-radius: 2px;""></div>
                        </div>
                        <div style=""flex-direction: column; display: inline-flex;"">
                            <div style=""color: #0C0A09; font-size: 24px; font-weight: 700; line-height: 32px;"">{nombreMostrar}</div>
                            <div style=""color: #96541F; font-size: 12px; font-weight: 700;"">🥉 3° Lugar</div>
                        </div>
                    </div>
                    <div style=""padding-right: 4px;"">
                        <div style=""padding: 4px 20px; background: white; border-radius: 9999px; outline: 1px #F4CAA5 solid; box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.05);"">
                            <div style=""color: #1C1917; font-size: 18px; font-weight: 800; line-height: 28px;"">{puntos} pts</div>
                        </div>
                    </div>
                </div>";
            }
            else
            {
                html += $@"
                <div style=""align-self: stretch; padding: 10px; background: rgba(255, 255, 255, 0.10); box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.05); border-radius: 9999px; outline: 1px rgba(255, 255, 255, 0.20) solid; outline-offset: -1px; justify-content: space-between; align-items: center; display: inline-flex;"">
                    <div style=""padding-left: 4px; justify-content: flex-start; align-items: center; gap: 12px; display: flex;"">
                        <div style=""width: 36px; height: 36px; background: rgba(255, 255, 255, 0.10); border-radius: 9999px; justify-content: center; align-items: center; display: flex;"">
                            <div style=""color: rgba(255, 255, 255, 0.80); font-size: 16px; font-weight: 700;"">{posicion}°</div>
                        </div>
                        <div style=""width: 44px; height: 44px; background: rgba(28, 25, 23, 0.80); border-radius: 9999px; outline: 1px rgba(255, 255, 255, 0.10) solid; justify-content: center; align-items: center; display: flex;"">
                            <div style=""width: 13.33px; height: 13.33px; background: rgba(255, 255, 255, 0.70); border-radius: 2px;""></div>
                        </div>
                        <div style=""color: white; font-size: 24px; font-weight: 700; line-height: 32px;"">{nombreMostrar}</div>
                    </div>
                    <div style=""padding-right: 4px;"">
                        <div style=""padding: 4px 20px; background: rgba(255, 255, 255, 0.20); box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.05); border-radius: 9999px;"">
                            <div style=""color: white; font-size: 18px; font-weight: 700; line-height: 28px;"">{puntos} pts</div>
                        </div>
                    </div>
                </div>";
            }

            posicion++;
        }

        html += "</div>";
        return html;
    }
}