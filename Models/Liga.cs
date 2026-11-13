namespace RIK.Models;

public class Liga
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Capacidad { get; set; }
    public bool Privacidad { get; set; }
    public string Contraseña { get; set; }
    public int IdUsuarioCreador { get; set; }
}