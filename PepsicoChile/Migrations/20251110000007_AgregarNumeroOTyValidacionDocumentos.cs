using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PepsicoChile.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeroOTyValidacionDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroOT",
                table: "IngresosTaller",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoValidacion",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValidacion",
                table: "Documentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesValidacion",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroOT",
                table: "IngresosTaller");

            migrationBuilder.DropColumn(
                name: "EstadoValidacion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FechaValidacion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ObservacionesValidacion",
                table: "Documentos");
        }
    }
}
