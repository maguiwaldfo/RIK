namespace RIK.Models;

public class Nivel
{
    BD bd = new BD();
    public string Tema { get; set; }
    public int CantDesafios { get; set; }


    public List<Pregunta> cargarPreguntas(int idNivel)
    {
        return bd.ObtenerPreguntasNivel(idNivel);
    }

    public List<Pregunta> agarrarPreguntas(List<Pregunta> preguntas)
    {
        List<Pregunta> preguntasElegidas = new List<Pregunta>();

        for(int i = 0; i < 8; i++)
        {
            Random random = new Random();
            int numero = random.Next(0, preguntas.Count);
            Pregunta preguntaElegida = preguntas[numero];
            preguntasElegidas.Add(preguntaElegida);
            preguntas.RemoveAt(numero);
        }
        return preguntasElegidas;
    }
}