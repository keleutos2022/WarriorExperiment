using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarriorExperiment.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReflectionQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FunActivity",
                table: "daily_surveys",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GratefulFor",
                table: "daily_surveys",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookingForwardTo",
                table: "daily_surveys",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FunActivity",
                table: "daily_surveys");

            migrationBuilder.DropColumn(
                name: "GratefulFor",
                table: "daily_surveys");

            migrationBuilder.DropColumn(
                name: "LookingForwardTo",
                table: "daily_surveys");
        }
    }
}
