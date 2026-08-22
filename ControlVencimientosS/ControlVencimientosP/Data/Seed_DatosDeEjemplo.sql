/* ============================================================
   Control de Vencimientos — datos de ejemplo para probar la app.

   Carga 17 ítems con su vencimiento (repartidos entre vencido, por vencer
   y vigente, según el aviso por defecto de cada categoría) y 3 destinatarios
   de ejemplo. Las categorías NO se tocan: ya vienen cargadas por la
   migración inicial (Habilitación, Seguro, Matafuego, ART, Capacitación,
   Carnet, Licencia, Mantenimiento, Inspección, Contrato, Otro).

   Las fechas de vencimiento se calculan con DATEADD contra GETDATE(), no
   son fechas fijas: no importa qué día se corra el script, siempre va a
   haber una mezcla realista de vencidos / por vencer / vigentes.

   CÓMO CORRERLO: pegar y ejecutar contra la base de LocalDB del proyecto
   (SSMS, Azure Data Studio, o el editor de consultas SQL de Visual Studio
   — SQL Server Object Explorer → botón derecho sobre la base → New Query).
   Requiere que la migración inicial ya esté aplicada (Update-Database).

   Es IDEMPOTENTE por las dudas: si la tabla Items ya tiene algo cargado,
   no hace nada (para no duplicar datos si se corre dos veces por error).
   Si querés recargar de cero, borrá primero los ítems de prueba y volvé
   a correrlo.
   ============================================================ */

SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.Items)
BEGIN
    PRINT 'Ya hay ítems cargados: no se insertó nada. Si querés recargar los datos de ejemplo, borrá los ítems existentes primero.';
    RETURN;
END

DECLARE @EmpresaId INT = 1;
DECLARE @ItemId INT;

BEGIN TRANSACTION;

-- ---------- Habilitación (categoría 1, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 1, N'Habilitación Municipal', NULL, N'Local principal', NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 200, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 1, N'Habilitación Bromatológica', NULL, N'Local principal', NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, -10, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Seguro (categoría 2, aviso 60 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 2, N'Seguro de Incendio', NULL, N'Depósito', N'La Caja Seguros', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 15, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 2, N'Seguro de Responsabilidad Civil', NULL, N'Local principal', N'La Caja Seguros', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 400, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Matafuego (categoría 3, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 3, N'Matafuego', N'182', N'Depósito', N'Seguridad e Higiene SRL', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 20, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 3, N'Matafuego', N'183', N'Oficina', N'Seguridad e Higiene SRL', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 5, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 3, N'Matafuego', N'184', N'Taller', N'Seguridad e Higiene SRL', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- ART (categoría 4, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 4, N'Póliza ART Empleados', NULL, NULL, N'Prevención ART', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 25, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Capacitación (categoría 5, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 5, N'Capacitación Manejo de Extintores', NULL, NULL, NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 10, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 5, N'Capacitación Trabajo en Altura', NULL, NULL, NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 90, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Carnet (categoría 6, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 6, N'Carnet de Conductor — Juan Pérez', NULL, NULL, NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 12, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Licencia (categoría 7, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 7, N'Licencia de Conducir Comercial', NULL, NULL, NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 150, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Mantenimiento (categoría 8, aviso 15 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 8, N'Mantenimiento de Ascensor', NULL, N'Edificio', N'Ascensores del Sur', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 8, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 8, N'Mantenimiento de Aire Acondicionado', NULL, N'Oficina', NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, -1, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Inspección (categoría 9, aviso 30 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 9, N'Inspección de Calderas', NULL, N'Depósito', NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 40, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Contrato (categoría 10, aviso 60 días) ----------

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 10, N'Contrato de Alquiler del Depósito', NULL, NULL, NULL, 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 300, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

INSERT INTO dbo.Items (EmpresaId, CategoriaId, Nombre, Codigo, Ubicacion, Proveedor, Activo, CreadoEn)
VALUES (@EmpresaId, 10, N'Contrato con Proveedor de Insumos', NULL, NULL, N'Insumos del Litoral', 1, SYSUTCDATETIME());
SET @ItemId = SCOPE_IDENTITY();
INSERT INTO dbo.Vencimientos (EmpresaId, ItemId, FechaVencimiento, Estado, CreadoEn)
VALUES (@EmpresaId, @ItemId, DATEADD(DAY, 50, CAST(GETDATE() AS DATE)), 0, SYSUTCDATETIME());

-- ---------- Destinatarios de ejemplo ----------
-- CategoriasFiltro: ids separados por coma, NULL = recibe todas.

INSERT INTO dbo.Destinatarios (EmpresaId, Nombre, Email, Telefono, CategoriasFiltro, Activo)
VALUES (@EmpresaId, N'Rodrigo Bombieri', N'rodrigo@tuempresa.com.ar', NULL, NULL, 1);

INSERT INTO dbo.Destinatarios (EmpresaId, Nombre, Email, Telefono, CategoriasFiltro, Activo)
VALUES (@EmpresaId, N'Seguridad e Higiene', N'seguridad@tuempresa.com.ar', NULL, N'3,4,5', 1);

INSERT INTO dbo.Destinatarios (EmpresaId, Nombre, Email, Telefono, CategoriasFiltro, Activo)
VALUES (@EmpresaId, N'Estudio Contable', N'estudio@tuempresa.com.ar', NULL, N'1,2,10', 1);

COMMIT TRANSACTION;

PRINT '17 ítems con su vencimiento y 3 destinatarios cargados correctamente.';
