const Nombre = document.getElementById("nombre");
const NombreUsuario = document.getElementById("nombreUsuario");
const Contrasena = document.getElementById("contrasena");
const Apellido = document.getElementById("apellido");
const TipoUsuario = document.getElementById("tipoUsuario");
const divResultado = document.getElementById("resultado");

function registrarUsuario() {
  const nombre = Nombre.value;
  const nombreUsuario = NombreUsuario.value;
  const contrasena = Contrasena.value;
  const apellido = Apellido.value;
  const tipoUsuario = TipoUsuario.value;

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

  // Validar que el apellido no esté vacío
  if (apellido === "") {
    errores.push("El apellido es obligatorio.");
    mostrarError("fb-apellido", "Campo obligatorio");
  } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(apellido)) {
    errores.push("El apellido solo puede contener letras y espacios.");
    mostrarError("fb-apellido", "Solo se permiten letras y espacios");
  } else {
    mostrarOk("fb-apellido", "✓ OK");
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

  // Validar que el tipo de usuario esté seleccionado
  if (tipoUsuario === "") {
    errores.push("Debe seleccionar un tipo de usuario.");
    mostrarError("fb-tipoUsuario", "Seleccione un tipo");
  } else {
    mostrarOk("fb-tipoUsuario", "✓ OK");
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
  document.getElementById("fb-apellido").innerHTML = "";
  document.getElementById("fb-tipoUsuario").innerHTML = "";
}

function limpiarFormulario() {
  Nombre.value = "";
  NombreUsuario.value = "";
  Contrasena.value = "";
  Apellido.value = "";
  TipoUsuario.value = "";
  divResultado.innerHTML = "";
  limpiarFeedbacks();
}