using Microsoft.AspNetCore.Mvc;
using RIK.Models;

namespace RIK.Controllers;

public class LigaController : Controller
{
    private string ObtenerUsuarioActual()
    {
        return HttpContext.Session.GetString("NombreUsuario");
    }

    // Avisos que se muestran una sola vez en la página siguiente (después de un RedirectToAction).
    // Se guardan en la Session y se borran apenas se leen.
    private void GuardarAviso(string clave, string texto)
    {
        HttpContext.Session.SetString(clave, texto);
    }

    private string LeerAviso(string clave)
    {
        string texto = HttpContext.Session.GetString(clave);
        HttpContext.Session.Remove(clave);
        return texto;
    }

    public IActionResult Index(int idLigaAmigos = 0)
    {
        string nombreUsuario = ObtenerUsuarioActual();

        if (string.IsNullOrEmpty(nombreUsuario))
        {
            return RedirectToAction("Index", "Login");
        }

        int idUsuario = BD.ObtenerIdUsuarioPorNombre(nombreUsuario);

        if (idUsuario == 0)
        {
            return RedirectToAction("Index", "Login");
        }

        ViewData["Title"] = "Ligas y Tabla de Posiciones";

        // Obtener todas las ligas del usuario
        List<Liga> ligas = BD.ObtenerLigasDeUsuario(idUsuario);

        ViewBag.Ligas = ligas;
        ViewBag.OcultarSinLigas = ligas.Count > 0;

        // Aviso que llega desde otra acción (por ejemplo, después de unirse a una liga)
        string mensaje = LeerAviso("AvisoLiga");
        ViewBag.Mensaje = mensaje;
        ViewBag.OcultarAviso = string.IsNullOrEmpty(mensaje);

        // Valores por defecto: sin liga elegida no se muestra "Invitar amigos" ni el botón de abandonar/eliminar
        ViewBag.IdLiga = 0;
        ViewBag.OcultarAccionLiga = true;
        ViewBag.UrlAccionLiga = "";
        ViewBag.TextoBotonLiga = "";
        ViewBag.ClaseBotonLiga = "";
        ViewBag.OcultarCompartir = true;
        ViewBag.LinkInvitacion = "";
        ViewBag.OcultarSolicitudes = true;
        ViewBag.UrlSolicitudes = "";
        ViewBag.CantidadSolicitudes = 0;
        ViewBag.OcultarCantidadSolicitudes = true;

        // Seleccionar la liga que se quiere mostrar
        Liga ligaActual = null;

        foreach (Liga liga in ligas)
        {
            if (liga.Id == idLigaAmigos)
            {
                ligaActual = liga;
            }
        }

        // Si no se seleccionó ninguna, mostrar la primera
        if (ligaActual == null && ligas.Count > 0)
        {
            ligaActual = ligas[0];
        }

        List<UsuarioRanking> leaderboardGlobal = BD.ObtenerLeaderboardGlobal();

        ViewBag.RankingGlobal = ArmarRanking(leaderboardGlobal);
        ViewBag.OcultarRankingGlobal = leaderboardGlobal.Count == 0;
        ViewBag.MensajeRankingGlobal = "No hay datos registrados en el ranking global.";
        ViewBag.OcultarMensajeRankingGlobal = leaderboardGlobal.Count > 0;

        // Ranking de la liga elegida (vacío si no pertenece a ninguna liga)
        List<UsuarioRanking> leaderboardLiga = new List<UsuarioRanking>();
        string mensajeRankingLiga = "No hay ligas. Todavía no pertenecés a ninguna liga de amigos.";
        string tituloLiga = "Mis ligas";

        if (ligaActual != null)
        {
            leaderboardLiga = BD.ObtenerLeaderboardLiga(ligaActual.Id);
            mensajeRankingLiga = "No hay participantes en esta liga.";

            ViewBag.IdLiga = ligaActual.Id;
            ViewBag.OcultarAccionLiga = false;
            ViewBag.UrlAccionLiga = Url.Action("Abandonar", "Liga");
            ViewBag.TextoBotonLiga = "Abandonar liga";

            if (ligaActual.IdUsuarioCreador == idUsuario)
            {
                ViewBag.UrlAccionLiga = Url.Action("Eliminar", "Liga");
                ViewBag.TextoBotonLiga = "Eliminar liga";
                ViewBag.ClaseBotonLiga = "peligro-boton";

                // Solo el dueño puede compartir la liga y ver las solicitudes
                ViewBag.OcultarCompartir = false;
                ViewBag.LinkInvitacion = Url.Action("UnirseALiga", "Liga", new { codigo = ligaActual.CodigoInvitacion }, Request.Scheme);

                if (ligaActual.Privacidad)
                {
                    int cantidadSolicitudes = BD.ObtenerSolicitudesLiga(ligaActual.Id, idUsuario).Count;

                    ViewBag.OcultarSolicitudes = false;
                    ViewBag.UrlSolicitudes = Url.Action("Solicitudes", "Liga", new { idLiga = ligaActual.Id });
                    ViewBag.CantidadSolicitudes = cantidadSolicitudes;
                    ViewBag.OcultarCantidadSolicitudes = cantidadSolicitudes == 0;
                }
            }

            tituloLiga = "Liga: " + ligaActual.Nombre;
        }

        ViewBag.TituloLiga = tituloLiga;
        ViewBag.RankingLiga = ArmarRanking(leaderboardLiga);
        ViewBag.OcultarRankingLiga = leaderboardLiga.Count == 0;
        ViewBag.MensajeRankingLiga = mensajeRankingLiga;
        ViewBag.OcultarMensajeRankingLiga = leaderboardLiga.Count > 0;

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

        busqueda = TextoLimpio(busqueda);
        codigo = TextoLimpio(codigo).ToUpper();

        string error = "";

        if (!string.IsNullOrWhiteSpace(link))
        {
            codigo = ExtraerCodigoDeLink(link);

            if (codigo == "")
            {
                error = "Ese link no es válido. Pedile a quien creó la liga que te lo vuelva a pasar.";
            }
        }

        LigaBusqueda ligaInvitacion = null;

        if (codigo != "")
        {
            if (EsCodigoValido(codigo))
            {
                ligaInvitacion = BD.ObtenerLigaPorCodigo(codigo, idUsuario);
            }

            if (ligaInvitacion == null)
            {
                error = "No encontramos ninguna liga con ese link. Puede que la hayan eliminado.";
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

            resultados = BD.BuscarLigas(busqueda, idUsuario);
        }

        ViewData["Title"] = "Unirse a una liga";
        ViewBag.Busqueda = busqueda;

        // Mensajes que llegan desde SolicitarUnirse y errores de esta misma página
        string mensaje = LeerAviso("AvisoLiga");
        string errorAnterior = LeerAviso("ErrorLiga");

        if (!string.IsNullOrEmpty(errorAnterior))
        {
            error = (errorAnterior + " " + error).Trim();
        }

        ViewBag.Mensaje = mensaje;
        ViewBag.OcultarMensaje = string.IsNullOrEmpty(mensaje);
        ViewBag.Error = error;
        ViewBag.OcultarError = error == "";

        // Tarjeta de la liga del link de invitación (lista de 0 o 1 tarjetas)
        List<TarjetaLiga> tarjetasInvitacion = new List<TarjetaLiga>();

        if (ligaInvitacion != null)
        {
            tarjetasInvitacion.Add(ArmarTarjeta(ligaInvitacion, codigo, ""));
        }

        ViewBag.TarjetasInvitacion = tarjetasInvitacion;
        ViewBag.OcultarInvitacion = ligaInvitacion == null;

        // Tarjetas de los resultados de la búsqueda
        List<TarjetaLiga> tarjetasResultados = new List<TarjetaLiga>();

        if (resultados != null)
        {
            foreach (LigaBusqueda liga in resultados)
            {
                tarjetasResultados.Add(ArmarTarjeta(liga, "", busqueda));
            }
        }

        ViewBag.TarjetasResultados = tarjetasResultados;
        ViewBag.OcultarResultados = resultados == null;
        ViewBag.TituloResultados = "Resultados para \"" + busqueda + "\"";
        ViewBag.OcultarSinResultados = resultados == null || resultados.Count > 0;

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

        codigo = TextoLimpio(codigo).ToUpper();

        if (!EsCodigoValido(codigo))
        {
            codigo = "";
        }

        // Al procedure se le manda null cuando no hay contraseña o no hay código
        string contraseñaParaBD = null;
        string codigoParaBD = null;

        if (!string.IsNullOrEmpty(contraseña))
        {
            contraseñaParaBD = contraseña;
        }

        if (codigo != "")
        {
            codigoParaBD = codigo;
        }

        int resultado = BD.SolicitarUnirseALiga(idLiga, idUsuario, contraseñaParaBD, codigoParaBD);

        if (resultado == 1)
        {
            GuardarAviso("AvisoLiga", "¡Listo! Ya sos parte de la liga.");
            return RedirectToAction("Index", new { idLigaAmigos = idLiga });
        }

        if (resultado == 2)
        {
            GuardarAviso("AvisoLiga", "Solicitud enviada. Cuando el creador de la liga la acepte, vas a aparecer en el ranking.");
        }
        else
        {
            GuardarAviso("ErrorLiga", MensajeErrorUnirse(resultado));
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

        Liga liga = BD.ObtenerLigaPorId(idLiga);

        if (liga == null || liga.IdUsuarioCreador != idUsuario)
        {
            return RedirectToAction("Index");
        }

        List<SolicitudLiga> solicitudes = BD.ObtenerSolicitudesLiga(idLiga, idUsuario);
        int cantidadMiembros = BD.ObtenerLeaderboardLiga(idLiga).Count;

        // El texto de cada solicitud (nombre y fecha) se arma acá para que la vista solo lo muestre
        foreach (SolicitudLiga solicitud in solicitudes)
        {
            solicitud.Detalle = solicitud.Nombre + " · pidió entrar el " + solicitud.FechaSolicitud.ToString("dd/MM/yyyy HH:mm");
        }

        ViewData["Title"] = "Solicitudes de " + liga.Nombre;
        ViewBag.IdLiga = liga.Id;
        ViewBag.UrlVolver = Url.Action("Index", "Liga", new { idLigaAmigos = liga.Id });
        ViewBag.TituloPagina = "Solicitudes para " + liga.Nombre;
        ViewBag.TextoLugares = cantidadMiembros + " de " + liga.Capacidad + " lugares ocupados";
        ViewBag.Solicitudes = solicitudes;
        ViewBag.OcultarVacio = solicitudes.Count > 0;

        string mensaje = LeerAviso("AvisoLiga");
        string error = LeerAviso("ErrorLiga");

        ViewBag.Mensaje = mensaje;
        ViewBag.OcultarMensaje = string.IsNullOrEmpty(mensaje);
        ViewBag.Error = error;
        ViewBag.OcultarError = string.IsNullOrEmpty(error);

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

        int resultado = BD.ResponderSolicitudLiga(idSolicitud, idUsuario, aceptar);

        if (resultado == 1)
        {
            GuardarAviso("AvisoLiga", "Solicitud aceptada. Ya está en tu liga.");
        }
        else if (resultado == 2)
        {
            GuardarAviso("AvisoLiga", "Solicitud vetada.");
        }
        else if (resultado == -2)
        {
            GuardarAviso("ErrorLiga", "La liga está llena. Para aceptar a alguien más tiene que liberarse un lugar.");
        }
        else
        {
            GuardarAviso("ErrorLiga", "No encontramos esa solicitud. Puede que ya la hayas respondido.");
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

        return BD.ObtenerIdUsuarioPorNombre(nombreUsuario);
    }

    // Si el texto es null lo cambia por "", y le saca los espacios de los costados.
    private string TextoLimpio(string texto)
    {
        if (texto == null)
        {
            return "";
        }

        return texto.Trim();
    }

    // Acepta el link completo (https://.../Liga/UnirseALiga?codigo=ABC123) o solo el código.
    private string ExtraerCodigoDeLink(string link)
    {
        string texto = link.Trim();
        string codigo = texto;

        // Si es un link, el código es lo que viene después de "codigo=" (hasta el próximo "&" o "#")
        if (texto.Contains("://"))
        {
            codigo = "";

            int posicionPregunta = texto.IndexOf('?');

            if (posicionPregunta != -1)
            {
                string parametros = "&" + texto.Substring(posicionPregunta + 1);
                int posicionCodigo = parametros.ToLower().IndexOf("&codigo=");

                if (posicionCodigo != -1)
                {
                    codigo = parametros.Substring(posicionCodigo + "&codigo=".Length);

                    int fin = codigo.IndexOf('&');
                    if (fin != -1)
                    {
                        codigo = codigo.Substring(0, fin);
                    }

                    fin = codigo.IndexOf('#');
                    if (fin != -1)
                    {
                        codigo = codigo.Substring(0, fin);
                    }
                }
            }
        }

        codigo = codigo.Trim().ToUpper();

        if (!EsCodigoValido(codigo))
        {
            return "";
        }

        return codigo;
    }

    // Un código válido tiene entre 1 y 12 caracteres, solo letras mayúsculas (A-Z) y números.
    private bool EsCodigoValido(string codigo)
    {
        string permitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        if (codigo == null || codigo.Length == 0 || codigo.Length > 12)
        {
            return false;
        }

        bool valido = true;

        foreach (char letra in codigo)
        {
            if (permitidos.IndexOf(letra) == -1)
            {
                valido = false;
            }
        }

        return valido;
    }

    // Prepara todo lo que muestra una tarjeta de "Unirse a una liga".
    // Antes estas decisiones estaban en la vista _TarjetaLiga.cshtml.
    // codigo: código del link de invitación ("" si se llegó buscando).
    // busqueda: texto buscado, para volver a la misma búsqueda después de enviar.
    private TarjetaLiga ArmarTarjeta(LigaBusqueda liga, string codigo, string busqueda)
    {
        TarjetaLiga tarjeta = new TarjetaLiga();

        tarjeta.Id = liga.Id;
        tarjeta.Nombre = liga.Nombre;
        tarjeta.Detalle = "Creada por " + liga.NombreCreador + " · " + liga.CantidadMiembros + "/" + liga.Capacidad + " miembros";
        tarjeta.Busqueda = busqueda;
        tarjeta.UrlVerLiga = Url.Action("Index", "Liga", new { idLigaAmigos = liga.Id });
        tarjeta.IdCampoContraseña = "contraseña-" + liga.Id;

        if (liga.Privacidad)
        {
            tarjeta.TextoEtiqueta = "Privada";
            tarjeta.ClaseEtiqueta = "unirse-etiqueta unirse-etiqueta-privada badge";
            tarjeta.TextoBoton = "Solicitar unirse";
            // Con el link de invitación no hace falta la contraseña, pero el dueño igual tiene que aceptar
            tarjeta.Codigo = codigo;
        }
        else
        {
            tarjeta.TextoEtiqueta = "Pública";
            tarjeta.ClaseEtiqueta = "unirse-etiqueta badge";
            tarjeta.TextoBoton = "Unirse";
            tarjeta.Codigo = "";
        }

        // De estas tres cosas se muestra una sola: el link a la liga, un texto de estado o el formulario
        tarjeta.OcultarVerLiga = !liga.EsMiembro;
        tarjeta.TextoEstado = "";

        if (!liga.EsMiembro && liga.SolicitudPendiente)
        {
            tarjeta.TextoEstado = "Solicitud enviada, esperando respuesta";
        }
        else if (!liga.EsMiembro && liga.EstaLlena)
        {
            tarjeta.TextoEstado = "Liga completa";
        }

        tarjeta.OcultarEstado = tarjeta.TextoEstado == "";
        tarjeta.OcultarFormulario = liga.EsMiembro || liga.SolicitudPendiente || liga.EstaLlena;

        // La contraseña se pide solo en ligas privadas a las que se llegó buscando (sin código)
        tarjeta.PideContraseña = !tarjeta.OcultarFormulario && liga.Privacidad && codigo == "";
        tarjeta.OcultarContraseña = !tarjeta.PideContraseña;

        if (tarjeta.PideContraseña)
        {
            tarjeta.ClaseFormulario = "unirse-form-accion";
        }
        else
        {
            tarjeta.ClaseFormulario = "unirse-form-accion unirse-form-solo-boton";
        }

        return tarjeta;
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

        ViewData["Title"] = "Crear Liga";
        ViewBag.Error = "";
        ViewBag.OcultarError = true;
        ViewBag.Liga = new Liga { Capacidad = 10 };

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

        if (liga.Privacidad && string.IsNullOrWhiteSpace(liga.Contraseña))
        {
            ViewData["Title"] = "Crear Liga";
            ViewBag.Error = "Las ligas privadas necesitan una contraseña.";
            ViewBag.OcultarError = false;
            ViewBag.Liga = liga;
            return View();
        }

        if (liga.Contraseña == null)
        {
            liga.Contraseña = "";
        }

        int idUsuario = BD.ObtenerIdUsuarioPorNombre(nombreUsuario);
        int idLigaCreada = BD.CrearLiga(liga, idUsuario);

        return RedirectToAction("Index", new { idLigaAmigos = idLigaCreada });
    }

    [HttpPost]
    public IActionResult Eliminar(int idLiga)
    {
        string nombreUsuario = ObtenerUsuarioActual();

        if (nombreUsuario == null)
        {
            return RedirectToAction("Index", "Login");
        }

        int idUsuario = BD.ObtenerIdUsuarioPorNombre(nombreUsuario);

        Liga liga = BD.ObtenerLigaPorId(idLiga);

        if (liga == null || liga.IdUsuarioCreador != idUsuario)
        {
            return RedirectToAction("Index");
        }

        BD.EliminarLiga(idLiga, idUsuario);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Abandonar(int idLiga)
    {
        string nombreUsuario = ObtenerUsuarioActual();

        if (nombreUsuario == null)
        {
            return RedirectToAction("Index", "Login");
        }

        int idUsuario = BD.ObtenerIdUsuarioPorNombre(nombreUsuario);

        Liga liga = BD.ObtenerLigaPorId(idLiga);

        if (liga == null || liga.IdUsuarioCreador == idUsuario)
        {
            return RedirectToAction("Index");
        }

        BD.AbandonarLiga(idLiga, idUsuario);

        return RedirectToAction("Index");
    }

    // Prepara las filas del ranking que muestra la vista Liga/Index.
    // Los tres primeros puestos tienen otro color (oro, plata y bronce) y un subtítulo.
    private List<FilaRanking> ArmarRanking(List<UsuarioRanking> lista)
    {
        List<FilaRanking> filas = new List<FilaRanking>();
        int posicion = 1;

        foreach (UsuarioRanking usuario in lista)
        {
            FilaRanking fila = new FilaRanking();
            fila.Posicion = posicion + "°";
            fila.Nombre = usuario.NombreUsuario;
            fila.Puntos = usuario.PuntajeTotal + " pts";

            if (posicion == 1)
            {
                fila.Clase = "ranking-fila ranking-fila-oro list-group-item";
                fila.Subtitulo = "🥇 CAMPEÓN";
            }
            else if (posicion == 2)
            {
                fila.Clase = "ranking-fila ranking-fila-plata ranking-fila-medalla list-group-item";
                fila.Subtitulo = "🥈 2° Lugar";
            }
            else if (posicion == 3)
            {
                fila.Clase = "ranking-fila ranking-fila-bronce ranking-fila-medalla list-group-item";
                fila.Subtitulo = "🥉 3° Lugar";
            }
            else
            {
                fila.Clase = "ranking-fila ranking-fila-resto list-group-item";
                fila.Subtitulo = "";
            }

            filas.Add(fila);
            posicion++;
        }

        return filas;
    }
}
