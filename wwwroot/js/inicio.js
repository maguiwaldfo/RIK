(function () {
            var mapa = document.getElementById('inicioMapa');
            var lienzo = document.getElementById('inicioLienzo');
            if (!mapa || !lienzo) return;

            var avance = 0;           // 0 = inicio del camino, 1 = final
            var arrastre = null;      // datos del arrastre en curso
            var huboArrastre = false; // para no abrir un nivel al soltar un arrastre

            // El camino sube en diagonal: al avanzar, la línea y las monedas
            // bajan hacia la izquierda. El resto de la pantalla no se toca.
            function recorrido() {
                return {
                    x: Math.max(1, lienzo.offsetWidth - mapa.clientWidth),
                    y: Math.max(1, lienzo.offsetHeight - mapa.clientHeight)
                };
            }

            // Punto de la pantalla donde se "apoya" el camino: el lugar del nivel 1
            // cuando recién se empieza (abajo a la izquierda).
            var ANCLA_X = 0.092;
            var ANCLA_Y = 0.813;
            var nodos = lienzo.querySelectorAll('.inicio-nodo');

            // Altura del camino (dentro de la capa) en una posición horizontal dada,
            // tomando como referencia el centro de las monedas.
            function alturaCamino(x) {
                if (!nodos.length) return 0;
                if (x <= nodos[0].offsetLeft) return nodos[0].offsetTop;
                for (var i = 1; i < nodos.length; i++) {
                    var a = nodos[i - 1], b = nodos[i];
                    if (x <= b.offsetLeft) {
                        var p = (x - a.offsetLeft) / (b.offsetLeft - a.offsetLeft);
                        return a.offsetTop + (b.offsetTop - a.offsetTop) * p;
                    }
                }
                return nodos[nodos.length - 1].offsetTop;
            }

            // Cuánto tiene que bajar la capa para que el camino pase por el ancla
            // cuando la capa está corrida x píxeles hacia la izquierda.
            function bajadaEnAncla(x) {
                var y = lienzo.offsetHeight - (1 - ANCLA_Y) * mapa.clientHeight
                        - alturaCamino(x + ANCLA_X * mapa.clientWidth);
                return Math.max(0, Math.min(recorrido().y, y));
            }

            // La capa se mueve en línea recta (en diagonal) desde el principio hasta el final
            // del camino: la bajada es proporcional al avance, sin seguir cada subida y bajada
            // entre monedas.
            function mover(valor) {
                var r = recorrido();
                avance = Math.max(0, Math.min(1, valor));
                var x = avance * r.x;
                var yInicio = bajadaEnAncla(0);
                var yFin = bajadaEnAncla(r.x);
                var y = yInicio + (yFin - yInicio) * avance;
                lienzo.style.transform = 'translate(' + (-x) + 'px, ' + y + 'px)';
            }

            // Rueda del mouse: hacia arriba avanza por el camino (que sube), hacia abajo vuelve.
            // Por eso deltaY va con el signo cambiado. El scroll horizontal queda igual.
            mapa.addEventListener('wheel', function (e) {
                e.preventDefault();
                var delta = Math.abs(e.deltaX) > Math.abs(e.deltaY) ? e.deltaX : -e.deltaY;
                if (e.deltaMode === 1) delta *= 40;
                mover(avance + delta / recorrido().x);
            }, { passive: false });

            // Arrastre con mouse o dedo.
            mapa.addEventListener('pointerdown', function (e) {
                if (e.button !== 0 || e.target.closest('.inicio-pantalla')) return;
                arrastre = { id: e.pointerId, x: e.clientX, y: e.clientY, inicio: avance, activo: false };
                huboArrastre = false;
            });

            mapa.addEventListener('pointermove', function (e) {
                if (!arrastre || e.pointerId !== arrastre.id) return;
                var dx = e.clientX - arrastre.x;
                var dy = e.clientY - arrastre.y;
                if (!arrastre.activo) {
                    if (Math.abs(dx) + Math.abs(dy) < 6) return;
                    arrastre.activo = true;
                    huboArrastre = true;
                    mapa.classList.add('inicio-arrastrando');
                    mapa.setPointerCapture(arrastre.id);
                }
                var r = recorrido();
                mover(arrastre.inicio + (dy - dx) / (r.x + r.y));
            });

            function soltar(e) {
                if (!arrastre || e.pointerId !== arrastre.id) return;
                arrastre = null;
                mapa.classList.remove('inicio-arrastrando');
            }
            mapa.addEventListener('pointerup', soltar);
            mapa.addEventListener('pointercancel', soltar);

            mapa.addEventListener('click', function (e) {
                if (huboArrastre) {
                    e.preventDefault();
                    e.stopPropagation();
                    huboArrastre = false;
                }
            }, true);

            mapa.addEventListener('dragstart', function (e) { e.preventDefault(); });
            window.addEventListener('resize', function () { mover(avance); });

            // Al entrar, el mapa arranca en el nivel que toca jugar: esa moneda queda
            // abajo a la izquierda, donde está el nivel 1 cuando recién se empieza.
            var actual = lienzo.querySelector('.inicio-nodo-actual');
            if (actual) {
                lienzo.style.transition = 'none';
                mover((actual.offsetLeft - mapa.clientWidth * ANCLA_X) / recorrido().x);
                lienzo.offsetWidth;
                lienzo.style.transition = '';
            }
        })();