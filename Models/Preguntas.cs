namespace RIK.Models;

public class Pregunta
{
    public int Id { get; set; }
    public string Enunciado { get; set; }
    public int IDnivel { get; set; }
    public List<Opcion> Opciones { get; set; } = new List<Opcion>();


}