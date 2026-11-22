namespace RIK.Models;

public class Pregunta
{
    public int Id { get; set; }
    public string Enunciado { get; set; }
    public int IDnivel { get; set; }
    public List<Opcion> Opciones { get; set; } = new List<Opcion>();

    // id del <div> de la pregunta en la vista ("pregunta-0", "pregunta-1"...). Lo completa JuegoController.Nivel.
    public string IdHtml { get; set; } = "";


}