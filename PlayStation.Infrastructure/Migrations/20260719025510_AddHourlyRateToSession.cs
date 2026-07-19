using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRateToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Sessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$D9BXICZEmth0S.nR0KV0WeQ7.hD7XMPsj/6PxtYLf7v/f1DV/CCY6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Sessions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$tE/bekqFDxZqjB9X2CWWeupeID6ikjukABcuJaiFCIltJwm0n6Xm2");
        }
    }
}
