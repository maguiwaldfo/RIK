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

// Valida el formulario de Crear Liga antes de enviarlo.
// Las reglas son las mismas que valida el servidor en Liga.ValidarDatos.
function ValidarLiga() {
    let nombre = document.getElementById("Nombre").value.trim();
    let capacidad = Number(document.getElementById("Capacidad").value);
    let esPrivada = document.getElementById("Privacidad").value == "true";
    let contraseña = document.getElementById("Contraseña").value;
    let error = document.getElementById("errorLiga");
    let mensaje = "";

    if (nombre == "") {
        mensaje = "Escribí el nombre de la liga.";
    }
    else if (nombre.length > 50) {
        mensaje = "El nombre puede tener como máximo 50 caracteres.";
    }
    else if (!Number.isInteger(capacidad) || capacidad < 2 || capacidad > 50) {
        mensaje = "La capacidad tiene que ser un número entre 2 y 50.";
    }
    else if (esPrivada && contraseña.trim() == "") {
        mensaje = "Las ligas privadas necesitan una contraseña.";
    }
    else if (contraseña.length > 50) {
        mensaje = "La contraseña puede tener como máximo 50 caracteres.";
    }

    error.textContent = mensaje;
    error.hidden = mensaje == "";

    return mensaje == "";
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
