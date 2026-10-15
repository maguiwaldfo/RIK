namespace RIK.Models;

public class Pregunta
{
    public int Id { get; set; }
    public string Enunciado { get; set; }
    public int Respuesta { get; set; }
    public int IDnivel { get; set; }

    public string Opcion1 { get; set; }
    public string Opcion2 { get; set; }
    public string Opcion3 { get; set; }
    public string Opcion4 { get; set; }
}