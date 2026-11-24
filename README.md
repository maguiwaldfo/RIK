# RIK

## Integrantes

- Matías Acuña
- Julián Falk
- Emanuel Miklaski
- Magalí Waldfogiel

## Descripción

RIK es un juego de educación financiera para adolescentes. A través de niveles con preguntas, el jugador aprende conceptos sobre el dinero de una forma dinámica, acompañado por Ricky, la mascota que funciona como mentor. Cada nivel desbloquea el siguiente, se suman puntos y se puede competir con otros jugadores y con amigos.

## Funcionalidades implementadas

- **Registro e inicio de sesión**, con validación de datos del lado del cliente y del servidor.
- **Mapa de niveles**: cada nivel se desbloquea al completar el anterior.
- **Teoría por nivel**: antes de jugar, una charla entre Ricky y un chico explica el tema.
- **Partida**: 8 preguntas al azar por nivel, 3 vidas, barra de progreso y tiempo. Al terminar se muestra un resumen con XP, tiempo y porcentaje de aciertos.
- **Racha** de días consecutivos completando niveles.
- **Biblioteca** de teorías y moralejas de cada tema.
- **Ligas**:
  - Ranking global y ranking por liga.
  - Crear ligas públicas o privadas (con contraseña) y elegir su capacidad.
  - Buscar ligas por nombre o unirse con un link de invitación.
  - En las ligas privadas, el creador acepta o rechaza las solicitudes.
  - Abandonar una liga o, si sos el creador, eliminarla.
- **Perfil**: modificar los datos personales, cambiar la contraseña y cerrar sesión.

## Tecnologías

- ASP.NET Core MVC (.NET 9)
- SQL Server con Stored Procedures
- Dapper
- HTML, CSS, JavaScript y Bootstrap

## Cómo ejecutar el proyecto

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (Express o superior) y SQL Server Management Studio

### Pasos

1. Clonar el repositorio.
2. Crear la base de datos: abrir `Rik script.sql` en SQL Server Management Studio y ejecutarlo. Crea la base `RIK` con sus tablas, datos y stored procedures.
   > El script trae la ruta de los archivos `.mdf` y `.ldf` de la instancia `MSSQL14.SQLEXPRESS`. Si tu instalación es otra, cambiá esas rutas al principio del script o borrá las líneas `ON PRIMARY` / `LOG ON` para usar las de por defecto.
3. Revisar la cadena de conexión en `Models/BD.cs`. Por defecto usa `Server=localhost` con autenticación de Windows. Si tu servidor tiene otro nombre (por ejemplo `localhost\SQLEXPRESS`), cambialo ahí.
4. Desde la carpeta del proyecto, ejecutar:

   ```bash
   dotnet run
   ```

5. Abrir <http://localhost:5090> en el navegador.

## Enlaces

- **Trello**: <https://trello.com/b/B1yPcq1w>
- **Figma**: <https://www.figma.com/design/DjExXM4JsmAbsXyyfZyNIY/Aplicacion?node-id=0-1&p=f&t=YUaBXECKayajGTwY-0>
