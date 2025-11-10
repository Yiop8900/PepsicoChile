using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PepsicoChile.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAsignacionVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionesVehiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    ChoferId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDesasignacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AsignadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesVehiculo_Usuarios_AsignadoPorId",
                        column: x => x.AsignadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesVehiculo_Usuarios_ChoferId",
                        column: x => x.ChoferId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesVehiculo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_AsignadoPorId",
                table: "AsignacionesVehiculo",
                column: "AsignadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_ChoferId_Activa",
                table: "AsignacionesVehiculo",
                columns: new[] { "ChoferId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_VehiculoId_Activa",
                table: "AsignacionesVehiculo",
                columns: new[] { "VehiculoId", "Activa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesVehiculo");
        }
    }
}
