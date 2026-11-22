using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
namespace RIK.Models;

// Clase estática: se usa directo con BD.Metodo(...), sin crear un objeto.
public static class BD
{
    private static string _connectionString = @"Server=localhost;Database=RIK;Integrated Security=True;TrustServerCertificate=True;";

    // USUARIOS

    public static void AgregarUsuario(Usuario usuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_AgregarUsuario",
                new
                {
                    nombre = usuario.Nombre,
                    nombreUsuario = usuario.NombreUsuario,
                    contraseña = usuario.Contraseña,
                    email = usuario.Email
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public static int ModificarUsuario(int idUsuario, Usuario usuario)
    {
        int resultado = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            resultado = connection.QueryFirstOrDefault<int>(
                "sp_ModificarUsuario",
                new
                {
                    id = idUsuario,
                    nombre = usuario.Nombre,
                    nombreUsuario = usuario.NombreUsuario,
                    email = usuario.Email
                },
                commandType: CommandType.StoredProcedure
            );
        }

        return resultado;
    }

    public static int CambiarContrasena(int idUsuario, string contrasenaActual, string nuevaContrasena, string repetirContrasena)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.QueryFirstOrDefault<int>(
                "sp_CambiarContrasena",
                new
                {
                    idUsuario = idUsuario,
                    contrasenaActual = contrasenaActual,
                    nuevaContrasena = nuevaContrasena,
                    repetirContrasena = repetirContrasena
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public static bool FijarseSiExisteUsuario(string nombreUsuario)
    {
        bool existe = false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            int resultado = connection.QueryFirstOrDefault<int>(
                "sp_FijarseSiExisteUsuario",
                new { nombreUsuario = nombreUsuario },
                commandType: CommandType.StoredProcedure
            );

            if (resultado > 0)
            {
                existe = true;
            }
        }

        return existe;
    }

    public static Usuario ObtenerUsuario(string nombreUsuario, string contraseña)
    {
        Usuario usuario = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            usuario = connection.QueryFirstOrDefault<Usuario>(
                "sp_ObtenerUsuario",
                new { nombreUsuario = nombreUsuario, contraseña = contraseña },
                commandType: CommandType.StoredProcedure
            );
        }

        return usuario;
    }

    public static int ObtenerIdUsuario(string nombreUsuario)
    {
        int idUsuario = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            idUsuario = connection.QueryFirstOrDefault<int>(
                "sp_ObtenerIdUsuario",
                new { nombreUsuario = nombreUsuario },
                commandType: CommandType.StoredProcedure
            );
        }

        return idUsuario;
    }

    // NIVELES

    public static Nivel ObtenerNivel(int idNivel)
    {
        Nivel nivel = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            nivel = connection.QueryFirstOrDefault<Nivel>(
                "sp_ObtenerNivel",
                new { idNivel = idNivel },
                commandType: CommandType.StoredProcedure
            );
        }

