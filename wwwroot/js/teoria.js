// Charla de teoría (Views/Juego/Teoria.cshtml): se muestra de a dos mensajes
// con los personajes de medio cuerpo y al final aparece el resumen completo.

let pasosCharla = [];
let pasoActual = 0;
let puedeComenzar = false;

function iniciarCharla() {
    let charla = document.getElementById("charla");
    if (charla == null) {
        return;
    }

    let resumen = document.getElementById("charlaResumen");
    let comenzar = document.getElementById("charlaComenzar");

    // El controller decide si "Comenzar desafío" se puede mostrar (si el nivel tiene preguntas suficientes).
    puedeComenzar = !comenzar.hidden;

    // Los mensajes salen del resumen que ya vino armado desde el servidor.
    let filas = resumen.getElementsByClassName("charla-fila");
    let mensajes = [];
    for (let i = 0; i < filas.length; i++) {
        let mensaje = {
            personaje: filas[i].getAttribute("data-personaje"),
            texto: filas[i].getElementsByClassName("charla-globo")[0].textContent
        };
        mensajes.push(mensaje);
    }

    if (mensajes.length == 0) {
        return;
    }

    // Se muestran de a dos (pregunta y respuesta).
    for (let i = 0; i < mensajes.length; i = i + 2) {
        let paso = [];
        paso.push(mensajes[i]);
        if (i + 1 < mensajes.length) {
            paso.push(mensajes[i + 1]);
        }
        pasosCharla.push(paso);
    }

    // Si al final queda un mensaje suelto, se suma al paso anterior.
    let ultimo = pasosCharla.length - 1;
    if (pasosCharla.length > 1 && pasosCharla[ultimo].length == 1) {
        pasosCharla[ultimo - 1].push(pasosCharla[ultimo][0]);
        pasosCharla.pop();
    }

    // Arranque: se esconde el resumen y se muestra el primer paso.
    resumen.hidden = true;
    comenzar.hidden = true;
    document.getElementById("charlaPaso").hidden = false;

    let siguiente = document.getElementById("charlaSiguiente");
    siguiente.hidden = false;
    siguiente.onclick = siguientePaso;

    mostrarPaso();
}

function mostrarPaso() {
    let charla = document.getElementById("charla");
    let contenedor = document.getElementById("charlaPaso");
    contenedor.innerHTML = "";

    let mensajes = pasosCharla[pasoActual];
    for (let i = 0; i < mensajes.length; i++) {
        let mensaje = mensajes[i];

        let fila = document.createElement("div");
        fila.className = "charla-fila charla-fila-" + mensaje.personaje;

        let imagen = document.createElement("img");
        imagen.className = "charla-cuerpo";
        if (mensaje.personaje == "chico") {
            imagen.src = charla.getAttribute("data-cuerpo-chico");
            imagen.alt = "Chico";
        } else {
            imagen.src = charla.getAttribute("data-cuerpo-perro");
            imagen.alt = "Riky";
        }

        let globo = document.createElement("p");
        globo.className = "charla-globo";
        globo.textContent = mensaje.texto;

        fila.appendChild(imagen);
        fila.appendChild(globo);
        contenedor.appendChild(fila);
    }
}

function siguientePaso() {
    pasoActual++;
    if (pasoActual < pasosCharla.length) {
        mostrarPaso();
    } else {
        mostrarResumen();
    }
}

function mostrarResumen() {
    document.getElementById("charlaPaso").hidden = true;
    document.getElementById("charlaSiguiente").hidden = true;
    document.getElementById("charlaResumen").hidden = false;
    document.getElementById("charlaComenzar").hidden = !puedeComenzar;
}

iniciarCharla();
