namespace RIK.Models;

// Pedido de un usuario para entrar a una liga privada.
public class SolicitudLiga
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
    public string Nombre { get; set; }
    public DateTime FechaSolicitud { get; set; }

    // Texto listo para mostrar (nombre y fecha). Lo arma LigaController.Solicitudes.
    public string Detalle { get; set; } = "";
}
