using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddClientDocumentAndPasswordChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clients_clinic_id",
                schema: "vetpass",
                table: "clients");

            migrationBuilder.AddColumn<bool>(
                name: "requires_password_change",
                schema: "vetpass",
                table: "user_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "document_number",
                schema: "vetpass",
                table: "clients",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "document_type",
                schema: "vetpass",
                table: "clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Un cliente sin documento no puede existir en el dominio. Si la tabla
            // ya tiene clientes, la migración se detiene con un mensaje claro en
            // lugar de fallar al crear el índice único sobre valores vacíos.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM vetpass.clients WHERE document_number = '') THEN
                        RAISE EXCEPTION 'Hay clientes sin documento de identidad. Regístralo o recrea el caso de demostración antes de aplicar esta migración.';
                    END IF;
                END $$;
                ALTER TABLE vetpass.clients ALTER COLUMN document_type DROP DEFAULT;
                ALTER TABLE vetpass.clients ALTER COLUMN document_number DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_clients_clinic_id_document_type_document_number",
                schema: "vetpass",
                table: "clients",
                columns: new[] { "clinic_id", "document_type", "document_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clients_clinic_id_document_type_document_number",
                schema: "vetpass",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "requires_password_change",
                schema: "vetpass",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "document_number",
                schema: "vetpass",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "document_type",
                schema: "vetpass",
                table: "clients");

            migrationBuilder.CreateIndex(
                name: "IX_clients_clinic_id",
                schema: "vetpass",
                table: "clients",
                column: "clinic_id");
        }
    }
}
