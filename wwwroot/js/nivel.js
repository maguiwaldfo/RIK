
let preguntaActual = 0;
let vidas = 3;
let puntos = 0;
let segundos = 0;
let respondida = false;
let comprobando = false;
let terminada = false;

document.getElementById("pregunta-0").style.display = "block";

function actualizarTiempo()
{
    if (terminada == false)
    {
        segundos++;

        let minutos = Math.floor(segundos / 60);
        let resto = segundos % 60;

        let textoMinutos = minutos.toString();
        let textoSegundos = resto.toString();

        if (minutos < 10)
        {
            textoMinutos = "0" + minutos;
        }

        if (resto < 10)
        {
            textoSegundos = "0" + resto;
        }

        document.getElementById("tiempo").innerHTML =
            "Tiempo: " + textoMinutos + ":" + textoSegundos;
    }
}

setInterval(actualizarTiempo, 1000);

function comprobar(idPregunta, idOpcion)
{
    if (respondida == true || comprobando == true || terminada == true)
    {
        return;
    }

    comprobando = true;

    fetch("/Juego/ComprobarRespuesta?idPregunta=" + idPregunta +
          "&idOpcion=" + idOpcion, {
        method: "POST"
    })
    .then(respuesta => respuesta.text())
    .then(resultado =>
    {
        comprobando = false;

        if (resultado == "Correcta")
        {
            respondida = true;
            puntos += 100;

            document.getElementById("resultado").innerHTML =
                "¡Correcta! +100 puntos";

            document.getElementById("puntos").innerHTML =
                "Puntos: " + puntos;

            document.getElementById("siguiente").style.display = "block";
        }
        else if (resultado == "Incorrecta")
        {
            vidas--;

            document.getElementById("resultado").innerHTML =
                "Incorrecta. Perdiste una vida. ¡Intentá otra vez!";

            mostrarVidas();
        }
        else if (resultado == "SinVidas")
        {
            vidas = 0;
            terminada = true;

            mostrarVidas();

            window.location.href = "/Juego/Resultado";
        }
        else
        {
            document.getElementById("resultado").innerHTML = resultado;
        }
    })
    .catch(error =>
    {
        comprobando = false;
        document.getElementById("resultado").innerHTML =
            "No se pudo comprobar la respuesta.";
    });
}

function mostrarVidas()
{
    let corazones = "";

    for (let i = 0; i < vidas; i++)
    {
        corazones += "❤️";
    }

    document.getElementById("vidas").innerHTML = "Vidas: " + corazones;
}

function siguientePregunta()
{
    if (respondida == false)
    {
        return;
    }

    document.getElementById("pregunta-" + preguntaActual).style.display = "none";

    preguntaActual++;

    document.getElementById("resultado").innerHTML = "";
    document.getElementById("siguiente").style.display = "none";

    respondida = false;

    if (preguntaActual < 8)
    {
        document.getElementById("pregunta-" + preguntaActual).style.display = "block";

        document.getElementById("contador").innerHTML =
            "Pregunta " + (preguntaActual + 1) + " de 8";
    }
    else
    {
        terminada = true;

        document.getElementById("contador").innerHTML =
            "¡Terminaste las preguntas!";

        document.getElementById("finalizar").style.display = "block";
    }
}

function finalizarNivel()
{
    document.getElementById("finalizar").style.display = "none";

    fetch("/Juego/FinalizarNivel", {
        method: "POST"
    })
    .then(respuesta => respuesta.text())
    .then(resultado =>
    {
        if (resultado == "Completado")
        {
            window.location.href = "/Juego/Resultado";
        }
        else
        {
            document.getElementById("resultado").innerHTML = resultado;
            document.getElementById("finalizar").style.display = "block";
        }
    })
    .catch(error =>
    {
        document.getElementById("resultado").innerHTML =
            "No se pudo guardar el nivel.";

        document.getElementById("finalizar").style.display = "block";
    });
}
