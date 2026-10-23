using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using RIK.Models;

namespace RIK.Models;

public class BD
{
    private static string _connectionString = @"Server=.\SQLEXPRESS;DataBase=RIK;Integrated Security=True;TrustServerCertificate=True;";

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
                new { nombreUsuario, contraseña },
                commandType: CommandType.StoredProcedure
            );
        }

        return usuario;
    }

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

    public void GuardarNivelCompletado(int idUsuario, int idNivel)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(
                "sp_GuardarNivelCompletado",
                new { idUsuario = idUsuario, idNivel = idNivel },
                commandType: CommandType.StoredProcedure
            );
        }
    }

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

    public List<CharlaTeoria> ObtenerCharlaTeoria(string idNivel)
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
}