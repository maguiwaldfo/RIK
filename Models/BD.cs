using Microsoft.Data.SqlClient;
using Dapper;
using RIK.Models;

namespace RIK.Models;

public class BD
{
    private static string _connectionString = @"Server=.\SQLEXPRESS;DataBase=RIK;Integrated Security=True;TrustServerCertificate=True;";
 
    public void AgregarUsuario(Usuario usuario)
    {
        string nombre = usuario.Nombre;
        string nombreUsuario = usuario.NombreUsuario;
        string contraseña = usuario.Contraseña;
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Usuarios (nombre, nombreUsuario, contraseña, email)
                             VALUES (@nombre, @nombreUsuario, @contraseña, @email)";
            connection.Execute(query, new
            {
                nombre = nombre,
                nombreUsuario = nombreUsuario,
                contraseña = contraseña,
                email = usuario.Email
            });
        }
    }

    public bool FijarseSiExisteUsuario(string nombreUsuario)
    {
        bool existe = false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT COUNT(*) FROM Usuarios WHERE nombreUsuario = @nombreUsuario";

            //esto que sigue me va a devolver un numero que es la cantidad de usuarios con ese nombre de usuario que es la pk
            
            int count = connection.QueryFirstOrDefault<int>(query, new { nombreUsuario = nombreUsuario });
            
            if (count > 0)
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
            string query = "SELECT * FROM Usuarios WHERE nombreUsuario = @nombreUsuario AND contraseña = @contraseña";
            //esto me va a devolver un objeto de tipo usuario que es el que tiene ese nombre de usuario y esa contraseña tipo para poder llevarlo a la vista y mostrarlo en la pagina de inicio
            usuario = connection.QueryFirstOrDefault<Usuario>(query, new { nombreUsuario, contraseña }); 
        }
        return usuario;
    }

     // Recibe el ID de un nivel y devuelve un objeto Nivel con sus datos.
    public Nivel ObtenerNivel(int idNivel)
    {
       Nivel nivel = null;
       using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Niveles WHERE id = @idNivel";
            nivel = connection.QueryFirstOrDefault<Nivel>(
            query, new { idNivel = idNivel });
        }

        return nivel;
    }

    // Devuelve una lista con todas las preguntas de un nivel.

     public List<Pregunta> ObtenerPreguntasNivel(int idNivel)
     {
         List<Pregunta> preguntas = new List<Pregunta>();

         using(SqlConnection connection = new SqlConnection(_connectionString))
         {
            string query = @"SELECT id, pregunta AS Enunciado, respuesta, IDnivel, opcion1, opcion2, opcion3, opcion4 FROM Preguntas WHERE IDnivel = @idNivel ORDER BY id";

            preguntas = connection.Query<Pregunta>(query, new{idNivel = idNivel}).ToList();
    }

        return preguntas;
     }

// Devuelve una pregunta según su ID, o null si no existe.
     public Pregunta ObtenerPregunta(int idPregunta)
     {
         Pregunta pregunta = null;

        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT id, pregunta AS Enunciado,respuesta, IDnivel, opcion1, opcion2, opcion3, opcion4 FROM Preguntas WHERE id = @idPregunta";
            pregunta = connection.QueryFirstOrDefault<Pregunta>(
            query, new { idPregunta = idPregunta });
        }

        return pregunta;
     }

     // Devuelve el ID de un usuario según su nombre de usuario, o 0 si no existe.
    public int ObtenerIdUsuario(string nombreUsuario)
    {
        int idUsuario = 0;

        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
             string query = @"SELECT id FROM Usuarios WHERE nombreUsuario = @nombreUsuario";
             idUsuario = connection.QueryFirstOrDefault<int>(query, new{nombreUsuario = nombreUsuario});
        }

        return idUsuario;
    }

    // Guarda el nivel completado por un usuario.
    public void GuardarNivelCompletado(int idUsuario, int idNivel)
    {
        using(SqlConnection connection = new SqlConnection(_connectionString))
        {
        string query = @"INSERT INTO UsuarioXNivel(idUsuario, idNiveles) SELECT @idUsuario, @idNivel WHERE NOT EXISTS
                         (SELECT 1 FROM UsuarioXNivel WHERE idUsuario = @idUsuario AND idNiveles = @idNivel)";
        connection.Execute(query, new{idUsuario = idUsuario, idNivel = idNivel});
        }
    }
}
