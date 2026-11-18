namespace RIK.Models;

// Liga tal como se muestra al buscar o al abrir un link de invitación.
// No tiene la contraseña: nunca sale de la base hacia la vista.
public class LigaBusqueda
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Capacidad { get; set; }
    public bool Privacidad { get; set; }
    public string NombreCreador { get; set; }
    public int CantidadMiembros { get; set; }
    public bool EsMiembro { get; set; }
    public bool SolicitudPendiente { get; set; }

    public bool EstaLlena
    {
        get { return CantidadMiembros >= Capacidad; }
    }
}
