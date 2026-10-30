
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using RIK.Models;

namespace RIK.Models;

public class BD
{
    private string _connectionString = @"Server=localhost;Database=RIK;Integrated Security=True;TrustServerCertificate=True;";

    // USUARIOS

    public void AgregarUsuario(Usuario usuario)
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

    public bool ModificarUsuario(int idUsuario, Usuario usuario)
    {
        int modificados = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            modificados = connection.QueryFirstOrDefault<int>(
                "sp_ModificarUsuario",
                new
                {
                    id = idUsuario,
                    nombre = usuario.Nombre,
                    nombreUsuario = usuario.NombreUsuario,
                    contraseña = usuario.Contraseña,
                    email = usuario.Email
                },
                commandType: CommandType.StoredProcedure
            );
        }

        return modificados > 0;
    }

    public bool FijarseSiExisteUsuario(string nombreUsuario)
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

    public Usuario ObtenerUsuario(string nombreUsuario, string contraseña)
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

    public int ObtenerIdUsuario(string nombreUsuario)
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

    public Nivel ObtenerNivel(int idNivel)
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

    public Nivel ObtenerNivelPorNombre(string tema)
    {
        Nivel nivel = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            nivel = connection.QueryFirstOrDefault<Nivel>(
                "sp_ObtenerNivelPorNombre",
                new { tema = tema },
                commandType: CommandType.StoredProcedure
            );
        }

        return nivel;
    }

    public List<Nivel> ObtenerNiveles()
    {
        List<Nivel> niveles = new List<Nivel>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            niveles = connection.Query<Nivel>(
                "sp_ObtenerNiveles",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return niveles;
    }

    public List<int> ObtenerNivelesCompletados(int idUsuario)
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

    public void GuardarNivelCompletado(int idUsuario, int idNivel, int puntaje)
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

    public List<Pregunta> ObtenerPreguntasNivel(int idNivel)
    {
        List<Pregunta> preguntas = new List<Pregunta>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            preguntas = connection.Query<Pregunta>(
                "sp_ObtenerPreguntasNivel",
                new { idNivel = idNivel },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        foreach (Pregunta pregunta in preguntas)
        {
            pregunta.Opciones = ObtenerOpcionesPregunta(pregunta.Id);
        }

        return preguntas;
    }

    public Pregunta ObtenerPregunta(int idPregunta)
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

    // OPCIONES

    public List<Opcion> ObtenerOpcionesPregunta(int idPregunta)
    {
        List<Opcion> opciones = new List<Opcion>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            opciones = connection.Query<Opcion>(
                "sp_ObtenerOpcionesPregunta",
                new { idPregunta = idPregunta },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        return opciones;
    }

    public bool ComprobarOpcion(int idPregunta, int idOpcion)
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

    public List<Teoria> ObtenerTeorias()
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

    public List<CharlaTeoria> ObtenerCharlaTeoria(int idNivel)
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
}
