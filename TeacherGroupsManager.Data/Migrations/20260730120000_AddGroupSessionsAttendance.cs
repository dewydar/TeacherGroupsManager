using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations;

public partial class AddGroupSessionsAttendance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GroupSessions",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                GroupId = table.Column<int>(nullable: false), SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                PlannedStartTime = table.Column<TimeOnly>(type: "time", nullable: false), PlannedEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                ActualStartTime = table.Column<DateTime>(nullable: true), ActualEndTime = table.Column<DateTime>(nullable: true),
                Status = table.Column<int>(nullable: false), Topic = table.Column<string>(maxLength: 500, nullable: true), Notes = table.Column<string>(maxLength: 500, nullable: true), CancellationReason = table.Column<string>(maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: true), UpdatedAt = table.Column<DateTime>(nullable: true), CreatedByEmployeeId = table.Column<int>(nullable: true), UpdatedByEmployeeId = table.Column<int>(nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_GroupSessions", x => x.Id); table.ForeignKey("FK_GroupSessions_Groups_GroupId", x => x.GroupId, "Groups", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateTable(
            name: "StudentSessionAttendances",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), GroupSessionId = table.Column<int>(nullable: false), StudentId = table.Column<int>(nullable: false),
                AttendanceStatus = table.Column<int>(nullable: false), CheckInTime = table.Column<DateTime>(nullable: true), CheckInMethod = table.Column<int>(nullable: true), LateMinutes = table.Column<int>(nullable: false), DepartureStatus = table.Column<int>(nullable: false), CheckOutTime = table.Column<DateTime>(nullable: true), CheckOutMethod = table.Column<int>(nullable: true), ExcuseReason = table.Column<string>(maxLength: 500, nullable: true), Notes = table.Column<string>(maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: true), UpdatedAt = table.Column<DateTime>(nullable: true), CreatedByEmployeeId = table.Column<int>(nullable: true), UpdatedByEmployeeId = table.Column<int>(nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_StudentSessionAttendances", x => x.Id); table.ForeignKey("FK_StudentSessionAttendances_GroupSessions_GroupSessionId", x => x.GroupSessionId, "GroupSessions", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_StudentSessionAttendances_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateIndex("IX_GroupSessions_GroupId_SessionDate_PlannedStartTime", "GroupSessions", new[] { "GroupId", "SessionDate", "PlannedStartTime" }, unique: true);
        migrationBuilder.CreateIndex("IX_StudentSessionAttendances_GroupSessionId_StudentId", "StudentSessionAttendances", new[] { "GroupSessionId", "StudentId" }, unique: true);
        migrationBuilder.CreateIndex("IX_GroupSessions_CreatedByEmployeeId", "GroupSessions", "CreatedByEmployeeId");
        migrationBuilder.CreateIndex("IX_GroupSessions_UpdatedByEmployeeId", "GroupSessions", "UpdatedByEmployeeId");
        migrationBuilder.CreateIndex("IX_StudentSessionAttendances_CreatedByEmployeeId", "StudentSessionAttendances", "CreatedByEmployeeId");
        migrationBuilder.CreateIndex("IX_StudentSessionAttendances_UpdatedByEmployeeId", "StudentSessionAttendances", "UpdatedByEmployeeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable("StudentSessionAttendances"); migrationBuilder.DropTable("GroupSessions"); }
}
