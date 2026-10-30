namespace RIK.Models;

public class Opcion
{
    public int Id { get; set; }
    public int IdPregunta { get; set; }
    public string Texto { get; set; }
    public bool EsCorrecta { get; set; }
}