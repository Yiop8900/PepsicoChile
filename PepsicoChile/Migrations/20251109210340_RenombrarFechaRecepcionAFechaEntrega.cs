using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PepsicoChile.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarFechaRecepcionAFechaEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaRecepcion",
                table: "SolicitudesRepuesto",
                newName: "FechaEntrega");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "Repuestos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repuestos_Nombre_Proveedor",
                table: "Repuestos",
                columns: new[] { "Nombre", "Proveedor" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Repuestos_Nombre_Proveedor",
                table: "Repuestos");

            migrationBuilder.RenameColumn(
                name: "FechaEntrega",
                table: "SolicitudesRepuesto",
                newName: "FechaRecepcion");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "Repuestos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
