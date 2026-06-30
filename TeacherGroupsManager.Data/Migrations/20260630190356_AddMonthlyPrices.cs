using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherGroupsManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "Groups",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "AcademicYears",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AcademicYears",
                keyColumn: "Id",
                keyValue: 1,
                column: "MonthlyPrice",
                value: 600m);

            migrationBuilder.UpdateData(
                table: "AcademicYears",
                keyColumn: "Id",
                keyValue: 2,
                column: "MonthlyPrice",
                value: 600m);

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1,
                column: "MonthlyPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 2,
                column: "MonthlyPrice",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPayments_StudentId_Month_Year",
                table: "MonthlyPayments",
                columns: new[] { "StudentId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonthlyPayments_StudentId_Month_Year",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "AcademicYears");

        }
    }
}
