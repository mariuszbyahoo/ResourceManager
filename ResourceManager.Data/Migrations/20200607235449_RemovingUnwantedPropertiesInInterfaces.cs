using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ResourceManager.Data.Migrations
{
    public partial class RemovingUnwantedPropertiesInInterfaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "LeasedTo",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OccupiedTill",
                table: "Resources");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Availability",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LeasedTo",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "OccupiedTill",
                table: "Resources",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
