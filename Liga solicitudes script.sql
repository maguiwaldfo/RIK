/* =====================================================================
   LIGAS: unirse por búsqueda, solicitudes para ligas privadas
   y link de invitación.

   Correr una vez sobre la base RIK (después de "Rik script.sql").
   Se puede volver a correr sin romper nada.
   ===================================================================== */

USE [RIK]
GO

/* ---------------------------------------------------------------------
   1. Código de invitación de cada liga (va en el link que se comparte).
      Es aleatorio para que no se pueda adivinar el link de otra liga.
   --------------------------------------------------------------------- */

IF COL_LENGTH('dbo.Ligas', 'codigoInvitacion') IS NULL
BEGIN
    ALTER TABLE dbo.Ligas ADD codigoInvitacion VARCHAR(12) NULL;
END
GO

UPDATE dbo.Ligas
SET codigoInvitacion = UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 10))
WHERE codigoInvitacion IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Ligas_codigoInvitacion')
BEGIN
    ALTER TABLE dbo.Ligas ALTER COLUMN codigoInvitacion VARCHAR(12) NOT NULL;
    CREATE UNIQUE INDEX UQ_Ligas_codigoInvitacion ON dbo.Ligas (codigoInvitacion);
END
GO

/* ---------------------------------------------------------------------
   2. Solicitudes pendientes para entrar a una liga privada.
      Cuando el dueño acepta o veta, la solicitud se borra.
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.SolicitudesLiga', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudesLiga (
        id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idLiga         INT NOT NULL,
        idUsuario      INT NOT NULL,
        fechaSolicitud DATETIME NOT NULL CONSTRAINT DF_SolicitudesLiga_fecha DEFAULT GETDATE(),
        CONSTRAINT FK_SolicitudesLiga_Ligas    FOREIGN KEY (idLiga)    REFERENCES dbo.Ligas (id),
        CONSTRAINT FK_SolicitudesLiga_Usuarios FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios (id),
        CONSTRAINT UQ_SolicitudesLiga UNIQUE (idLiga, idUsuario)
    );
END
GO

/* ---------------------------------------------------------------------
   3. Procedimientos existentes que cambian
   --------------------------------------------------------------------- */

-- Ahora genera el código de invitación al crear la liga
CREATE OR ALTER PROCEDURE [dbo].[sp_CrearLiga]
    @nombre VARCHAR(50),
    @capacidad INT,
    @privacidad BIT,
    @contraseña VARCHAR(50),
    @idUsuarioCreador INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigo VARCHAR(12) = UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 10));

    WHILE EXISTS (SELECT 1 FROM dbo.Ligas WHERE codigoInvitacion = @codigo)
    BEGIN
        SET @codigo = UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 10));
    END

    INSERT INTO dbo.Ligas
        (nombre, capacidad, privacidad, contraseña, idUsuarioCreador, codigoInvitacion)
    VALUES
        (@nombre, @capacidad, @privacidad, @contraseña, @idUsuarioCreador, @codigo);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

-- Ahora también devuelve el código de invitación
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerLigaPorId]
    @idLiga INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id, nombre, capacidad, privacidad,
           contraseña, idUsuarioCreador, codigoInvitacion
    FROM dbo.Ligas
    WHERE id = @idLiga;
END
GO

-- Ahora también devuelve el código de invitación
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerLigasDeUsuario]
    @idUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT L.id, L.nombre, L.capacidad, L.privacidad, L.contraseña,
           L.idUsuarioCreador, L.codigoInvitacion
    FROM dbo.Ligas L
    INNER JOIN dbo.LigaXUsuario LXU ON L.id = LXU.idLiga
    WHERE LXU.idUsuario = @idUsuario;
END
GO

-- Ahora borra también las solicitudes pendientes de la liga
CREATE OR ALTER PROCEDURE [dbo].[sp_EliminarLiga]
    @idLiga INT,
    @idUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM dbo.Ligas
        WHERE id = @idLiga
        AND idUsuarioCreador = @idUsuario
    )
    BEGIN
        DELETE FROM dbo.SolicitudesLiga
        WHERE idLiga = @idLiga;

        DELETE FROM dbo.LigaXUsuario
        WHERE idLiga = @idLiga;

        DELETE FROM dbo.Ligas
        WHERE id = @idLiga;

        SELECT 1;
    END
    ELSE
    BEGIN
        SELECT 0;
    END
