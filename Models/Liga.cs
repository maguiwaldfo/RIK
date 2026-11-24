namespace RIK.Models;

public class Liga
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Capacidad { get; set; }
    public bool Privacidad { get; set; }
    public string Contraseña { get; set; }
    public int IdUsuarioCreador { get; set; }
    public string CodigoInvitacion { get; set; }

    // Límites de los datos de una liga (los mismos que usa Liga.js en el cliente)
    public const int LargoMaximoTexto = 50;
    public const int CapacidadMinima = 2;
    public const int CapacidadMaxima = 50;

    // Valida los datos de una liga nueva. Devuelve el mensaje de error, o "" si está todo bien.
    public static string ValidarDatos(Liga liga)
    {
        string error = "";

        if (string.IsNullOrWhiteSpace(liga.Nombre))
        {
            error = "Escribí el nombre de la liga.";
        }
        else if (liga.Nombre.Length > LargoMaximoTexto)
        {
            error = "El nombre puede tener como máximo " + LargoMaximoTexto + " caracteres.";
        }
        else if (liga.Capacidad < CapacidadMinima || liga.Capacidad > CapacidadMaxima)
        {
            error = "La capacidad tiene que ser un número entre " + CapacidadMinima + " y " + CapacidadMaxima + ".";
        }
        else if (liga.Privacidad && string.IsNullOrWhiteSpace(liga.Contraseña))
        {
            error = "Las ligas privadas necesitan una contraseña.";
        }
        else if (liga.Contraseña != null && liga.Contraseña.Length > LargoMaximoTexto)
        {
            error = "La contraseña puede tener como máximo " + LargoMaximoTexto + " caracteres.";
        }

        return error;
    }
}
