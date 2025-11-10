using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PepsicoChile.Migrations
{
    /// <inheritdoc />
    public partial class AgregarImagenesIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagenesIngreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngresoTallerId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioSubidaId = table.Column<int>(type: "int", nullable: true),
                    TamañoBytes = table.Column<long>(type: "bigint", nullable: false),
                    TipoImagen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesIngreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagenesIngreso_IngresosTaller_IngresoTallerId",
                        column: x => x.IngresoTallerId,
                        principalTable: "IngresosTaller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagenesIngreso_Usuarios_UsuarioSubidaId",
                        column: x => x.UsuarioSubidaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesIngreso_FechaSubida",
                table: "ImagenesIngreso",
                column: "FechaSubida");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesIngreso_IngresoTallerId",
                table: "ImagenesIngreso",
                column: "IngresoTallerId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesIngreso_UsuarioSubidaId",
                table: "ImagenesIngreso",
                column: "UsuarioSubidaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagenesIngreso");
        }
    }
}
