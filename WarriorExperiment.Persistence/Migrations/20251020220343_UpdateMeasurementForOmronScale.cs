using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarriorExperiment.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMeasurementForOmronScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoneMass",
                table: "measurement_entries");

            migrationBuilder.RenameColumn(
                name: "WaterPercentage",
                table: "measurement_entries",
                newName: "MuscleMassPercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MuscleMassPercentage",
                table: "measurement_entries",
                newName: "WaterPercentage");

            migrationBuilder.AddColumn<decimal>(
                name: "BoneMass",
                table: "measurement_entries",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }
    }
}
