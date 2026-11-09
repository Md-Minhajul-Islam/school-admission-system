using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolAdmission.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CaseInsensitiveClassNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_classes_name",
                table: "classes");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_classes_name ON classes (lower(name));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ux_classes_name;");

            migrationBuilder.CreateIndex(
                name: "ux_classes_name",
                table: "classes",
                column: "name",
                unique: true);
        }
    }
}
