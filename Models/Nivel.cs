namespace RIK.Models;

public class Nivel
{
    public int Id { get; set; }
    public string Tema { get; set; }
    public int CantDesafios { get; set; }

    // Cantidad de preguntas que se juegan en cada partida (antes era un 8 escrito en varios lugares).
    public const int PreguntasPorPartida = 8;

    // Regla para saber si un nivel se puede abrir: el primero siempre,
    // y los demás si ya se completó el anterior (o si ya se completó ese mismo).
    // La usan JuegoController (teoría y nivel) y BibliotecaController.
    public static bool EstaDesbloqueado(int idNivel, List<int> completados)
    {
        bool desbloqueado = false;

        if (idNivel == 1 || completados.Contains(idNivel - 1) || completados.Contains(idNivel))
        {
            desbloqueado = true;
        }

        return desbloqueado;
    }
}
