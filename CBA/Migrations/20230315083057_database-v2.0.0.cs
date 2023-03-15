using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CBA.Migrations
{
    /// <inheritdoc />
    public partial class databasev200 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_logPerson",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personID = table.Column<long>(type: "bigint", nullable: true),
                    deviceID = table.Column<long>(type: "bigint", nullable: true),
                    image = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_logPerson", x => x.ID);
                    table.ForeignKey(
                        name: "FK_tb_logPerson_tb_device_deviceID",
                        column: x => x.deviceID,
                        principalTable: "tb_device",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_tb_logPerson_tb_person_personID",
                        column: x => x.personID,
                        principalTable: "tb_person",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_logPerson_deviceID",
                table: "tb_logPerson",
                column: "deviceID");

            migrationBuilder.CreateIndex(
                name: "IX_tb_logPerson_personID",
                table: "tb_logPerson",
                column: "personID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_logPerson");
        }
    }
}
