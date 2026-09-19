using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vetpass");

            migrationBuilder.CreateTable(
                name: "clinics",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    address = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccines",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    species = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_core = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_clients_clinics_clinic_id",
                        column: x => x.clinic_id,
                        principalSchema: "vetpass",
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule_items",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaccine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    minimum_age_weeks = table.Column<int>(type: "integer", nullable: false),
                    minimum_interval_weeks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedule_items_vaccines_vaccine_id",
                        column: x => x.vaccine_id,
                        principalSchema: "vetpass",
                        principalTable: "vaccines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pets",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    species = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    breed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pets", x => x.id);
                    table.ForeignKey(
                        name: "FK_pets_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "vetpass",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    full_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    clinic_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_profiles_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "vetpass",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_profiles_clinics_clinic_id",
                        column: x => x.clinic_id,
                        principalSchema: "vetpass",
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccination_cards",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    pet_birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccination_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_vaccination_cards_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "vetpass",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinarian_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visit_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    findings = table.Column<string>(type: "text", nullable: true),
                    diagnosis = table.Column<string>(type: "text", nullable: false),
                    treatment = table.Column<string>(type: "text", nullable: true),
                    weight_kg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visits", x => x.id);
                    table.ForeignKey(
                        name: "FK_visits_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "vetpass",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visits_user_profiles_veterinarian_id",
                        column: x => x.veterinarian_id,
                        principalSchema: "vetpass",
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doses",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaccine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    expected_date = table.Column<DateOnly>(type: "date", nullable: false),
                    application_date = table.Column<DateOnly>(type: "date", nullable: true),
                    batch_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    veterinarian_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    minimum_age_weeks = table.Column<int>(type: "integer", nullable: false),
                    minimum_interval_weeks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doses", x => x.id);
                    table.ForeignKey(
                        name: "FK_doses_user_profiles_veterinarian_id",
                        column: x => x.veterinarian_id,
                        principalSchema: "vetpass",
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doses_vaccination_cards_card_id",
                        column: x => x.card_id,
                        principalSchema: "vetpass",
                        principalTable: "vaccination_cards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doses_vaccines_vaccine_id",
                        column: x => x.vaccine_id,
                        principalSchema: "vetpass",
                        principalTable: "vaccines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescriptions_visits_visit_id",
                        column: x => x.visit_id,
                        principalSchema: "vetpass",
                        principalTable: "visits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescription_items",
                schema: "vetpass",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prescription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    medication = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    dosage = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    duration = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescription_items_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalSchema: "vetpass",
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clients_clinic_id",
                schema: "vetpass",
                table: "clients",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_full_name",
                schema: "vetpass",
                table: "clients",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "IX_doses_card_id",
                schema: "vetpass",
                table: "doses",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_doses_status",
                schema: "vetpass",
                table: "doses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_doses_vaccine_id",
                schema: "vetpass",
                table: "doses",
                column: "vaccine_id");

            migrationBuilder.CreateIndex(
                name: "IX_doses_veterinarian_id",
                schema: "vetpass",
                table: "doses",
                column: "veterinarian_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_client_id",
                schema: "vetpass",
                table: "pets",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_name",
                schema: "vetpass",
                table: "pets",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_prescription_id",
                schema: "vetpass",
                table: "prescription_items",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_visit_id",
                schema: "vetpass",
                table: "prescriptions",
                column: "visit_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_items_vaccine_id_sequence_number",
                schema: "vetpass",
                table: "schedule_items",
                columns: new[] { "vaccine_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_client_id",
                schema: "vetpass",
                table: "user_profiles",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_clinic_id",
                schema: "vetpass",
                table: "user_profiles",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_email",
                schema: "vetpass",
                table: "user_profiles",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_cards_pet_id",
                schema: "vetpass",
                table: "vaccination_cards",
                column: "pet_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visits_pet_id_visit_date",
                schema: "vetpass",
                table: "visits",
                columns: new[] { "pet_id", "visit_date" });

            migrationBuilder.CreateIndex(
                name: "IX_visits_veterinarian_id",
                schema: "vetpass",
                table: "visits",
                column: "veterinarian_id");

            // El perfil comparte identificador con la cuenta que administra el
            // proveedor de identidad. La clave foránea no la modela Entity
            // Framework porque el esquema auth no pertenece a la aplicación.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables
                               WHERE table_schema = 'auth' AND table_name = 'users') THEN
                        ALTER TABLE vetpass.user_profiles
                            ADD CONSTRAINT fk_user_profiles_auth_users
                            FOREIGN KEY (id) REFERENCES auth.users (id) ON DELETE CASCADE;
                    END IF;
                END $$;
                """);

            // Seguridad a nivel de fila, habilitada y sin políticas: nadie
            // alcanza estas tablas por la interfaz REST automática del
            // proveedor. El único camino hacia los datos clínicos es esta API,
            // que se conecta con un rol que omite la restricción.
            foreach (var table in new[]
                     {
                         "clinics", "user_profiles", "clients", "pets", "vaccines", "schedule_items",
                         "vaccination_cards", "doses", "visits", "prescriptions", "prescription_items"
                     })
            {
                migrationBuilder.Sql($"ALTER TABLE vetpass.{table} ENABLE ROW LEVEL SECURITY;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE IF EXISTS vetpass.user_profiles
                    DROP CONSTRAINT IF EXISTS fk_user_profiles_auth_users;
                """);

            migrationBuilder.DropTable(
                name: "doses",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "prescription_items",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "schedule_items",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "vaccination_cards",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "prescriptions",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "vaccines",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "visits",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "pets",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "user_profiles",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "vetpass");

            migrationBuilder.DropTable(
                name: "clinics",
                schema: "vetpass");
        }
    }
}
