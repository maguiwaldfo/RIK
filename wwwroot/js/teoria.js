let mensajeActual = 0;

function mostrarMensaje()
{
    document.getElementById("mensaje-" + mensajeActual).style.display = "block";

    mensajeActual++;

    if (document.getElementById("mensaje-" + mensajeActual) == null)
    {
        document.getElementById("siguienteMensaje").style.display = "none";
        document.getElementById("comenzar").style.display = "block";
    }
}