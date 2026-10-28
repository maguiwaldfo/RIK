function comprobar(idPregunta, respuestaElegida)
{
    let datos = new FormData();

    datos.append("idPregunta", idPregunta);
    datos.append("respuestaElegida", respuestaElegida);

    fetch("/Juego/ComprobarRespuesta", {
        method: "POST",
        body: datos
    })
    .then(respuesta => respuesta.text())
    .then(resultado =>
    {
        document.getElementById("resultado").innerText = resultado;
    });
}