const Nombre = document.getElementById("nombre");
const NombreUsuario = document.getElementById("nombreUsuario");
const Contrasena = document.getElementById("contrasena");
const Email = document.getElementById("email");
const divResultado = document.getElementById("resultado");

function registrarUsuario() {
  const nombre = Nombre.value;
  const nombreUsuario = NombreUsuario.value;
  const contrasena = Contrasena.value;
  const email = Email.value.trim();
  Email.value = email; // se envía sin espacios, así el servidor no lo rechaza

  const errores = [];
  limpiarFeedbacks();

  // Validar que el nombre no esté vacío
  if (nombre === "") {
    errores.push("El nombre es obligatorio.");
    mostrarError("fb-nombre", "Campo obligatorio");
    //esto es para que solo se puedan poner letras y espacios en el nombre
  } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(nombre)) {
    errores.push("El nombre solo puede contener letras y espacios.");
    mostrarError("fb-nombre", "Solo se permiten letras y espacios");
  } else {
    mostrarOk("fb-nombre", "✓ OK");
  }

  // Validar que el email no esté vacío y tenga formato válido (misma regex que el servidor)
  if (email === "") {
    errores.push("El email es obligatorio.");
    mostrarError("fb-email", "Campo obligatorio");
  } else if (email.length > 50) {
    errores.push("El email puede tener como máximo 50 caracteres.");
    mostrarError("fb-email", "Máximo 50 caracteres");
  } else if (!/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(email)) {
    errores.push("El email no tiene un formato válido (ej: nombre@mail.com).");
    mostrarError("fb-email", "Formato inválido");
  } else {
    mostrarOk("fb-email", "✓ OK");
  }

  // Validar que el nombre de usuario no esté vacío
  if (nombreUsuario === "") {
    errores.push("El nombre de usuario es obligatorio.");
    mostrarError("fb-nombreUsuario", "Campo obligatorio");
  } else if (nombreUsuario.length < 6) {
    errores.push("El nombre de usuario debe tener mínimo 6 caracteres.");
    mostrarError("fb-nombreUsuario", "Mínimo 6 caracteres");
  } else {
    mostrarOk("fb-nombreUsuario", "✓ OK");
  }

  // Validar que la contraseña no esté vacía
  if (contrasena === "") {
    errores.push("La contraseña es obligatoria.");
    mostrarError("fb-contrasena", "Campo obligatorio");
  } else if (contrasena.length <= 8) {
    errores.push("La contraseña debe tener más de 8 caracteres.");
    mostrarError("fb-contrasena", "Más de 8 caracteres");
  } else {
    mostrarOk("fb-contrasena", "✓ OK");
  }

  if (errores.length > 0) {
    divResultado.style.color = "red";
    divResultado.style.border = "1px solid red";
    divResultado.style.padding = "8px";
    divResultado.innerHTML = "<strong>No se pudo registrar:</strong><br>"
                           + errores.join("<br>");
    return false; // Evita que el formulario se envíe si hay errores
  } else {
    return true; // Permite que el formulario se envíe si no hay errores
  }
}

function mostrarError(id, msg) {
  const el = document.getElementById(id);
  el.innerHTML = msg;
  el.style.color = "red";
}

function mostrarOk(id, msg) {
  const el = document.getElementById(id);
  el.innerHTML = msg;
  el.style.color = "green";
}

function limpiarFeedbacks() {
  document.getElementById("fb-nombre").innerHTML = "";
  document.getElementById("fb-nombreUsuario").innerHTML = "";
  document.getElementById("fb-contrasena").innerHTML = "";
  document.getElementById("fb-email").innerHTML = "";
}

function limpiarFormulario() {
  Nombre.value = "";
  NombreUsuario.value = "";
  Contrasena.value = "";
  Email.value = "";
  divResultado.innerHTML = "";
  limpiarFeedbacks();
}