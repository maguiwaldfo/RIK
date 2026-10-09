function validarLogin() {
  const nombreUsuario = document.getElementById("nombreUsuario").value.trim();
  const contrasena = document.getElementById("contrasena").value.trim();
  const feedback = document.getElementById("fb-login");

  if (nombreUsuario === "" || contrasena === "") {
    feedback.textContent = "El nombre de usuario y la contraseña son obligatorios.";
    feedback.style.color = "red";
    return false;
  }

  else{
    feedback.textContent = "";
    return true;
  }
  
}