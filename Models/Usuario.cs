namespace RIK.Models;

public class Usuario
{
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
    public string Email { get; set; }

    private const string Letras = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LetrasConAcentos = Letras + "áéíóúÁÉÍÓÚñÑ";
    private const string Numeros = "0123456789";

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

        if (string.IsNullOrWhiteSpace(nombre) || !SoloLetrasYEspacios(nombre))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario) || !SoloLetrasYEspacios(nombreUsuario) || nombreUsuario.Length < 6)
        {
            esValido = false;
        }

        bool contraseñaVacia = string.IsNullOrEmpty(contraseña);
        if (!(contraseñaOpcional && contraseñaVacia) && (string.IsNullOrWhiteSpace(contraseña) || contraseña.Length < 8))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(email) || email.Length > 50 || !EmailValido(email))
        {
            esValido = false;
        }

        return esValido;
    }

    // true si el texto tiene solo letras (con acentos y ñ) y espacios
    private static bool SoloLetrasYEspacios(string texto)
    {
        bool valido = true;

        foreach (char letra in texto)
        {
            if (LetrasConAcentos.IndexOf(letra) == -1 && !char.IsWhiteSpace(letra))
            {
                valido = false;
            }
        }

        return valido;
    }

    // true si el texto no está vacío y todas sus letras están en "permitidos"
    private static bool TieneSolo(string texto, string permitidos)
    {
        bool valido = texto.Length > 0;

        foreach (char letra in texto)
        {
            if (permitidos.IndexOf(letra) == -1)
            {
                valido = false;
            }
        }

        return valido;
    }

    // Formato nombre@dominio.com: un solo @, algo antes, y después del último punto al menos 2 letras
    private static bool EmailValido(string email)
    {
        int posicionArroba = email.IndexOf('@');

        if (posicionArroba <= 0 || email.IndexOf('@', posicionArroba + 1) != -1)
        {
            return false;
        }

        string usuario = email.Substring(0, posicionArroba);
        string dominio = email.Substring(posicionArroba + 1);

        if (!TieneSolo(usuario, Letras + Numeros + "._%+-") || !TieneSolo(dominio, Letras + Numeros + ".-"))
        {
            return false;
        }

        int posicionPunto = dominio.LastIndexOf('.');

        if (posicionPunto <= 0)
        {
            return false;
        }

        string terminacion = dominio.Substring(posicionPunto + 1);

        return terminacion.Length >= 2 && TieneSolo(terminacion, Letras);
    }
}
