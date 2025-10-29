using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarriorExperiment.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReflectionQuestionsToSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatDidNotGoWell",
                table: "daily_surveys",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatToChangeTomorrow",
                table: "daily_surveys",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatWentWell",
                table: "daily_surveys",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatDidNotGoWell",
                table: "daily_surveys");

            migrationBuilder.DropColumn(
                name: "WhatToChangeTomorrow",
                table: "daily_surveys");

            migrationBuilder.DropColumn(
                name: "WhatWentWell",
                table: "daily_surveys");
        }
    }
}
