using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firmeza.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposReciboAVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Iva",
                table: "Ventas",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReciboArchivo",
                table: "Ventas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iva",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ReciboArchivo",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Ventas");
        }
    }
}
