using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroupTeacherRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Employees_AssistantTeacherId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Employees_TeacherId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_AssistantTeacherId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_TeacherId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "AssistantTeacherId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssistantTeacherId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AssistantTeacherId", "TeacherId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AssistantTeacherId", "TeacherId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_AssistantTeacherId",
                table: "Groups",
                column: "AssistantTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TeacherId",
                table: "Groups",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Employees_AssistantTeacherId",
                table: "Groups",
                column: "AssistantTeacherId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Employees_TeacherId",
                table: "Groups",
                column: "TeacherId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
