using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Employees_CreatedByEmployeeId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyPayments_Employees_CreatedByEmployeeId",
                table: "MonthlyPayments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MonthlyPayments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MonthlyPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "MonthlyPayments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Lessons",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Lessons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Groups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Groups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AcademicYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                table: "AcademicYears",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AcademicYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByEmployeeId",
                table: "AcademicYears",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AcademicYears",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "AcademicYears",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedByEmployeeId", "UpdatedAt", "UpdatedByEmployeeId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Students_CreatedByEmployeeId",
                table: "Students",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UpdatedByEmployeeId",
                table: "Students",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedByEmployeeId",
                table: "Roles",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UpdatedByEmployeeId",
                table: "Roles",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CreatedByEmployeeId",
                table: "Permissions",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UpdatedByEmployeeId",
                table: "Permissions",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPayments_UpdatedByEmployeeId",
                table: "MonthlyPayments",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_UpdatedByEmployeeId",
                table: "Lessons",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedByEmployeeId",
                table: "Groups",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UpdatedByEmployeeId",
                table: "Groups",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedByEmployeeId",
                table: "Employees",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UpdatedByEmployeeId",
                table: "Employees",
                column: "UpdatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_CreatedByEmployeeId",
                table: "AcademicYears",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_UpdatedByEmployeeId",
                table: "AcademicYears",
                column: "UpdatedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Employees_CreatedByEmployeeId",
                table: "AcademicYears",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Employees_UpdatedByEmployeeId",
                table: "AcademicYears",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_CreatedByEmployeeId",
                table: "Employees",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_UpdatedByEmployeeId",
                table: "Employees",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Employees_CreatedByEmployeeId",
                table: "Groups",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Employees_UpdatedByEmployeeId",
                table: "Groups",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Employees_CreatedByEmployeeId",
                table: "Lessons",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Employees_UpdatedByEmployeeId",
                table: "Lessons",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyPayments_Employees_CreatedByEmployeeId",
                table: "MonthlyPayments",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyPayments_Employees_UpdatedByEmployeeId",
                table: "MonthlyPayments",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Employees_CreatedByEmployeeId",
                table: "Permissions",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Employees_UpdatedByEmployeeId",
                table: "Permissions",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Employees_CreatedByEmployeeId",
                table: "Roles",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Employees_UpdatedByEmployeeId",
                table: "Roles",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Employees_CreatedByEmployeeId",
                table: "Students",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Employees_UpdatedByEmployeeId",
                table: "Students",
                column: "UpdatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Employees_CreatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Employees_UpdatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_CreatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_UpdatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Employees_CreatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Employees_UpdatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Employees_CreatedByEmployeeId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Employees_UpdatedByEmployeeId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyPayments_Employees_CreatedByEmployeeId",
                table: "MonthlyPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyPayments_Employees_UpdatedByEmployeeId",
                table: "MonthlyPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Employees_CreatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Employees_UpdatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Employees_CreatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Employees_UpdatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Employees_CreatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Employees_UpdatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CreatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UpdatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Roles_CreatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_UpdatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_CreatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_UpdatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPayments_UpdatedByEmployeeId",
                table: "MonthlyPayments");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_UpdatedByEmployeeId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_UpdatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CreatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UpdatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_CreatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_UpdatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "AcademicYears");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MonthlyPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Lessons",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Employees_CreatedByEmployeeId",
                table: "Lessons",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyPayments_Employees_CreatedByEmployeeId",
                table: "MonthlyPayments",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
