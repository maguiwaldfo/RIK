// Charla de teoría (Views/Juego/Teoria.cshtml): se muestra de a dos mensajes
// con los personajes de medio cuerpo y al final aparece el resumen completo.
(function () {
    var charla = document.getElementById('charla');
    if (!charla) return;

    var paso = document.getElementById('charlaPaso');
    var resumen = document.getElementById('charlaResumen');
    var siguiente = document.getElementById('charlaSiguiente');
    var comenzar = document.getElementById('charlaComenzar');

    // Los mensajes salen del resumen que ya vino armado desde el servidor.
    var mensajes = [].map.call(resumen.querySelectorAll('.charla-fila'), function (fila) {
        return {
            personaje: fila.getAttribute('data-personaje'),
            texto: fila.querySelector('.charla-globo').textContent
        };
    });
    if (mensajes.length === 0) return;

    // Se muestran de a dos (pregunta y respuesta). Si al final queda uno suelto,
    // se suma al último paso.
    var pasos = [];
    for (var i = 0; i < mensajes.length; i += 2) {
        pasos.push(mensajes.slice(i, i + 2));
    }
    if (pasos.length > 1 && pasos[pasos.length - 1].length === 1) {
        var suelto = pasos.pop();
        pasos[pasos.length - 1] = pasos[pasos.length - 1].concat(suelto);
    }

    var imagenes = {
        chico: charla.getAttribute('data-cuerpo-chico'),
        perro: charla.getAttribute('data-cuerpo-perro')
    };
    var actual = 0;

    function mostrarPaso() {
        paso.innerHTML = '';
        pasos[actual].forEach(function (mensaje) {
            var fila = document.createElement('div');
            fila.className = 'charla-fila charla-fila-' + mensaje.personaje;

            var imagen = document.createElement('img');
            imagen.className = 'charla-cuerpo';
            imagen.src = imagenes[mensaje.personaje];
            imagen.alt = mensaje.personaje === 'chico' ? 'Chico' : 'Riky';

            var globo = document.createElement('p');
            globo.className = 'charla-globo';
            globo.textContent = mensaje.texto;

            fila.appendChild(imagen);
            fila.appendChild(globo);
            paso.appendChild(fila);
        });
    }

    function mostrarResumen() {
        paso.hidden = true;
        siguiente.hidden = true;
        resumen.hidden = false;
        comenzar.hidden = false;
    }

    siguiente.addEventListener('click', function () {
        actual++;
        if (actual < pasos.length) {
            mostrarPaso();
        } else {
            mostrarResumen();
        }
    });

    // Arranque: se esconde el resumen y se muestra el primer paso.
    resumen.hidden = true;
    comenzar.hidden = true;
    paso.hidden = false;
    siguiente.hidden = false;
    mostrarPaso();
})();
