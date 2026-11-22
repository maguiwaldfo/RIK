namespace RIK.Models;

// Una fila del ranking de Liga/Index, ya preparada por LigaController.
// La vista solo la muestra.
public class FilaRanking
{
    public string Clase { get; set; }       // "ranking-fila ranking-fila-oro", "-plata", "-bronce" o "-resto"
    public string Posicion { get; set; }    // "1°", "2°"...
    public string Nombre { get; set; }
    public string Subtitulo { get; set; }   // "🥇 CAMPEÓN", "🥈 2° Lugar", "🥉 3° Lugar" o vacío
    public string Puntos { get; set; }      // "800 pts"
}
