// El estado inicial viene de la session (si se refrescó, se retoma donde estaba).
const datosNivel = document.getElementById("nivel").dataset;

let preguntaActual = parseInt(datosNivel.preguntaActual);
let vidas = parseInt(datosNivel.vidas);
let puntos = parseInt(datosNivel.puntos);
let segundos = parseInt(datosNivel.segundos);
let respondida = false;
let comprobando = false;
let terminada = false;

// Opción marcada en la pregunta actual (todavía sin enviar).
let opcionElegida = null;

const totalPreguntas = document.querySelectorAll(".nivel-pregunta").length;

document.getElementById("pregunta-" + preguntaActual).style.display = "block";

function mostrarTiempo()
{
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
        textoMinutos + ":" + textoSegundos;
}

function actualizarTiempo()
{
    if (terminada == false)
    {
        segundos++;
        mostrarTiempo();
    }
}

mostrarTiempo();
setInterval(actualizarTiempo, 1000);

// La barra sube con cada pregunta acertada.
function mostrarProgreso(acertadas)
{
    let porcentaje = Math.round(acertadas * 100 / totalPreguntas);

    document.getElementById("barraProgreso").style.width = porcentaje + "%";
}

// Al tocar una opción solo queda marcada; se comprueba recién con "Enviar".
function elegirOpcion(boton, idPregunta, idOpcion)
{
    if (respondida == true || comprobando == true || terminada == true)
    {
        return;
    }

    if (opcionElegida != null)
    {
        opcionElegida.boton.classList.remove("nivel-opcion-elegida");
    }

    boton.classList.add("nivel-opcion-elegida");

    opcionElegida = {
        boton: boton,
        idPregunta: idPregunta,
        idOpcion: idOpcion
    };

    document.getElementById("resultado").innerHTML = "";
    document.getElementById("enviar").disabled = false;
}

function enviarRespuesta()
{
    if (opcionElegida == null)
    {
        return;
    }

    comprobar(opcionElegida.idPregunta, opcionElegida.idOpcion);
}

function comprobar(idPregunta, idOpcion)
{
    if (respondida == true || comprobando == true || terminada == true)
    {
        return;
    }

    comprobando = true;
    document.getElementById("enviar").disabled = true;

    let boton = opcionElegida != null ? opcionElegida.boton : null;

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
            opcionElegida = null;

            if (boton != null)
            {
                boton.classList.remove("nivel-opcion-elegida");
                boton.classList.add("nivel-opcion-correcta");
            }

            document.getElementById("resultado").innerHTML =
                "¡Correcta! +100 puntos";

            mostrarProgreso(preguntaActual + 1);

            document.getElementById("enviar").style.display = "none";

            if (preguntaActual + 1 < totalPreguntas)
            {
                document.getElementById("siguiente").style.display = "flex";
            }
            else
            {
                terminada = true;
                document.getElementById("finalizar").style.display = "flex";
            }
        }
        else if (resultado == "Incorrecta")
        {
            vidas--;
            opcionElegida = null;

            if (boton != null)
            {
                boton.classList.remove("nivel-opcion-elegida");
                boton.classList.add("nivel-opcion-incorrecta");
                boton.disabled = true;
            }

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
            document.getElementById("enviar").disabled = (opcionElegida == null);
        }
    })
    .catch(error =>
    {
        comprobando = false;
        document.getElementById("resultado").innerHTML =
            "No se pudo comprobar la respuesta.";
        document.getElementById("enviar").disabled = (opcionElegida == null);
    });
}

function mostrarVidas()
{
    document.getElementById("vidas").innerHTML = vidas;
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
    opcionElegida = null;

    document.getElementById("pregunta-" + preguntaActual).style.display = "block";

    document.getElementById("enviar").disabled = true;
    document.getElementById("enviar").style.display = "flex";
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
            document.getElementById("finalizar").style.display = "flex";
        }
    })
    .catch(error =>
    {
        document.getElementById("resultado").innerHTML =
            "No se pudo guardar el nivel.";

        document.getElementById("finalizar").style.display = "flex";
    });
}
