using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByEmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupSchedules_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupSchedules_Employees_UpdatedByEmployeeId",
                        column: x => x.UpdatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupSchedules_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO GroupSchedules (GroupId, DayOfWeek, StartTime, EndTime, CreatedAt, CreatedByEmployeeId, UpdatedAt, UpdatedByEmployeeId)
                SELECT Id, DayOfWeek, StartTime, EndTime, CreatedAt, CreatedByEmployeeId, UpdatedAt, UpdatedByEmployeeId
                FROM Groups AS source
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM GroupSchedules AS schedule
                    WHERE schedule.GroupId = source.Id
                )
                """);

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedules_CreatedByEmployeeId",
                table: "GroupSchedules",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedules_GroupId_DayOfWeek_StartTime",
                table: "GroupSchedules",
                columns: new[] { "GroupId", "DayOfWeek", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedules_UpdatedByEmployeeId",
                table: "GroupSchedules",
                column: "UpdatedByEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupSchedules");
        }
    }
}