        return nivel;
    }

    public static List<Nivel> ObtenerNiveles()
    {
        List<Nivel> niveles = new List<Nivel>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            niveles = connection.Query<Nivel>(
                "sp_ObtenerNiveles",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        // Se ordenan por Id (el procedure no tiene ORDER BY) para que el mapa los muestre en orden.
        for (int i = 0; i < niveles.Count - 1; i++)
        {
            for (int j = 0; j < niveles.Count - 1 - i; j++)
            {
                if (niveles[j].Id > niveles[j + 1].Id)
                {
                    Nivel auxiliar = niveles[j];
                    niveles[j] = niveles[j + 1];
                    niveles[j + 1] = auxiliar;
                }
            }
        }

        return niveles;
    }

    public static List<int> ObtenerNivelesCompletados(int idUsuario)
    {
        List<int> niveles = new List<int>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            niveles = connection.Query<int>(
                "sp_ObtenerNivelesCompletados",
                new { idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return niveles;
    }

    public static void GuardarNivelCompletado(int idUsuario, int idNivel, int puntaje)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_GuardarNivelCompletado",
                new
                {
                    idUsuario = idUsuario,
                    idNivel = idNivel,
                    puntaje = puntaje
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    // PREGUNTAS

    public static List<Pregunta> ObtenerPreguntasNivel(int idNivel)
    {
        List<Pregunta> preguntas;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            preguntas = connection.Query<Pregunta>(
                "sp_ObtenerPreguntasNivel",
                new { idNivel = idNivel },
                commandType: CommandType.StoredProcedure
            ).ToList();

            foreach (Pregunta pregunta in preguntas)
            {
                pregunta.Opciones = connection.Query<Opcion>(
                    "sp_ObtenerOpcionesPregunta",
                    new { idPregunta = pregunta.Id },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        return preguntas;
    }

    public static Pregunta ObtenerPregunta(int idPregunta)
    {
        Pregunta pregunta = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            pregunta = connection.QueryFirstOrDefault<Pregunta>(
                "sp_ObtenerPregunta",
                new { idPregunta = idPregunta },
                commandType: CommandType.StoredProcedure
            );
        }

        return pregunta;
    }

    public static bool ComprobarOpcion(int idPregunta, int idOpcion)
    {
        bool esCorrecta = false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            esCorrecta = connection.QueryFirstOrDefault<bool>(
                "sp_ComprobarOpcion",
                new
                {
                    idPregunta = idPregunta,
                    idOpcion = idOpcion
                },
                commandType: CommandType.StoredProcedure
            );
        }

        return esCorrecta;
    }

    // TEORIAS

    public static List<Teoria> ObtenerTeorias()
    {
        List<Teoria> teorias = new List<Teoria>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            teorias = connection.Query<Teoria>(
                "sp_ObtenerTeorias",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return teorias;
    }

    public static List<CharlaTeoria> ObtenerCharlaTeoria(int idNivel)
    {
        List<CharlaTeoria> charla = new List<CharlaTeoria>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            charla = connection.Query<CharlaTeoria>(
                "sp_ObtenerCharlaTeoria",
                new { idNivel = idNivel },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return charla;
    }

    public static List<TeoriaMoraleja> ObtenerTeoriaMoraleja(int idTeoria)
    {
        List<TeoriaMoraleja> lista = new List<TeoriaMoraleja>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            lista = connection.Query<TeoriaMoraleja>(
                "sp_ObtenerTeoriaMoraleja",
                new { idTeoria = idTeoria },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return lista;
    }

    // LIGAS Y LEADERBOARD

    public static List<UsuarioRanking> ObtenerLeaderboardGlobal()
    {
        List<UsuarioRanking> ranking = new List<UsuarioRanking>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            ranking = connection.Query<UsuarioRanking>(
                "sp_ObtenerLeaderboardGlobal",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return ranking;
    }

    public static List<Liga> ObtenerLigasDeUsuario(int idUsuario)
    {
        List<Liga> ligas = new List<Liga>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            ligas = connection.Query<Liga>(
                "sp_ObtenerLigasDeUsuario",
                new { idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return ligas;
    }

    public static List<UsuarioRanking> ObtenerLeaderboardLiga(int idLiga)
    {
        List<UsuarioRanking> ranking = new List<UsuarioRanking>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            ranking = connection.Query<UsuarioRanking>(
                "sp_ObtenerLeaderboardLiga",
                new { idLiga = idLiga },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return ranking;
    }

    public static Liga ObtenerLigaPorId(int idLiga)
    {
        Liga liga = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            liga = connection.QueryFirstOrDefault<Liga>(
                "sp_ObtenerLigaPorId",
                new { idLiga = idLiga },
                commandType: CommandType.StoredProcedure
            );
        }

        return liga;
    }

    public static int CrearLiga(Liga liga, int idUsuarioCreador)
    {
        int idLigaCreada = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            idLigaCreada = connection.QueryFirstOrDefault<int>(
                "sp_CrearLiga",
                new
                {
                    nombre = liga.Nombre,
                    capacidad = liga.Capacidad,
                    privacidad = liga.Privacidad,
                    contraseña = liga.Contraseña,
                    idUsuarioCreador = idUsuarioCreador
                },
                commandType: CommandType.StoredProcedure
            );
        }

        UnirUsuarioALiga(idLigaCreada, idUsuarioCreador);

        return idLigaCreada;
    }

    public static void UnirUsuarioALiga(int idLiga, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_UnirUsuarioALiga",
                new
                {
                    idLiga = idLiga,
                    idUsuario = idUsuario
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public static int ObtenerIdUsuarioPorNombre(string nombreUsuario)
    {
        int idUsuario = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            idUsuario = connection.QueryFirstOrDefault<int>(
                "sp_ObtenerIdUsuarioPorNombre",
                new { nombreUsuario = nombreUsuario },
                commandType: CommandType.StoredProcedure
            );
        }

        return idUsuario;
    }

    public static void EliminarLiga(int idLiga, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_EliminarLiga",
                new
                {
                    idLiga = idLiga,
                    idUsuario = idUsuario
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public static void AbandonarLiga(int idLiga, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_AbandonarLiga",
                new
                {
                    idLiga = idLiga,
                    idUsuario = idUsuario
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    // UNIRSE A LIGAS Y SOLICITUDES

    public static List<LigaBusqueda> BuscarLigas(string nombre, int idUsuario)
    {
        List<LigaBusqueda> ligas = new List<LigaBusqueda>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            ligas = connection.Query<LigaBusqueda>(
                "sp_BuscarLigas",
                new { nombre = nombre, idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return ligas;
    }

    public static LigaBusqueda ObtenerLigaPorCodigo(string codigo, int idUsuario)
    {
        LigaBusqueda liga = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            liga = connection.QueryFirstOrDefault<LigaBusqueda>(
                "sp_ObtenerLigaPorCodigo",
                new { codigo = codigo, idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            );
        }

        return liga;
    }

    // Devuelve el código de sp_SolicitarUnirseALiga (1 entró, 2 solicitud enviada, <= 0 error)
    public static int SolicitarUnirseALiga(int idLiga, int idUsuario, string contraseña, string codigo)
    {
        int resultado = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            resultado = connection.QueryFirstOrDefault<int>(
                "sp_SolicitarUnirseALiga",
                new
                {
                    idLiga = idLiga,
                    idUsuario = idUsuario,
                    contraseña = contraseña,
                    codigo = codigo
                },
                commandType: CommandType.StoredProcedure
            );
        }

        return resultado;
    }

    public static List<SolicitudLiga> ObtenerSolicitudesLiga(int idLiga, int idUsuarioCreador)
    {
        List<SolicitudLiga> solicitudes = new List<SolicitudLiga>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            solicitudes = connection.Query<SolicitudLiga>(
                "sp_ObtenerSolicitudesLiga",
                new { idLiga = idLiga, idUsuarioCreador = idUsuarioCreador },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return solicitudes;
    }

    // Devuelve el código de sp_ResponderSolicitudLiga (1 aceptada, 2 vetada, <= 0 error)
    public static int ResponderSolicitudLiga(int idSolicitud, int idUsuarioCreador, bool aceptar)
    {
        int resultado = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            resultado = connection.QueryFirstOrDefault<int>(
                "sp_ResponderSolicitudLiga",
                new
                {
                    idSolicitud = idSolicitud,
                    idUsuarioCreador = idUsuarioCreador,
                    aceptar = aceptar
                },
                commandType: CommandType.StoredProcedure
            );
        }

        return resultado;
    }
    
    public static void ActualizarRacha(int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_ActualizarRacha",
                new { idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public static int ObtenerRacha(int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.QueryFirstOrDefault<int>(
                "sp_ObtenerRacha",
                new { idUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}