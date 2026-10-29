// Cada campo tiene su propio mensaje: se muestra solo si hay un error
// y se oculta apenas el usuario lo corrige.
// Las reglas son las mismas que valida el servidor en Usuario.ValidarDatosRegistro.
// Si falta algún elemento en la página no se rompe el script: se avisa y se sigue con lo que hay
// (el servidor igual valida los datos).
const soloLetras = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/;
const formatoEmail = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

const elementosFaltantes = [];

function obtenerElemento(id) {
  const el = document.getElementById(id);
  if (el === null) {
    elementosFaltantes.push(id);
  }
  return el;
}

// Si falta el <p> del mensaje pero el input existe, se crea uno debajo del input
function obtenerFeedback(id, input) {
  const fb = obtenerElemento(id);
  if (fb !== null || input === null) {
    return fb;
  }
  const nuevo = document.createElement("p");
  nuevo.id = id;
  nuevo.className = "feedback";
  input.insertAdjacentElement("afterend", nuevo);
  return nuevo;
}

function mostrarAvisoFaltantes() {
  if (elementosFaltantes.length === 0) {
    return;
  }
  const mensaje = "Aviso: no se encontraron estos elementos en la página: "
                + elementosFaltantes.map(id => "#" + id).join(", ")
                + ". El formulario igual funciona y el servidor valida los datos.";
  console.warn(mensaje);

  const aviso = document.createElement("p");
  aviso.className = "feedback aviso-js";
  aviso.textContent = mensaje;
  const contenedor = document.getElementById("registroForm") || document.body;
  contenedor.prepend(aviso);
}

const inputNombre = obtenerElemento("nombre");
const emailInput = obtenerElemento("email");
const inputUsuario = obtenerElemento("nombreUsuario");
const inputContrasena = obtenerElemento("contrasena");

const camposRegistro = [
  {
    input: inputNombre,
    feedback: obtenerFeedback("fb-nombre", inputNombre),
    validar: valor => {
      if (valor.trim() === "") return "El nombre es obligatorio.";
      if (!soloLetras.test(valor)) return "El nombre solo puede tener letras y espacios.";
      return "";
    }
  },
  {
    input: emailInput,
    feedback: obtenerFeedback("fb-email", emailInput),
    validar: valor => {
      if (valor === "") return "El email es obligatorio.";
      if (valor.length > 50) return "El email puede tener como máximo 50 caracteres.";
      if (!formatoEmail.test(valor)) return "El email no es válido (ej: nombre@mail.com).";
      return "";
    }
  },
  {
    input: inputUsuario,
    feedback: obtenerFeedback("fb-nombreUsuario", inputUsuario),
    validar: valor => {
      if (valor.trim() === "") return "El nombre de usuario es obligatorio.";
      if (!soloLetras.test(valor)) return "El nombre de usuario solo puede tener letras.";
      if (valor.length < 6) return "El nombre de usuario debe tener al menos 6 letras.";
      return "";
    }
  },
  {
    input: inputContrasena,
    feedback: obtenerFeedback("fb-contrasena", inputContrasena),
    validar: valor => {
      if (valor === "") return "La contraseña es obligatoria.";
      if (valor.length < 8) return "La contraseña debe tener al menos 8 caracteres.";
      return "";
    }
  }
].filter(campo => campo.input !== null); // sin input no hay nada que validar

const fbServidor = document.getElementById("fb-servidor"); // opcional: solo tiene texto si el servidor mandó un error

mostrarAvisoFaltantes();

function mostrarFeedback(campo, mensaje) {
  campo.feedback.textContent = mensaje;
  campo.input.classList.toggle("input-error", mensaje !== "");
}

function validarCampo(campo) {
  const mensaje = campo.validar(campo.input.value);
  mostrarFeedback(campo, mensaje);
  return mensaje === "";
}

function limpiarEmail() {
  if (emailInput !== null) {
    emailInput.value = emailInput.value.trim(); // se envía sin espacios, así el servidor no lo rechaza
  }
}

function registrarUsuario() {
  limpiarEmail();

  let primeroConError = null;

  camposRegistro.forEach(campo => {
    if (!validarCampo(campo) && primeroConError === null) {
      primeroConError = campo;
    }
  });

  if (primeroConError !== null) {
    primeroConError.input.focus();
    return false; // Evita que el formulario se envíe si hay errores
  }

  return true;
}

camposRegistro.forEach(campo => {
  // Al salir del campo se valida; mientras escribe, solo se re-valida si ya mostraba un error
  campo.input.addEventListener("blur", () => {
    if (campo.input === emailInput) {
      limpiarEmail();
    }
    validarCampo(campo);
  });
  campo.input.addEventListener("input", () => {
    if (campo.feedback.textContent !== "") {
      validarCampo(campo);
    }
  });
});

// El error del servidor (ej: usuario repetido) deja de aplicar cuando se cambia el nombre de usuario
if (inputUsuario !== null && fbServidor !== null) {
  inputUsuario.addEventListener("input", () => {
    fbServidor.textContent = "";
  });
}