END
GO

/* ---------------------------------------------------------------------
   4. Procedimientos nuevos
   --------------------------------------------------------------------- */

-- Busca ligas por nombre. Nunca devuelve la contraseña.
CREATE OR ALTER PROCEDURE [dbo].[sp_BuscarLigas]
    @nombre VARCHAR(50),
    @idUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Los caracteres % _ [ que escriba el usuario se buscan como texto, no como comodines
    DECLARE @patron VARCHAR(160) =
        '%' + REPLACE(REPLACE(REPLACE(@nombre, '[', '[[]'), '%', '[%]'), '_', '[_]') + '%';

    SELECT TOP 30
        L.id AS Id,
        L.nombre AS Nombre,
        L.capacidad AS Capacidad,
        L.privacidad AS Privacidad,
        U.nombreUsuario AS NombreCreador,
        (SELECT COUNT(*) FROM dbo.LigaXUsuario LXU WHERE LXU.idLiga = L.id) AS CantidadMiembros,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.LigaXUsuario LXU
                               WHERE LXU.idLiga = L.id AND LXU.idUsuario = @idUsuario)
                  THEN 1 ELSE 0 END AS BIT) AS EsMiembro,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.SolicitudesLiga S
                               WHERE S.idLiga = L.id AND S.idUsuario = @idUsuario)
                  THEN 1 ELSE 0 END AS BIT) AS SolicitudPendiente
    FROM dbo.Ligas L
    INNER JOIN dbo.Usuarios U ON U.id = L.idUsuarioCreador
    WHERE L.nombre LIKE @patron
    ORDER BY L.nombre;
END
GO

-- Busca la liga de un link de invitación. Nunca devuelve la contraseña.
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerLigaPorCodigo]
    @codigo VARCHAR(12),
    @idUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        L.id AS Id,
        L.nombre AS Nombre,
        L.capacidad AS Capacidad,
        L.privacidad AS Privacidad,
        U.nombreUsuario AS NombreCreador,
        (SELECT COUNT(*) FROM dbo.LigaXUsuario LXU WHERE LXU.idLiga = L.id) AS CantidadMiembros,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.LigaXUsuario LXU
                               WHERE LXU.idLiga = L.id AND LXU.idUsuario = @idUsuario)
                  THEN 1 ELSE 0 END AS BIT) AS EsMiembro,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.SolicitudesLiga S
                               WHERE S.idLiga = L.id AND S.idUsuario = @idUsuario)
                  THEN 1 ELSE 0 END AS BIT) AS SolicitudPendiente
    FROM dbo.Ligas L
    INNER JOIN dbo.Usuarios U ON U.id = L.idUsuarioCreador
    WHERE L.codigoInvitacion = @codigo;
END
GO

/* Pide entrar a una liga.
   - Pública: entra directo.
   - Privada: hace falta la contraseña o el código del link, y queda
     una solicitud pendiente hasta que el dueño la acepte o la vete.

   Devuelve:
     1  = entró a la liga (pública)
     2  = solicitud enviada (privada)
     0  = la liga no existe
    -1  = ya es miembro
    -2  = la liga está llena
    -3  = contraseña incorrecta
    -4  = ya tenía una solicitud pendiente */
