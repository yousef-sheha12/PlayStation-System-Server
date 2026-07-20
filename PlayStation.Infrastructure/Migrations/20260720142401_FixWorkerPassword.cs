using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkerPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Users WHERE Email = 'worker@playstation.com')
                    UPDATE Users SET PasswordHash = '$2a$11$9dOZzNTqZIyRJaZYvv6ULegnr.9DLazmOY6dS3Rr3UFhD3VVbu20e' WHERE Email = 'worker@playstation.com'
                ELSE
                    INSERT INTO Users (Email, PasswordHash, FullName, RoleId, IsActive, IsDeleted, CreatedAt)
                    VALUES ('worker@playstation.com', '$2a$11$9dOZzNTqZIyRJaZYvv6ULegnr.9DLazmOY6dS3Rr3UFhD3VVbu20e', 'PlayStation Worker', 2, 1, 0, '2024-01-01T00:00:00.0000000Z');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Users WHERE Email = 'worker@playstation.com'");
        }
    }
}
