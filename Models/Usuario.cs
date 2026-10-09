namespace RIK.Models;
using System.Text.RegularExpressions;
public class Usuario
{
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }

    public Usuario(string nombre, string nombreUsuario, string contraseña, string email, string telefono)
    {
        this.Nombre = nombre;
        this.NombreUsuario = nombreUsuario;
        this.Contraseña = contraseña;
        this.Email = email;
        this.Telefono = telefono;
    }
    public static bool ValidarDatosRegistro(string nombre, string nombreUsuario, string contraseña, string email, string telefono)
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

        if (string.IsNullOrWhiteSpace(contraseña))
        {
            esValido = false;
        }
        else if (contraseña.Length < 8)
        {
            esValido = false;
        }
        if (!Regex.IsMatch(email, " ^[ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(telefono))
        {
            esValido = false;
        }
        else if (!Regex.IsMatch(telefono, "^\\d+$"))
        {
            esValido = false;
        }

        return esValido;
    }
}