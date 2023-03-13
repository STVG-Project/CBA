using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CBA.Migrations
{
    /// <inheritdoc />
    public partial class databasev00 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_device",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    des = table.Column<string>(type: "text", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_device", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tb_group",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    des = table.Column<string>(type: "text", nullable: false),
                    createdTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastestTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_group", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tb_role",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    des = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_role", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tb_person",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    age = table.Column<int>(type: "integer", nullable: false),
                    groupID = table.Column<long>(type: "bigint", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    lastestTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_person", x => x.ID);
                    table.ForeignKey(
                        name: "FK_tb_person_tb_group_groupID",
                        column: x => x.groupID,
                        principalTable: "tb_group",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "tb_user",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    displayName = table.Column<string>(type: "text", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    phoneNumber = table.Column<string>(type: "text", nullable: false),
                    des = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    roleID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_user", x => x.ID);
                    table.ForeignKey(
                        name: "FK_tb_user_tb_role_roleID",
                        column: x => x.roleID,
                        principalTable: "tb_role",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "tb_face",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    age = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    personID = table.Column<long>(type: "bigint", nullable: true),
                    deviceID = table.Column<long>(type: "bigint", nullable: true),
                    createdTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_face", x => x.ID);
                    table.ForeignKey(
                        name: "FK_tb_face_tb_device_deviceID",
                        column: x => x.deviceID,
                        principalTable: "tb_device",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_tb_face_tb_person_personID",
                        column: x => x.personID,
                        principalTable: "tb_person",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SqlGroupSqlUser",
                columns: table => new
                {
                    groupsID = table.Column<long>(type: "bigint", nullable: false),
                    usersID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlGroupSqlUser", x => new { x.groupsID, x.usersID });
                    table.ForeignKey(
                        name: "FK_SqlGroupSqlUser_tb_group_groupsID",
                        column: x => x.groupsID,
                        principalTable: "tb_group",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SqlGroupSqlUser_tb_user_usersID",
                        column: x => x.usersID,
                        principalTable: "tb_user",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SqlGroupSqlUser_usersID",
                table: "SqlGroupSqlUser",
                column: "usersID");

            migrationBuilder.CreateIndex(
                name: "IX_tb_face_deviceID",
                table: "tb_face",
                column: "deviceID");

            migrationBuilder.CreateIndex(
                name: "IX_tb_face_personID",
                table: "tb_face",
                column: "personID");

            migrationBuilder.CreateIndex(
                name: "IX_tb_person_groupID",
                table: "tb_person",
                column: "groupID");

            migrationBuilder.CreateIndex(
                name: "IX_tb_user_roleID",
                table: "tb_user",
                column: "roleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SqlGroupSqlUser");

            migrationBuilder.DropTable(
                name: "tb_face");

            migrationBuilder.DropTable(
                name: "tb_user");

            migrationBuilder.DropTable(
                name: "tb_device");

            migrationBuilder.DropTable(
                name: "tb_person");

            migrationBuilder.DropTable(
                name: "tb_role");

            migrationBuilder.DropTable(
                name: "tb_group");
        }
    }
}
