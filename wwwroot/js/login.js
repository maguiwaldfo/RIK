// Cada campo tiene su propio mensaje: se muestra solo si hay un error
// y se oculta apenas el usuario lo corrige.
// Si falta algún elemento en la página no se rompe el script: se avisa y se sigue con lo que hay
// (el servidor igual valida los datos).
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
  const contenedor = document.querySelector(".login-form") || document.body;
  contenedor.prepend(aviso);
}

const inputUsuario = obtenerElemento("nombreUsuario");
const inputContrasena = obtenerElemento("contrasena");

const camposLogin = [
  {
    input: inputUsuario,
    feedback: obtenerFeedback("fb-nombreUsuario", inputUsuario),
    validar: valor => valor.trim() === "" ? "Ingresá tu nombre de usuario." : ""
  },
  {
    input: inputContrasena,
    feedback: obtenerFeedback("fb-contrasena", inputContrasena),
    validar: valor => valor === "" ? "Ingresá tu contraseña." : ""
  }
].filter(campo => campo.input !== null); // sin input no hay nada que validar

const fbServidor = document.getElementById("fb-servidor"); // opcional: solo existe si el servidor mandó un error

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

function validarLogin() {
  let primeroConError = null;

  camposLogin.forEach(campo => {
    if (!validarCampo(campo) && primeroConError === null) {
      primeroConError = campo;
    }
  });

  if (primeroConError !== null) {
    primeroConError.input.focus();
    return false;
  }

  return true;
}

camposLogin.forEach(campo => {
  // Al salir del campo se valida; mientras escribe, solo se re-valida si ya mostraba un error
  campo.input.addEventListener("blur", () => validarCampo(campo));
  campo.input.addEventListener("input", () => {
    if (campo.feedback.textContent !== "") {
      validarCampo(campo);
    }
    // El error del servidor ("usuario o contraseña incorrectos") deja de aplicar cuando cambia algo
    if (fbServidor) {
      fbServidor.textContent = "";
    }
  });
});
