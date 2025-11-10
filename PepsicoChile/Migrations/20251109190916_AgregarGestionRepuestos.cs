using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PepsicoChile.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGestionRepuestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_AsignadoPorId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_ChoferId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Vehiculos_VehiculoId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_ImagenesIngreso_Usuarios_UsuarioSubidaId",
                table: "ImagenesIngreso");

            migrationBuilder.DropForeignKey(
                name: "FK_Repuestos_TareasTaller_TareaTallerId",
                table: "Repuestos");

            migrationBuilder.DropIndex(
                name: "IX_Repuestos_TareaTallerId",
                table: "Repuestos");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_UsuarioId_Leida",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_ImagenesIngreso_FechaSubida",
                table: "ImagenesIngreso");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesVehiculo_ChoferId_Activa",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesVehiculo_VehiculoId_Activa",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "FechaRecepcion",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "FechaSolicitud",
                table: "Repuestos");

            migrationBuilder.RenameColumn(
                name: "TareaTallerId",
                table: "Repuestos",
                newName: "StockMinimo");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "Repuestos",
                newName: "StockActual");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "Repuestos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Repuestos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CodigoRepuesto",
                table: "Repuestos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Repuestos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Repuestos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Repuestos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Repuestos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Repuestos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "Repuestos",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "Repuestos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MovimientosRepuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepuestoId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    StockAnterior = table.Column<int>(type: "int", nullable: false),
                    StockNuevo = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VehiculoId = table.Column<int>(type: "int", nullable: true),
                    TareaTallerId = table.Column<int>(type: "int", nullable: true),
                    MecanicoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioRegistroId = table.Column<int>(type: "int", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosRepuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosRepuesto_Repuestos_RepuestoId",
                        column: x => x.RepuestoId,
                        principalTable: "Repuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosRepuesto_TareasTaller_TareaTallerId",
                        column: x => x.TareaTallerId,
                        principalTable: "TareasTaller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosRepuesto_Usuarios_MecanicoId",
                        column: x => x.MecanicoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosRepuesto_Usuarios_UsuarioRegistroId",
                        column: x => x.UsuarioRegistroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosRepuesto_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesRepuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TareaTallerId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodigoRepuesto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SolicitadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRepuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRepuesto_TareasTaller_TareaTallerId",
                        column: x => x.TareaTallerId,
                        principalTable: "TareasTaller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesRepuesto_Usuarios_SolicitadoPorId",
                        column: x => x.SolicitadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repuestos_Categoria",
                table: "Repuestos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Repuestos_CodigoRepuesto",
                table: "Repuestos",
                column: "CodigoRepuesto");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_ChoferId",
                table: "AsignacionesVehiculo",
                column: "ChoferId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_VehiculoId",
                table: "AsignacionesVehiculo",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_FechaMovimiento",
                table: "MovimientosRepuesto",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_MecanicoId",
                table: "MovimientosRepuesto",
                column: "MecanicoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_RepuestoId",
                table: "MovimientosRepuesto",
                column: "RepuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_TareaTallerId",
                table: "MovimientosRepuesto",
                column: "TareaTallerId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_TipoMovimiento",
                table: "MovimientosRepuesto",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_UsuarioRegistroId",
                table: "MovimientosRepuesto",
                column: "UsuarioRegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosRepuesto_VehiculoId",
                table: "MovimientosRepuesto",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_Estado",
                table: "SolicitudesRepuesto",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_FechaSolicitud",
                table: "SolicitudesRepuesto",
                column: "FechaSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_SolicitadoPorId",
                table: "SolicitudesRepuesto",
                column: "SolicitadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRepuesto_TareaTallerId",
                table: "SolicitudesRepuesto",
                column: "TareaTallerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_AsignadoPorId",
                table: "AsignacionesVehiculo",
                column: "AsignadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_ChoferId",
                table: "AsignacionesVehiculo",
                column: "ChoferId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Vehiculos_VehiculoId",
                table: "AsignacionesVehiculo",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImagenesIngreso_Usuarios_UsuarioSubidaId",
                table: "ImagenesIngreso",
                column: "UsuarioSubidaId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_AsignadoPorId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_ChoferId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesVehiculo_Vehiculos_VehiculoId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_ImagenesIngreso_Usuarios_UsuarioSubidaId",
                table: "ImagenesIngreso");

            migrationBuilder.DropTable(
                name: "MovimientosRepuesto");

            migrationBuilder.DropTable(
                name: "SolicitudesRepuesto");

            migrationBuilder.DropIndex(
                name: "IX_Repuestos_Categoria",
                table: "Repuestos");

            migrationBuilder.DropIndex(
                name: "IX_Repuestos_CodigoRepuesto",
                table: "Repuestos");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesVehiculo_ChoferId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesVehiculo_VehiculoId",
                table: "AsignacionesVehiculo");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "Repuestos");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Repuestos");

            migrationBuilder.RenameColumn(
                name: "StockMinimo",
                table: "Repuestos",
                newName: "TareaTallerId");

            migrationBuilder.RenameColumn(
                name: "StockActual",
                table: "Repuestos",
                newName: "Cantidad");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "Repuestos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Repuestos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoRepuesto",
                table: "Repuestos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Repuestos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecepcion",
                table: "Repuestos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSolicitud",
                table: "Repuestos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repuestos_TareaTallerId",
                table: "Repuestos",
                column: "TareaTallerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_Leida",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesIngreso_FechaSubida",
                table: "ImagenesIngreso",
                column: "FechaSubida");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_ChoferId_Activa",
                table: "AsignacionesVehiculo",
                columns: new[] { "ChoferId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVehiculo_VehiculoId_Activa",
                table: "AsignacionesVehiculo",
                columns: new[] { "VehiculoId", "Activa" });

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_AsignadoPorId",
                table: "AsignacionesVehiculo",
                column: "AsignadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Usuarios_ChoferId",
                table: "AsignacionesVehiculo",
                column: "ChoferId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesVehiculo_Vehiculos_VehiculoId",
                table: "AsignacionesVehiculo",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImagenesIngreso_Usuarios_UsuarioSubidaId",
                table: "ImagenesIngreso",
                column: "UsuarioSubidaId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Repuestos_TareasTaller_TareaTallerId",
                table: "Repuestos",
                column: "TareaTallerId",
                principalTable: "TareasTaller",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
