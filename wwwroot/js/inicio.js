// Mapa de niveles (Views/Inicio/IndexInicio.cshtml): el camino con las monedas
// se puede mover con la rueda del mouse o arrastrando con el mouse o el dedo.

let mapa = document.getElementById("inicioMapa");
let lienzo = document.getElementById("inicioLienzo");

let avance = 0;           // 0 = inicio del camino, 1 = final
let arrastre = null;      // datos del arrastre en curso
let huboArrastre = false; // para no abrir un nivel al soltar un arrastre

// Punto de la pantalla donde se "apoya" el camino: el lugar del nivel 1
// cuando recién se empieza (abajo a la izquierda).
let ANCLA_X = 0.092;
let ANCLA_Y = 0.813;

// El camino sube en diagonal: al avanzar, la línea y las monedas
// bajan hacia la izquierda. El resto de la pantalla no se toca.
function recorrido() {
    return {
        x: Math.max(1, lienzo.offsetWidth - mapa.clientWidth),
        y: Math.max(1, lienzo.offsetHeight - mapa.clientHeight)
    };
}

// Altura del camino (dentro de la capa) en una posición horizontal dada,
// tomando como referencia el centro de las monedas.
function alturaCamino(x) {
    let nodos = lienzo.getElementsByClassName("inicio-nodo");

    if (nodos.length == 0) {
        return 0;
    }
    if (x <= nodos[0].offsetLeft) {
        return nodos[0].offsetTop;
    }
    for (let i = 1; i < nodos.length; i++) {
        let a = nodos[i - 1];
        let b = nodos[i];
        if (x <= b.offsetLeft) {
            let p = (x - a.offsetLeft) / (b.offsetLeft - a.offsetLeft);
            return a.offsetTop + (b.offsetTop - a.offsetTop) * p;
        }
    }
    return nodos[nodos.length - 1].offsetTop;
}

// Cuánto tiene que bajar la capa para que el camino pase por el ancla
// cuando la capa está corrida x píxeles hacia la izquierda.
function bajadaEnAncla(x) {
    let y = lienzo.offsetHeight - (1 - ANCLA_Y) * mapa.clientHeight
            - alturaCamino(x + ANCLA_X * mapa.clientWidth);
    return Math.max(0, Math.min(recorrido().y, y));
}

// La capa se mueve en línea recta (en diagonal) desde el principio hasta el final
// del camino: la bajada es proporcional al avance, sin seguir cada subida y bajada
// entre monedas.
function mover(valor) {
    let r = recorrido();
    avance = Math.max(0, Math.min(1, valor));
    let x = avance * r.x;
    let yInicio = bajadaEnAncla(0);
    let yFin = bajadaEnAncla(r.x);
    let y = yInicio + (yFin - yInicio) * avance;
    lienzo.style.transform = "translate(" + (-x) + "px, " + y + "px)";
}

// true si el elemento está dentro de los paneles fijos (biblioteca, racha, desempeño, mascota)
function estaEnPaneles(elemento) {
    while (elemento != null) {
        if (elemento.className == "inicio-pantalla") {
            return true;
        }
        elemento = elemento.parentElement;
    }
    return false;
}

// Rueda del mouse: hacia arriba avanza por el camino (que sube), hacia abajo vuelve.
// Por eso deltaY va con el signo cambiado. El scroll horizontal queda igual.
function moverConRueda(e) {
    e.preventDefault();
    let delta = -e.deltaY;
    if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) {
        delta = e.deltaX;
    }
    if (e.deltaMode == 1) {
        delta = delta * 40;
    }
    mover(avance + delta / recorrido().x);
}

// Arrastre con mouse o dedo.
function empezarArrastre(e) {
    if (e.button != 0 || estaEnPaneles(e.target)) {
        return;
    }
    arrastre = { id: e.pointerId, x: e.clientX, y: e.clientY, inicio: avance, activo: false };
    huboArrastre = false;
}

function arrastrar(e) {
    if (arrastre == null || e.pointerId != arrastre.id) {
        return;
    }
    let dx = e.clientX - arrastre.x;
    let dy = e.clientY - arrastre.y;
    if (arrastre.activo == false) {
        // Recién se cuenta como arrastre si se movió más de 6 píxeles
        if (Math.abs(dx) + Math.abs(dy) < 6) {
            return;
        }
        arrastre.activo = true;
        huboArrastre = true;
        mapa.className = "inicio-mapa inicio-arrastrando";
        mapa.setPointerCapture(arrastre.id);
    }
    let r = recorrido();
    mover(arrastre.inicio + (dy - dx) / (r.x + r.y));
}

function soltar(e) {
    if (arrastre == null || e.pointerId != arrastre.id) {
        return;
    }
    arrastre = null;
    mapa.className = "inicio-mapa";
}

// Si se terminó de arrastrar sobre una moneda, no se abre el nivel
function evitarClickDespuesDeArrastrar(e) {
    if (huboArrastre) {
        e.preventDefault();
        e.stopPropagation();
        huboArrastre = false;
    }
}

function iniciarMapa() {
    if (mapa == null || lienzo == null) {
        return;
    }

    mapa.onwheel = moverConRueda;
    mapa.onpointerdown = empezarArrastre;
    mapa.onpointermove = arrastrar;
    mapa.onpointerup = soltar;
    mapa.onpointercancel = soltar;
    mapa.onclick = evitarClickDespuesDeArrastrar;
    mapa.ondragstart = function (e) {
        e.preventDefault();
    };
    window.onresize = function () {
        mover(avance);
    };

    // Al entrar, el mapa arranca en el nivel que toca jugar: esa moneda queda
    // abajo a la izquierda, donde está el nivel 1 cuando recién se empieza.
    let actual = lienzo.getElementsByClassName("inicio-nodo-actual")[0];
    if (actual != null) {
        lienzo.style.transition = "none";
        mover((actual.offsetLeft - mapa.clientWidth * ANCLA_X) / recorrido().x);
        lienzo.offsetWidth; // obliga al navegador a aplicar el movimiento antes de volver a activar la transición
        lienzo.style.transition = "";
    }
}

iniciarMapa();
