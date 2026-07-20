using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'worker@playstation.com')
                BEGIN
                    INSERT INTO Users (Email, PasswordHash, FullName, RoleId, IsActive, IsDeleted, CreatedAt)
                    VALUES ('worker@playstation.com', '$2a$11$dWedR6.EiY2XrPk.MMaOueJhoHQrzzTOXGqAckHElh9VgMNBZo6w2', 'PlayStation Worker', 2, 1, 0, '2024-01-01T00:00:00.0000000Z');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Users WHERE Email = 'worker@playstation.com'");
        }
    }
}
