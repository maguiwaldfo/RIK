
namespace RIK.Models;

public class Nivel
{
    BD bd = new BD();

    public int Id { get; set; }
    public string Tema { get; set; }
    public int CantDesafios { get; set; }


    public List<Pregunta> cargarPreguntas(int idNivel)
    {
        return bd.ObtenerPreguntasNivel(idNivel);
    }

    public List<Pregunta> agarrarPreguntas(List<Pregunta> preguntas)
    {
        List<Pregunta> preguntasElegidas = new List<Pregunta>();
        Random random = new Random();

        while(preguntasElegidas.Count < 8 && preguntas.Count > 0)
        {
            int numero = random.Next(0, preguntas.Count);
            preguntasElegidas.Add(preguntas[numero]);
            preguntas.RemoveAt(numero);
        }

        return preguntasElegidas;
    }
}
