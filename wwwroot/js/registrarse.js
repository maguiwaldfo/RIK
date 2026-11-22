// Validación del formulario de registro (Login/Registrarse) y de "Modificar mis datos" (Perfil).
// Cada campo tiene su propio mensaje: se muestra solo si hay un error
// y se oculta apenas el usuario lo corrige.
// Las reglas son las mismas que valida el servidor en Usuario.ValidarDatosRegistro
// (el servidor igual vuelve a validar todo).

let letras = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
let letrasConAcentos = letras + "áéíóúÁÉÍÓÚñÑ";
let numeros = "0123456789";

// true si el texto no está vacío y tiene solo letras (con acentos y ñ) y espacios
function soloLetrasYEspacios(texto) {
    if (texto.length == 0) {
        return false;
    }
    for (let i = 0; i < texto.length; i++) {
        let letra = texto[i];
        let esEspacio = letra.trim() == "";
        if (letrasConAcentos.indexOf(letra) == -1 && esEspacio == false) {
            return false;
        }
    }
    return true;
}

// true si todas las letras del texto están en "permitidos" (y el texto no está vacío)
function tieneSolo(texto, permitidos) {
    if (texto.length == 0) {
        return false;
    }
    for (let i = 0; i < texto.length; i++) {
        if (permitidos.indexOf(texto[i]) == -1) {
            return false;
        }
    }
    return true;
}

// Formato nombre@dominio.com: un solo @, algo antes, y después del último punto al menos 2 letras
function emailValido(email) {
    let posicionArroba = email.indexOf("@");
    if (posicionArroba <= 0 || email.indexOf("@", posicionArroba + 1) != -1) {
        return false;
    }

    let usuario = email.substring(0, posicionArroba);
    let dominio = email.substring(posicionArroba + 1);

    if (tieneSolo(usuario, letras + numeros + "._%+-") == false) {
        return false;
    }
    if (tieneSolo(dominio, letras + numeros + ".-") == false) {
        return false;
    }

    let posicionPunto = dominio.lastIndexOf(".");
    if (posicionPunto <= 0) {
        return false;
    }

    let terminacion = dominio.substring(posicionPunto + 1);
    if (terminacion.length < 2 || tieneSolo(terminacion, letras) == false) {
        return false;
    }

    return true;
}

// Cada función devuelve el mensaje de error, o "" si el valor está bien
function validarNombre(valor) {
    if (valor.trim() == "") {
        return "El nombre es obligatorio.";
    }
    if (soloLetrasYEspacios(valor) == false) {
        return "El nombre solo puede tener letras y espacios.";
    }
    return "";
}

function validarEmail(valor) {
    if (valor == "") {
        return "El email es obligatorio.";
    }
    if (valor.length > 50) {
        return "El email puede tener como máximo 50 caracteres.";
    }
    if (emailValido(valor) == false) {
        return "El email no es válido (ej: nombre@mail.com).";
    }
    return "";
}

function validarNombreUsuario(valor) {
    if (valor.trim() == "") {
        return "El nombre de usuario es obligatorio.";
    }
    if (soloLetrasYEspacios(valor) == false) {
        return "El nombre de usuario solo puede tener letras.";
    }
    if (valor.length < 6) {
        return "El nombre de usuario debe tener al menos 6 letras.";
    }
    return "";
}

function validarContrasena(valor) {
    // Si el input tiene data-opcional="true", vacío significa "no cambiar la contraseña"
    let input = document.getElementById("contrasena");
    if (valor == "" && input.getAttribute("data-opcional") == "true") {
        return "";
    }
    if (valor == "") {
        return "La contraseña es obligatoria.";
    }
    if (valor.length < 8) {
        return "La contraseña debe tener al menos 8 caracteres.";
    }
    return "";
}

// Campos del formulario: id del input, id de su mensaje y la función que lo valida.
// En "Modificar mis datos" no está la contraseña: los campos que no existen se saltean.
let camposRegistro = [
    { idInput: "nombre", idFeedback: "fb-nombre", validar: validarNombre },
    { idInput: "email", idFeedback: "fb-email", validar: validarEmail },
    { idInput: "nombreUsuario", idFeedback: "fb-nombreUsuario", validar: validarNombreUsuario },
    { idInput: "contrasena", idFeedback: "fb-contrasena", validar: validarContrasena }
];

function existeCampo(campo) {
    let input = document.getElementById(campo.idInput);
    let feedback = document.getElementById(campo.idFeedback);
    return input != null && feedback != null;
}

// Valida un campo, muestra su mensaje y marca el input en rojo si hay error
function validarCampo(campo) {
    let input = document.getElementById(campo.idInput);
    let feedback = document.getElementById(campo.idFeedback);

    if (campo.idInput == "email") {
        input.value = input.value.trim(); // se envía sin espacios, así el servidor no lo rechaza
    }

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
function registrarUsuario() {
    let primeroConError = null;

    for (let i = 0; i < camposRegistro.length; i++) {
        let campo = camposRegistro[i];
        if (existeCampo(campo)) {
            let estaBien = validarCampo(campo);
            if (estaBien == false && primeroConError == null) {
                primeroConError = campo;
            }
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

        // El error del servidor (ej: usuario repetido) deja de aplicar cuando se cambia el nombre de usuario
        let fbServidor = document.getElementById("fb-servidor");
        if (campo.idInput == "nombreUsuario" && fbServidor != null) {
            fbServidor.innerHTML = "";
        }
    };
}

for (let i = 0; i < camposRegistro.length; i++) {
    if (existeCampo(camposRegistro[i])) {
        prepararCampo(camposRegistro[i]);
    }
}
