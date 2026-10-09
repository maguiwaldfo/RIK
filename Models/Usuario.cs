namespace RIK.Models;
using System.Text.RegularExpressions;
public class Usuario
{
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }

    public Usuario(string nombre, string nombreUsuario, string contraseña)
    {
        Nombre = nombre;
        NombreUsuario = nombreUsuario;
        Contraseña = contraseña;
    }
    public static bool ValidarDatosRegistro(string nombre, string nombreUsuario, string contraseña)
    {
        bool esValido = true;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            esValido = false;
        }
        else if (!Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            esValido = false;
        }
        
        else if (nombreUsuario.Length < 6)
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(contraseña))
        {
            esValido = false;
        }
        else if (contraseña.Length < 8)
        {
            esValido = false;
        }

        return esValido;
    }
}