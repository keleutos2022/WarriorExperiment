using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarriorExperiment.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyTasksAndPreferredTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PreferredTime = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    EnteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "daily_task_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyTaskId = table.Column<int>(type: "integer", nullable: false),
                    DailySurveyEntryId = table.Column<int>(type: "integer", nullable: false),
                    Done = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    EnteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_task_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_daily_task_entries_daily_surveys_DailySurveyEntryId",
                        column: x => x.DailySurveyEntryId,
                        principalTable: "daily_surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_daily_task_entries_daily_tasks_DailyTaskId",
                        column: x => x.DailyTaskId,
                        principalTable: "daily_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_task_entries_DailySurveyEntryId",
                table: "daily_task_entries",
                column: "DailySurveyEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_task_entries_DailyTaskId_DailySurveyEntryId",
                table: "daily_task_entries",
                columns: new[] { "DailyTaskId", "DailySurveyEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_tasks_Name",
                table: "daily_tasks",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_task_entries");

            migrationBuilder.DropTable(
                name: "daily_tasks");
        }
    }
}
