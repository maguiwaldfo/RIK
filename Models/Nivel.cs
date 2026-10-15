namespace RIK.Models;

public class Nivel
{
    BD bd = new BD();
    public string Tema { get; set; }
    public int CantDesafios { get; set; }


    public List<Pregunta> cargarPreguntas(int idNivel)
    {
        List<Pregunta> Contenidos = new List<Pregunta>();
        foreach(Pregunta pregunta in bd.ObtenerPreguntasNivel(int idNivel))
        {
            Contenidos.Add(pregunta);
        }
        return Contenidos;
    }
}