namespace RIK.Models;

// Datos ya preparados de una tarjeta de "Unirse a una liga".
// Los arma LigaController.ArmarTarjeta: la vista solo los muestra.
public class TarjetaLiga
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string TextoEtiqueta { get; set; } = "";
    public string ClaseEtiqueta { get; set; } = "";
    public string UrlVerLiga { get; set; } = "";
    public bool OcultarVerLiga { get; set; }
    public string TextoEstado { get; set; } = "";
    public bool OcultarEstado { get; set; }
    public bool OcultarFormulario { get; set; }
    public string ClaseFormulario { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Busqueda { get; set; } = "";
    public string IdCampoContraseña { get; set; } = "";
    public bool PideContraseña { get; set; }
    public bool OcultarContraseña { get; set; }
    public string TextoBoton { get; set; } = "";
}
