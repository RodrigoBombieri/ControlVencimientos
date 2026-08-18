using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ControlVencimientosP.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Cuit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ZonaHoraria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    RecibeAvisos = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    DiasAvisoDefault = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesAvisos",
                columns: table => new
                {
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    HitosDias = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HoraEnvio = table.Column<TimeOnly>(type: "time", nullable: false),
                    ReenviarVencidosCadaDias = table.Column<int>(type: "int", nullable: false),
                    DigestActivo = table.Column<bool>(type: "bit", nullable: false),
                    AlertaIndividualActiva = table.Column<bool>(type: "bit", nullable: false),
                    WhatsappActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesAvisos", x => x.EmpresaId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesAvisos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Destinatarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CategoriasFiltro = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinatarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destinatarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ResponsableUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Proveedor = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_AspNetUsers_ResponsableUsuarioId",
                        column: x => x.ResponsableUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vencimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DiasAviso = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    RenovadoPorVencimientoId = table.Column<int>(type: "int", nullable: true),
                    CreadoPorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vencimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vencimientos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vencimientos_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vencimientos_Vencimientos_RenovadoPorVencimientoId",
                        column: x => x.RenovadoPorVencimientoId,
                        principalTable: "Vencimientos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Adjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    VencimientoId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaBlob = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    SubidoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adjuntos_Vencimientos_VencimientoId",
                        column: x => x.VencimientoId,
                        principalTable: "Vencimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvisosEnviados",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    VencimientoId = table.Column<int>(type: "int", nullable: false),
                    HitoDias = table.Column<int>(type: "int", nullable: false),
                    Canal = table.Column<int>(type: "int", nullable: false),
                    DestinatarioId = table.Column<int>(type: "int", nullable: false),
                    EnviadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ProveedorMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvisosEnviados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvisosEnviados_Destinatarios_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Destinatarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvisosEnviados_Vencimientos_VencimientoId",
                        column: x => x.VencimientoId,
                        principalTable: "Vencimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Activa", "CreadaEn", "Cuit", "LogoUrl", "Nombre", "ZonaHoraria" },
                values: new object[] { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Mi empresa", "America/Argentina/Buenos_Aires" });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activa", "Color", "DiasAvisoDefault", "EmpresaId", "Icono", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, true, "#2a78d6", 30, 1, "building-2", "Habilitacion", 1 },
                    { 2, true, "#2a78d6", 60, 1, "shield", "Seguro", 2 },
                    { 3, true, "#2a78d6", 30, 1, "flame-kindling", "Matafuego", 3 },
                    { 4, true, "#2a78d6", 30, 1, "shield-plus", "ART", 4 },
                    { 5, true, "#2a78d6", 30, 1, "graduation-cap", "Capacitacion", 5 },
                    { 6, true, "#2a78d6", 30, 1, "id-card", "Carnet", 6 },
                    { 7, true, "#2a78d6", 30, 1, "scroll-text", "Licencia", 7 },
                    { 8, true, "#2a78d6", 15, 1, "wrench", "Mantenimiento", 8 },
                    { 9, true, "#2a78d6", 30, 1, "search-check", "Inspeccion", 9 },
                    { 10, true, "#2a78d6", 60, 1, "file-signature", "Contrato", 10 },
                    { 11, true, "#2a78d6", 30, 1, "file-text", "Otro", 11 }
                });

            migrationBuilder.InsertData(
                table: "ConfiguracionesAvisos",
                columns: new[] { "EmpresaId", "AlertaIndividualActiva", "DigestActivo", "HitosDias", "HoraEnvio", "ReenviarVencidosCadaDias", "WhatsappActivo" },
                values: new object[] { 1, true, true, "60,30,15,7,1,0", new TimeOnly(8, 0, 0), 7, false });

            migrationBuilder.CreateIndex(
                name: "IX_Adjuntos_VencimientoId",
                table: "Adjuntos",
                column: "VencimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AvisosEnviados_DestinatarioId",
                table: "AvisosEnviados",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "UX_AvisosEnviados_Idempotencia",
                table: "AvisosEnviados",
                columns: new[] { "VencimientoId", "HitoDias", "Canal", "DestinatarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId_Nombre",
                table: "Categorias",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Destinatarios_EmpresaId_Activo",
                table: "Destinatarios",
                columns: new[] { "EmpresaId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoriaId",
                table: "Items",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_EmpresaId_CategoriaId",
                table: "Items",
                columns: new[] { "EmpresaId", "CategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_EmpresaId_Nombre",
                table: "Items",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ResponsableUsuarioId",
                table: "Items",
                column: "ResponsableUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Vencimientos_EmpresaId_Estado_FechaVencimiento",
                table: "Vencimientos",
                columns: new[] { "EmpresaId", "Estado", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Vencimientos_RenovadoPorVencimientoId",
                table: "Vencimientos",
                column: "RenovadoPorVencimientoId");

            migrationBuilder.CreateIndex(
                name: "UX_Vencimientos_UnoActivoPorItem",
                table: "Vencimientos",
                column: "ItemId",
                unique: true,
                filter: "[Estado] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adjuntos");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AvisosEnviados");

            migrationBuilder.DropTable(
                name: "ConfiguracionesAvisos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Destinatarios");

            migrationBuilder.DropTable(
                name: "Vencimientos");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
