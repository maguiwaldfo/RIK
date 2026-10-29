namespace RIK.Models;
using System.Text.RegularExpressions;

public class Usuario
{
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
    public string Email { get; set; }


        public Usuario()
    {
        
    }

    public Usuario(string nombre, string nombreUsuario, string contraseña, string email)
    {
        this.Nombre = nombre;
        this.NombreUsuario = nombreUsuario;
        this.Contraseña = contraseña;
        this.Email = email;
    }

    // contraseñaOpcional: al modificar los datos se puede dejar la contraseña vacía para no cambiarla
    public static bool ValidarDatosRegistro(string nombre, string nombreUsuario, string contraseña, string email, bool contraseñaOpcional = false)
    {
        bool esValido = true;

        if (string.IsNullOrWhiteSpace(nombre) || !Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario) || !Regex.IsMatch(nombreUsuario, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$") || nombreUsuario.Length < 6)
        {
            esValido = false;
        }

        bool contraseñaVacia = string.IsNullOrEmpty(contraseña);
        if (!(contraseñaOpcional && contraseñaVacia) && (string.IsNullOrWhiteSpace(contraseña) || contraseña.Length < 8))
        {
            esValido = false;
        }
        if (string.IsNullOrWhiteSpace(email) || email.Length > 50 || !Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
        {
            esValido = false;
        }

        return esValido;
    }
}