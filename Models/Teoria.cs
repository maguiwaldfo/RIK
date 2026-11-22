namespace RIK.Models;

public class Teoria
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string NombreImagen { get; set; }
    public bool EsBloqueada { get; set; }

    // Datos para mostrar la tarjeta. Los completa BibliotecaController.Index.
    public string Url { get; set; } = "";
    public string Imagen { get; set; } = "";
    public bool OcultarBloqueada { get; set; }
}