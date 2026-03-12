using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bruinen.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequestCounterAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "request_counters",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_counters", x => x.key);
                });

            migrationBuilder.CreateIndex(
                name: "ix_request_counters_key",
                table: "request_counters",
                column: "key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "request_counters");
        }
    }
}
