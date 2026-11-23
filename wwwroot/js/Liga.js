// Funciones de las vistas de Liga (Crear e Index).

function ActualizarContraseña() {
    let privacidad = document.getElementById("Privacidad");
    let divContraseña = document.getElementById("contrsena");
    let contraseña = document.getElementById("Contraseña");

    if (privacidad.value == "true") {
        divContraseña.style.display = "block";
        contraseña.required = true;
    }
    else {
        divContraseña.style.display = "none";
        contraseña.required = false;
        contraseña.value = "";
    }
}

// Al volver de un error, conserva la privacidad elegida y vuelve a pedir la contraseña.
if (document.getElementById("Privacidad") != null) {
    ActualizarContraseña();
}

function CopiarLinkInvitacion() {
    let input = document.getElementById("linkInvitacion");
    let estado = document.getElementById("linkCopiado");

    // navigator.clipboard solo existe en https o localhost, y algunos navegadores lo bloquean
    if (!navigator.clipboard) {
        CopiarSeleccionando(input, estado);
        return;
    }

    navigator.clipboard.writeText(input.value)
        .then(function () {
            estado.textContent = "¡Link copiado!";
        })
        .catch(function () {
            CopiarSeleccionando(input, estado);
        });
}

// Plan B: selecciona el texto y usa el copiado clásico del navegador
function CopiarSeleccionando(input, estado) {
    input.select();
    input.setSelectionRange(0, input.value.length);

    let copiado = false;

    try {
        copiado = document.execCommand("copy");
    }
    catch (error) {
        copiado = false;
    }

    if (copiado) {
        estado.textContent = "¡Link copiado!";
    }
    else {
        estado.textContent = "Copialo con Ctrl+C (ya está seleccionado).";
    }
}
