using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendanceNotes",
                table: "LessonStudents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceStatus",
                table: "LessonStudents",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceNotes",
                table: "LessonStudents");

            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                table: "LessonStudents");
        }
    }
}
