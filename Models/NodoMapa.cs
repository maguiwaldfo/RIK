namespace RIK.Models;

// Una moneda del mapa de niveles, ya preparada por InicioController.
// La vista solo la dibuja.
public class NodoMapa
{
    public string Clase { get; set; } = "";
    public string Estilo { get; set; } = "";
    public string Url { get; set; }           // null = nivel que todavía no se puede abrir
    public string Imagen { get; set; } = "";
    public string Alt { get; set; } = "";
}
