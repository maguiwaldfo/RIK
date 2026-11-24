// Validación del formulario de Perfil/CambiarContrasena.
// Las reglas son las mismas que valida el servidor en PerfilController.CambiarContrasena
// (el servidor igual vuelve a validar todo).

function validarContrasenaActual(valor) {
    if (valor == "") {
        return "Escribí tu contraseña actual.";
    }
    return "";
}

function validarNuevaContrasena(valor) {
    if (valor == "") {
        return "Escribí la nueva contraseña.";
    }
    if (valor.length < 8) {
        return "La nueva contraseña debe tener al menos 8 caracteres.";
    }
    if (valor.length > 50) {
        return "La nueva contraseña puede tener como máximo 50 caracteres.";
    }
    return "";
}

function validarRepetirContrasena(valor) {
    let nueva = document.getElementById("nuevaContrasena").value;

    if (valor == "") {
        return "Repetí la nueva contraseña.";
    }
    if (valor != nueva) {
        return "La nueva contraseña y la confirmación no coinciden.";
    }
    return "";
}

let camposContrasena = [
    { idInput: "contrasenaActual", idFeedback: "fb-contrasenaActual", validar: validarContrasenaActual },
    { idInput: "nuevaContrasena", idFeedback: "fb-nuevaContrasena", validar: validarNuevaContrasena },
    { idInput: "repetirContrasena", idFeedback: "fb-repetirContrasena", validar: validarRepetirContrasena }
];

// Valida un campo, muestra su mensaje y marca el input en rojo si hay error
function validarCampoContrasena(campo) {
    let input = document.getElementById(campo.idInput);
    let feedback = document.getElementById(campo.idFeedback);
    let mensaje = campo.validar(input.value);

    feedback.textContent = mensaje;

    if (mensaje == "") {
        input.classList.remove("input-error");
        return true;
    } else {
        input.classList.add("input-error");
        return false;
    }
}

// Se llama desde el onsubmit del formulario: si hay errores, no se envía
function validarCambioContrasena() {
    let primeroConError = null;

    for (let i = 0; i < camposContrasena.length; i++) {
        let estaBien = validarCampoContrasena(camposContrasena[i]);

        if (!estaBien && primeroConError == null) {
            primeroConError = camposContrasena[i];
        }
    }

    if (primeroConError != null) {
        document.getElementById(primeroConError.idInput).focus();
        return false;
    }

    return true;
}

// Al salir de cada campo se valida
for (let i = 0; i < camposContrasena.length; i++) {
    let campo = camposContrasena[i];

    document.getElementById(campo.idInput).onblur = function () {
        validarCampoContrasena(campo);
    };
}
