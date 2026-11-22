namespace RIK.Models;

public class CharlaTeoria
{
    public int Id { get; set; }
    public int IdNivel { get; set; }
    public int Orden { get; set; }
    public string Personaje { get; set; }
    public string Texto { get; set; }

    // Datos para mostrar el mensaje. Los completa JuegoController.Teoria.
    public string ClaseFila { get; set; } = "";
    public string CodigoPersonaje { get; set; } = "";
    public string ImagenCara { get; set; } = "";
}