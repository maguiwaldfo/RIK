namespace RIK.Models;
using System.Text.RegularExpressions;

public class Usuario
{
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contrasena { get; set; }
    public string Email { get; set; }

    public Usuario(string nombre, string nombreUsuario, string contrasena, string email)
    {
        this.Nombre = nombre;
        this.NombreUsuario = nombreUsuario;
        this.Contrasena = contrasena;
        this.Email = email;
    }

    public static bool ValidarDatosRegistro(string nombre, string nombreUsuario, string contrasena, string email)
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
        else if (!Regex.IsMatch(nombreUsuario, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }
        else if (nombreUsuario.Length < 6)
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(contrasena))
        {
            esValido = false;
        }
        else if (contrasena.Length < 8)
        {
            esValido = false;
        }

        if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
        {
            esValido = false;
        }

        return esValido;
    }
}