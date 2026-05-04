using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kilozdazolik.WaterLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddVolumeToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaterLogs_VesselTypes_VesselTypeId",
                table: "WaterLogs");

            migrationBuilder.AlterColumn<int>(
                name: "VesselTypeId",
                table: "WaterLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "Volume",
                table: "WaterLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_WaterLogs_VesselTypes_VesselTypeId",
                table: "WaterLogs",
                column: "VesselTypeId",
                principalTable: "VesselTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaterLogs_VesselTypes_VesselTypeId",
                table: "WaterLogs");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "WaterLogs");

            migrationBuilder.AlterColumn<int>(
                name: "VesselTypeId",
                table: "WaterLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WaterLogs_VesselTypes_VesselTypeId",
                table: "WaterLogs",
                column: "VesselTypeId",
                principalTable: "VesselTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
