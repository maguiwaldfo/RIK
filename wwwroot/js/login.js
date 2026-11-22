// Validación del formulario de inicio de sesión (Login/Index).
// Cada campo tiene su propio mensaje: se muestra solo si hay un error
// y se oculta apenas el usuario lo corrige (el servidor igual valida los datos).

function validarUsuarioLogin(valor) {
    if (valor.trim() == "") {
        return "Ingresá tu nombre de usuario.";
    }
    return "";
}

function validarContrasenaLogin(valor) {
    if (valor == "") {
        return "Ingresá tu contraseña.";
    }
    return "";
}

// Campos del formulario: id del input, id de su mensaje y la función que lo valida
let camposLogin = [
    { idInput: "nombreUsuario", idFeedback: "fb-nombreUsuario", validar: validarUsuarioLogin },
    { idInput: "contrasena", idFeedback: "fb-contrasena", validar: validarContrasenaLogin }
];

// Valida un campo, muestra su mensaje y marca el input en rojo si hay error
function validarCampo(campo) {
    let input = document.getElementById(campo.idInput);
    let feedback = document.getElementById(campo.idFeedback);

    let mensaje = campo.validar(input.value);
    feedback.innerHTML = mensaje;

    if (mensaje == "") {
        input.className = "";
        return true;
    } else {
        input.className = "input-error";
        return false;
    }
}

// Se llama desde el onsubmit del formulario: si hay errores, no se envía
function validarLogin() {
    let primeroConError = null;

    for (let i = 0; i < camposLogin.length; i++) {
        let estaBien = validarCampo(camposLogin[i]);
        if (estaBien == false && primeroConError == null) {
            primeroConError = camposLogin[i];
        }
    }

    if (primeroConError != null) {
        document.getElementById(primeroConError.idInput).focus();
        return false;
    }

    return true;
}

// Al salir del campo se valida; mientras escribe, solo se re-valida si ya mostraba un error
function prepararCampo(campo) {
    let input = document.getElementById(campo.idInput);
    let feedback = document.getElementById(campo.idFeedback);

    input.onblur = function () {
        validarCampo(campo);
    };

    input.oninput = function () {
        if (feedback.innerHTML != "") {
            validarCampo(campo);
        }

        // El error del servidor ("usuario o contraseña incorrectos") deja de aplicar cuando cambia algo
        document.getElementById("fb-servidor").innerHTML = "";
    };
}

for (let i = 0; i < camposLogin.length; i++) {
    prepararCampo(camposLogin[i]);
}
