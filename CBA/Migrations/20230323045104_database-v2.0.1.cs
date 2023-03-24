using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CBA.Migrations
{
    /// <inheritdoc />
    public partial class databasev201 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "des",
                table: "tb_person",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "levelID",
                table: "tb_person",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_age",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    des = table.Column<string>(type: "text", nullable: false),
                    low = table.Column<int>(type: "integer", nullable: false),
                    high = table.Column<int>(type: "integer", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_age", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_person_levelID",
                table: "tb_person",
                column: "levelID");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_person_tb_age_levelID",
                table: "tb_person",
                column: "levelID",
                principalTable: "tb_age",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_person_tb_age_levelID",
                table: "tb_person");

            migrationBuilder.DropTable(
                name: "tb_age");

            migrationBuilder.DropIndex(
                name: "IX_tb_person_levelID",
                table: "tb_person");

            migrationBuilder.DropColumn(
                name: "des",
                table: "tb_person");

            migrationBuilder.DropColumn(
                name: "levelID",
                table: "tb_person");
        }
    }
}
