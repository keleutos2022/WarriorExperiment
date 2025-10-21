using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarriorExperiment.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRulesAcceptanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RulesAcceptedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RulesAcceptedName",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RulesAcceptedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RulesAcceptedName",
                table: "users");
        }
    }
}
