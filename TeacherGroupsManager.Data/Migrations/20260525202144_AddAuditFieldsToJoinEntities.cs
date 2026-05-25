using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToJoinEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "RolePermissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "RolePermissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LessonStudents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "LessonStudents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LessonStudents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "LessonStudents",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 2 },
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CreatedByEmployeeId",
                table: "RolePermissions",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_UpdatedByEmployeeId",
                table: "RolePermissions",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonStudents_CreatedByEmployeeId",
                table: "LessonStudents",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonStudents_UpdatedByEmployeeId",
                table: "LessonStudents",
                column: "UpdatedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonStudents_Employees_CreatedByEmployeeId",
                table: "LessonStudents",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonStudents_Employees_UpdatedByEmployeeId",
                table: "LessonStudents",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Employees_CreatedByEmployeeId",
                table: "RolePermissions",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Employees_UpdatedByEmployeeId",
                table: "RolePermissions",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonStudents_Employees_CreatedByEmployeeId",
                table: "LessonStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonStudents_Employees_UpdatedByEmployeeId",
                table: "LessonStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Employees_CreatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Employees_UpdatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CreatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_UpdatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_LessonStudents_CreatedByEmployeeId",
                table: "LessonStudents");

            migrationBuilder.DropIndex(
                name: "IX_LessonStudents_UpdatedByEmployeeId",
                table: "LessonStudents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LessonStudents");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "LessonStudents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LessonStudents");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "LessonStudents");
        }
    }
}
