using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ResourceManager.Data.Migrations
{
    public partial class AddingNewDataTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourceDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Availability = table.Column<int>(nullable: false),
                    LeasedTo = table.Column<Guid>(nullable: false),
                    OccupiedTill = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDatas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceDatas");

            migrationBuilder.DropTable(
                name: "TenantDatas");
        }
    }
}
