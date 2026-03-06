using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesForceSync.Migrations
{
    /// <inheritdoc />
    public partial class AddConflictLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConflictLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    SalesForceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalLastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesForceLastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConflictLogs");
        }
    }
}
