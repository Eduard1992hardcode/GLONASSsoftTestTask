using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLONASSsoftTestTask.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TestTask");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "TestTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    SecondName = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatisticTask",
                schema: "TestTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Percent = table.Column<decimal>(type: "numeric", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatisticTask_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "TestTask",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatisticTask_User_UserId1",
                        column: x => x.UserId1,
                        principalSchema: "TestTask",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatisticTaskResult",
                schema: "TestTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountSignIn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticTaskResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatisticTaskResult_StatisticTask_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "TestTask",
                        principalTable: "StatisticTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticTask_ResultId",
                schema: "TestTask",
                table: "StatisticTask",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticTask_UserId",
                schema: "TestTask",
                table: "StatisticTask",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticTask_UserId1",
                schema: "TestTask",
                table: "StatisticTask",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticTaskResult_TaskId",
                schema: "TestTask",
                table: "StatisticTaskResult",
                column: "TaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StatisticTask_StatisticTaskResult_ResultId",
                schema: "TestTask",
                table: "StatisticTask",
                column: "ResultId",
                principalSchema: "TestTask",
                principalTable: "StatisticTaskResult",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatisticTask_StatisticTaskResult_ResultId",
                schema: "TestTask",
                table: "StatisticTask");

            migrationBuilder.DropTable(
                name: "StatisticTaskResult",
                schema: "TestTask");

            migrationBuilder.DropTable(
                name: "StatisticTask",
                schema: "TestTask");

            migrationBuilder.DropTable(
                name: "User",
                schema: "TestTask");
        }
    }
}
