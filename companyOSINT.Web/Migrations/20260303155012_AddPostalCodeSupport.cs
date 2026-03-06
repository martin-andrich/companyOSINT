using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace companyOSINT.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPostalCodeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostalCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character(5)", fixedLength: true, maxLength: 5, nullable: false),
                    Place = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalCodes", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_PostalCode",
                table: "Companies",
                column: "PostalCode");

            // Backfill: extract 5-digit PLZ from RegisteredAddress (format: "Street, 01069 City")
            migrationBuilder.Sql("""
                UPDATE "Companies"
                SET "PostalCode" = substring("RegisteredAddress" from ',\s*(\d{5})\s')
                WHERE "RegisteredAddress" IS NOT NULL
                  AND "RegisteredAddress" ~ ',\s*\d{5}\s'
                  AND "PostalCode" IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalCodes");

            migrationBuilder.DropIndex(
                name: "IX_Companies_PostalCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Companies");
        }
    }
}