CREATE OR ALTER PROCEDURE [dbo].[sp_SolicitarUnirseALiga]
    @idLiga INT,
    @idUsuario INT,
    @contraseña VARCHAR(50) = NULL,
    @codigo VARCHAR(12) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @privacidad BIT,
            @capacidad INT,
            @contraseñaLiga VARCHAR(50),
            @codigoLiga VARCHAR(12);

    SELECT @privacidad = privacidad,
           @capacidad = capacidad,
           @contraseñaLiga = contraseña,
           @codigoLiga = codigoInvitacion
    FROM dbo.Ligas
    WHERE id = @idLiga;

    IF @capacidad IS NULL
    BEGIN
        SELECT 0;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.LigaXUsuario WHERE idLiga = @idLiga AND idUsuario = @idUsuario)
    BEGIN
        SELECT -1;
        RETURN;
    END

    IF (SELECT COUNT(*) FROM dbo.LigaXUsuario WHERE idLiga = @idLiga) >= @capacidad
    BEGIN
        SELECT -2;
        RETURN;
    END

    IF @privacidad = 0
    BEGIN
        INSERT INTO dbo.LigaXUsuario (idLiga, idUsuario)
        VALUES (@idLiga, @idUsuario);

        SELECT 1;
        RETURN;
    END

    -- Liga privada: la contraseña distingue mayúsculas y minúsculas
    DECLARE @accesoValido BIT = 0;

    IF @codigo IS NOT NULL AND @codigo = @codigoLiga
        SET @accesoValido = 1;

    IF @contraseña IS NOT NULL AND @contraseña <> ''
       AND @contraseña COLLATE Latin1_General_CS_AS = @contraseñaLiga COLLATE Latin1_General_CS_AS
        SET @accesoValido = 1;

    IF @accesoValido = 0
    BEGIN
        SELECT -3;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.SolicitudesLiga WHERE idLiga = @idLiga AND idUsuario = @idUsuario)
    BEGIN
        SELECT -4;
        RETURN;
    END

    INSERT INTO dbo.SolicitudesLiga (idLiga, idUsuario)
    VALUES (@idLiga, @idUsuario);

    SELECT 2;
END
GO

-- Solicitudes pendientes de una liga. Solo devuelve algo si quien pide es el dueño.
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerSolicitudesLiga]
    @idLiga INT,
    @idUsuarioCreador INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        S.id AS Id,
        S.idUsuario AS IdUsuario,
        U.nombreUsuario AS NombreUsuario,
        U.nombre AS Nombre,
        S.fechaSolicitud AS FechaSolicitud
    FROM dbo.SolicitudesLiga S
    INNER JOIN dbo.Ligas L ON L.id = S.idLiga
    INNER JOIN dbo.Usuarios U ON U.id = S.idUsuario
    WHERE S.idLiga = @idLiga
      AND L.idUsuarioCreador = @idUsuarioCreador
    ORDER BY S.fechaSolicitud;
END
GO

/* El dueño acepta o veta una solicitud.
   Devuelve:
     1  = aceptada (el usuario entró a la liga)
     2  = vetada
     0  = la solicitud no existe o no es de una liga de este usuario
    -2  = la liga está llena */
CREATE OR ALTER PROCEDURE [dbo].[sp_ResponderSolicitudLiga]
    @idSolicitud INT,
    @idUsuarioCreador INT,
    @aceptar BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idLiga INT, @idUsuario INT, @capacidad INT;

    SELECT @idLiga = S.idLiga,
           @idUsuario = S.idUsuario,
           @capacidad = L.capacidad
    FROM dbo.SolicitudesLiga S
    INNER JOIN dbo.Ligas L ON L.id = S.idLiga
    WHERE S.id = @idSolicitud
      AND L.idUsuarioCreador = @idUsuarioCreador;

    IF @idLiga IS NULL
    BEGIN
        SELECT 0;
        RETURN;
    END

    IF @aceptar = 0
    BEGIN
        DELETE FROM dbo.SolicitudesLiga WHERE id = @idSolicitud;
        SELECT 2;
        RETURN;
    END

    IF (SELECT COUNT(*) FROM dbo.LigaXUsuario WHERE idLiga = @idLiga) >= @capacidad
    BEGIN
        SELECT -2;
        RETURN;
    END

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.LigaXUsuario WHERE idLiga = @idLiga AND idUsuario = @idUsuario)
    BEGIN
        INSERT INTO dbo.LigaXUsuario (idLiga, idUsuario)
        VALUES (@idLiga, @idUsuario);
    END

    DELETE FROM dbo.SolicitudesLiga WHERE id = @idSolicitud;

    COMMIT TRANSACTION;

    SELECT 1;
END
GO
