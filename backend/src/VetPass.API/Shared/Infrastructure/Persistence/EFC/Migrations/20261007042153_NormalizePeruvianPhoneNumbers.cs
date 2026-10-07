using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePeruvianPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los teléfonos registrados antes de la regla se llevan a su forma
            // canónica, +51 seguido del número nacional. Solo se tocan los que
            // son celulares peruanos inequívocos —nueve dígitos que empiezan con
            // 9—: cualquier otro valor se deja como está, para que lo corrija una
            // persona y no una suposición de la migración.
            migrationBuilder.Sql("""
                UPDATE vetpass.clients
                SET phone_number = '+51' || regexp_replace(phone_number, '\D', '', 'g')
                WHERE phone_number NOT LIKE '+51%'
                  AND regexp_replace(phone_number, '\D', '', 'g') ~ '^9[0-9]{8}$';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La forma canónica es el mismo número escrito de otra manera: no
            // hay un estado anterior que valga la pena restaurar.
        }
    }
}
