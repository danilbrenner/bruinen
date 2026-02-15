using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bruinen.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    login = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.login);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_login",
                table: "users",
                column: "login");
            
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "login", "password_hash" },
                values: new object[] { "admin", "$argon2id$v=19$m=65536,t=3,p=1$5FatAUHEi3Smy3iWnc71oA$LpGilh0x9K+OXQ5Fo30RTk1/CxMLp4aajYFHF8heoY4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
