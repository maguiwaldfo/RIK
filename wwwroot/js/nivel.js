let preguntaActual = 0;
let respondida = false;

document.getElementById("pregunta-0").style.display = "block";

function comprobar(idPregunta, respuestaElegida)
{
    if (respondida == false)
    {
        fetch("/Juego/ComprobarRespuesta?idPregunta=" + idPregunta +
              "&respuestaElegida=" + respuestaElegida, {
            method: "POST"
        })
        .then(respuesta => respuesta.text())
        .then(resultado =>
        {
            document.getElementById("resultado").innerHTML = resultado;

            if (resultado == "Correcta" || resultado == "Incorrecta")
            {
                respondida = true;
                document.getElementById("siguiente").style.display = "block";
            }
        });
    }
}

function siguientePregunta()
{
    document.getElementById("pregunta-" + preguntaActual).style.display = "none";

    preguntaActual++;

    document.getElementById("resultado").innerHTML = "";
    document.getElementById("siguiente").style.display = "none";

    respondida = false;

    if (preguntaActual < 8)
    {
        document.getElementById("pregunta-" + preguntaActual).style.display = "block";
    }
    else
    {
        document.getElementById("finalizar").style.display = "block";
    }
}

function finalizarNivel()
{
    fetch("/Juego/FinalizarNivel", {
        method: "POST"
    })
    .then(respuesta => respuesta.text())
    .then(resultado =>
    {
        if (resultado == "Completado")
        {
            window.location.href = "/Inicio/IndexInicio";
        }
        else
        {
            document.getElementById("resultado").innerHTML = resultado;
        }
    });
}