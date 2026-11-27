using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGC.AccesoADatos.Migrations;

[DbContext(typeof(SgcDbContext))]
[Migration("20261126010000_AgregarSolicitudesCredito")]
public partial class AgregarSolicitudesCredito : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SolicitudesCredito",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ClienteId = table.Column<int>(type: "int", nullable: false),
                Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Comentarios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                UsuarioRegistroId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SolicitudesCredito", x => x.Id);
                table.ForeignKey("FK_SolicitudesCredito_Clientes_ClienteId", x => x.ClienteId, "Clientes", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_SolicitudesCredito_Usuarios_UsuarioRegistroId", x => x.UsuarioRegistroId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DocumentosSolicitudCredito",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SolicitudCreditoId = table.Column<int>(type: "int", nullable: false),
                NombreOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                NombreAlmacenado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RutaRelativa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentosSolicitudCredito", x => x.Id);
                table.ForeignKey("FK_DocumentosSolicitudCredito_SolicitudesCredito_SolicitudCreditoId", x => x.SolicitudCreditoId, "SolicitudesCredito", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_SolicitudesCredito_ClienteId_Estado", table: "SolicitudesCredito", columns: new[] { "ClienteId", "Estado" });
        migrationBuilder.CreateIndex(name: "IX_SolicitudesCredito_UsuarioRegistroId", table: "SolicitudesCredito", column: "UsuarioRegistroId");
        migrationBuilder.CreateIndex(name: "IX_DocumentosSolicitudCredito_SolicitudCreditoId", table: "DocumentosSolicitudCredito", column: "SolicitudCreditoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DocumentosSolicitudCredito");
        migrationBuilder.DropTable(name: "SolicitudesCredito");
    }
}
