using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace companyOSINT.Web.Migrations
{
    /// <inheritdoc />
    public partial class initDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainsToSkip",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainsToSkip", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    TokenPrefix = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjects_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RegisteredAddress = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    FederalState = table.Column<string>(type: "text", nullable: true),
                    RegisteredOffice = table.Column<string>(type: "text", nullable: true),
                    Registrar = table.Column<string>(type: "text", nullable: true),
                    RegisterArt = table.Column<string>(type: "text", nullable: true),
                    RegisterNummer = table.Column<string>(type: "text", nullable: true),
                    SectorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activity = table.Column<string>(type: "text", nullable: true),
                    DateLastChecked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    MaidenName = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Flag = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<string>(type: "text", nullable: true),
                    EndDate = table.Column<string>(type: "text", nullable: true),
                    Dismissed = table.Column<string>(type: "text", nullable: true),
                    ReferenceNo = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProjectCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectCompanies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjectCompanies_UserProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "UserProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UrlWebsite = table.Column<string>(type: "text", nullable: true),
                    UrlImprint = table.Column<string>(type: "text", nullable: true),
                    HttpResponseCode = table.Column<int>(type: "integer", nullable: false),
                    IsSubdomain = table.Column<bool>(type: "boolean", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    SslValid = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentManagerFound = table.Column<bool>(type: "boolean", nullable: false),
                    RequestsWithoutConsent = table.Column<int>(type: "integer", nullable: false),
                    CookiesWithoutConsent = table.Column<int>(type: "integer", nullable: false),
                    AverageTimeToFirstByte = table.Column<double>(type: "double precision", nullable: false),
                    DateLastChecked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Websites_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Software",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    FoundAt = table.Column<string>(type: "text", nullable: false),
                    WebsiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastChecked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Software", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Software_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FoundAt = table.Column<string>(type: "text", nullable: false),
                    WebsiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tools_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DomainsToSkip",
                columns: new[] { "Id", "DateCreated", "DateModified", "Domain" },
                values: new object[,]
                {
                    { new Guid("00cb64e5-5b0a-07e8-e6e4-6fed82e4333a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "yelp.com" },
                    { new Guid("0466f478-2e12-470a-c614-e876ec02533b"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "google.de" },
                    { new Guid("0b46c68d-b0bb-7588-7ed6-7ed8fb316b1b"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "instagram.com" },
                    { new Guid("0eba3dcd-ed37-edf3-fd42-ccc21e598e10"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "amazon.de" },
                    { new Guid("196949cb-eb20-bf99-8929-90332db2af70"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tiktok.com" },
                    { new Guid("1c9fb30b-1339-7bce-3d82-bd01d56cae4b"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "europages.de" },
                    { new Guid("216cea36-ca35-a48c-7997-c751d4bc4f6f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tripadvisor.de" },
                    { new Guid("2ae4cacb-bc4d-56f0-0630-6a3650099e98"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "northdata.com" },
                    { new Guid("3828a8a2-3864-a06c-4a56-94e8b58759e6"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "amazon.com" },
                    { new Guid("440cac45-940d-094a-ae75-42869be6b47f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hoppenstedt.de" },
                    { new Guid("4f1c9876-ee79-ff51-759c-02096918e1bb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ebay.de" },
                    { new Guid("57b524de-ea1d-7cea-f4b2-a5c00ef66fb7"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "apple.com" },
                    { new Guid("5ba00f85-a815-c40c-971d-535c0ec5c4c8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bundesanzeiger.de" },
                    { new Guid("63b44df5-0975-e040-e7f7-630fe327845e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "linkedin.com" },
                    { new Guid("65ad6b55-1efc-c1e5-3db4-dc1a7bd29eba"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "golocal.de" },
                    { new Guid("6652dd14-07c7-bd89-c806-364df4586335"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "youtube.com" },
                    { new Guid("679e3a82-41d1-372c-7e6d-b0b1b387009a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "11880.com" },
                    { new Guid("69ac119d-0bc2-a0a4-bbbe-fe430a218a0b"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "gelbeseiten.de" },
                    { new Guid("69b8870d-69f6-3b5e-fc48-059b24b0d201"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "firmenwissen.de" },
                    { new Guid("71032630-87a5-8361-7808-7fdc2f059297"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kununu.com" },
                    { new Guid("7638babd-f43e-7a86-dfd6-54759bde6f73"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "xing.com" },
                    { new Guid("77de6af6-827e-74b3-f4b3-607c57f4ec3e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tripadvisor.com" },
                    { new Guid("78ec4323-4ca0-a96e-d808-06345d31fd78"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "facebook.com" },
                    { new Guid("85fbbbfc-34c2-4b77-1bd0-af8708ceda1d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bisnode.de" },
                    { new Guid("887f5f46-e431-09cb-ba66-21dab2a4d6ee"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "wikipedia.org" },
                    { new Guid("9175967b-7a7b-1952-1d7e-ea0279fda3ec"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "yelp.de" },
                    { new Guid("92e2c812-6ca4-2905-663c-d963419e4645"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ebay.com" },
                    { new Guid("96c92ce4-b5c8-ce63-f2bf-ab18408d310e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "creditreform.de" },
                    { new Guid("a323439a-6942-ab83-2474-4f3fdbd15200"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "x.com" },
                    { new Guid("a37dfbbd-99c1-37cc-651d-e7e128cbb234"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "unternehmensregister.de" },
                    { new Guid("aca55a2d-0e3f-f6c1-1db6-d18d78615c3f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "dasoertliche.de" },
                    { new Guid("b226540a-5b51-c8d5-e76b-d079f3f0e1a5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "handelsregister.de" },
                    { new Guid("c4d10579-2ce1-9354-3a44-d19fcd5f9356"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "twitter.com" },
                    { new Guid("c5594f2d-350e-5d5a-9bed-bd409fc1caed"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "companyhouse.com" },
                    { new Guid("cacf4661-054c-534b-9b6b-0b5a6f6d185a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "europages.com" },
                    { new Guid("cf2aee9c-3450-6f82-4362-a5ea0fb46317"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "wlw.de" },
                    { new Guid("d5c7813f-384d-bae6-8592-b0308cb31333"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "facebook.de" },
                    { new Guid("e1bd3c68-b4a2-f79c-13be-7802f671f892"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "wikidata.org" },
                    { new Guid("f0685844-a46d-9f8d-4631-ddd35ae2da76"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "northdata.de" },
                    { new Guid("f420591d-4bb4-a827-02bd-77c4f0536f5a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "google.com" },
                    { new Guid("fcee17dd-c06b-b18c-4dd3-53d803b81dd7"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "dnb.com" },
                    { new Guid("ff9c0816-8db1-e04c-1755-291c873a9efd"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "companyhouse.de" }
                });

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "Id", "DateCreated", "DateModified", "Name" },
                values: new object[,]
                {
                    { new Guid("0745ca62-8cd5-004b-c646-d312c460eff3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Einzelhandel" },
                    { new Guid("07688fc8-a859-efb7-2dc4-52a756705914"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Spedition & Logistik" },
                    { new Guid("0ac25a9f-5f47-6e68-12ce-0a82250fc291"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Design & Kreativwirtschaft" },
                    { new Guid("0f3d3acb-72ac-833f-cad5-53c9982c0c45"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luft- & Raumfahrttechnik" },
                    { new Guid("0fad5f25-3f5d-1076-b243-154bdcbe8f7e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finanzdienstleistungen & Vermögensverwaltung" },
                    { new Guid("1039c6ae-5b7b-4947-1c34-b43180e1c190"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Optik & Feinmechanik" },
                    { new Guid("10836a6f-ff4d-8f9e-ed68-d6f9cdf639fb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bergbau & Rohstoffe" },
                    { new Guid("18cafd21-2bf7-3e57-e0c5-2f6a109882c2"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cybersecurity" },
                    { new Guid("1d868e48-0c3e-732f-d9a1-5cc2b8870f8a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Maschinenbau" },
                    { new Guid("215e2621-bb61-f1b5-eb2b-5519dfa9d408"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Entsorgung & Recycling" },
                    { new Guid("237b98cd-368e-1c71-af02-c4ef04085753"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Erneuerbare Energien" },
                    { new Guid("24776bed-f4cc-faa2-ceed-1566401cfa3a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forschung & Entwicklung" },
                    { new Guid("253f9c40-d86b-da29-90cc-83db8eb3644c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Softwareentwicklung" },
                    { new Guid("2cfb812d-7a2a-45f9-8de9-9d22aa92e10c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luftfahrt" },
                    { new Guid("3db09bf9-9a08-4223-3004-f1a91bae0b1a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Metallerzeugung & -verarbeitung" },
                    { new Guid("3fe1f230-425b-e1ba-5b4a-b50aff85cca9"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Event & Veranstaltung" },
                    { new Guid("40553504-ee27-dbfd-8e99-c1d1f814c705"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Online-Handel & E-Commerce" },
                    { new Guid("416009de-6361-d0be-d3d8-cd910cc3b900"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Landwirtschaft & Gartenbau" },
                    { new Guid("43044823-6e0b-b25b-34b8-d44097c9c97b"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Handwerk" },
                    { new Guid("4605adb3-3d92-95e1-8f62-df6f0561f7c8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Personaldienstleistung" },
                    { new Guid("49e5b835-f89f-2d9b-ca85-2ae72e051457"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Versicherungen" },
                    { new Guid("4e9c512d-38f5-55d1-3cf1-6de8f866f31d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Immobilienwirtschaft" },
                    { new Guid("4eeca4dc-7716-006b-3ce3-4548576fe4c8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Steuerberatung & Wirtschaftsprüfung" },
                    { new Guid("525bf4e9-cc7d-b84f-a01f-d04c803bd82f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rechtsberatung" },
                    { new Guid("539b5f3d-5018-1263-66bb-8eb024e5e512"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tiefbau & Infrastruktur" },
                    { new Guid("5517a5b5-6f99-8c52-03f6-5ed8316fc14d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bildung & Weiterbildung" },
                    { new Guid("55755328-107b-2af6-51cf-d2d3bfc48756"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Öffentlichkeitsarbeit & PR" },
                    { new Guid("55fbb727-1729-ff84-fcc4-f942379c28b2"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medizintechnik" },
                    { new Guid("58c8c2fd-1a39-1ff4-0c5f-7b48f181b83e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sozialwesen" },
                    { new Guid("5cc310ed-ee43-afbf-2275-9983d00bf44c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reinigung & Gebäudedienste" },
                    { new Guid("5cce7692-2613-d7b1-6b15-3c9b2d8fac0c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gebäudetechnik & Haustechnik" },
                    { new Guid("5d908a40-ad70-1855-9cfe-22cbdda36174"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "IT-Dienstleistungen & Beratung" },
                    { new Guid("6cbf6337-bd70-b6f7-bf62-2c992c707771"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tourismus & Reiseveranstaltung" },
                    { new Guid("6d16fff8-1f9f-8dee-2708-641ee40aaa47"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pflege & Betreuung" },
                    { new Guid("742eb0f2-2263-87e8-66c2-cd4ca7d5ac1d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sicherheitsdienstleistungen" },
                    { new Guid("76e042eb-3c6f-9d8b-ab36-200c38f0e53d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unternehmensberatung" },
                    { new Guid("78fb5b08-819b-e3a5-0a2f-387af7e92810"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kunststoff & Gummi" },
                    { new Guid("7f2ceda7-6a06-6e7e-381f-785ca3256879"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bankwesen" },
                    { new Guid("827f4e9f-9050-b0eb-4042-e126bde0c7cb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kultur & Unterhaltung" },
                    { new Guid("8a3ae1f9-d07a-ce59-041c-a864add3258f"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Telekommunikation" },
                    { new Guid("8b0f99ae-4219-ba20-ebdf-23825474a42a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Holz- & Möbelindustrie" },
                    { new Guid("8ddd8e7f-67de-728e-0c49-8309bc2d9c4e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wasser- & Abwasserwirtschaft" },
                    { new Guid("98e73429-d2aa-8349-dcda-83b6130a5dd0"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Großhandel" },
                    { new Guid("99a8b4a4-0d5f-114d-e80b-938711213f87"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rechenzentren & Cloud-Dienste" },
                    { new Guid("9e27184c-b36e-48ee-8d0e-db6c5476454e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Automobilindustrie & Fahrzeugbau" },
                    { new Guid("9fa8b0a9-2da4-c7f1-4fff-071b0daa0420"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Textil & Bekleidung" },
                    { new Guid("a5e1985c-02f1-2afe-d70b-acf6e8204e07"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Glas, Keramik & Baustoffe" },
                    { new Guid("a67562bf-d1dd-936f-f4c4-baccdcf789e3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gastronomie" },
                    { new Guid("ab872864-74b7-9e6e-8e2f-68b3631ad5a8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Verlagswesen" },
                    { new Guid("b011a47e-e375-6acb-5d0e-5e45ddbd3ed7"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Baugewerbe & Hochbau" },
                    { new Guid("b8fbe765-7d15-43f2-0dae-d0aea3a46418"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kraftfahrzeughandel" },
                    { new Guid("ba76e261-0ad3-f9af-351c-4b7a53204e82"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sport & Freizeit" },
                    { new Guid("bc05159f-064b-72ad-08be-edc71cf19f5d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gesundheitswesen & Kliniken" },
                    { new Guid("c2fa02d9-d311-3a6b-7840-bcf785641654"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elektrotechnik & Elektronik" },
                    { new Guid("c76472e0-b7e9-9632-53a5-4ef7a15db747"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Personenbeförderung" },
                    { new Guid("cc6c3cc7-b102-1405-5974-663ce06578b8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forstwirtschaft & Holzwirtschaft" },
                    { new Guid("d09e1783-3d24-be6d-67f9-c51ed68baae6"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Energieversorgung" },
                    { new Guid("d15ebff7-582c-a133-ce3c-ff12b083ddb4"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Schifffahrt & Hafenwirtschaft" },
                    { new Guid("d8f723ec-513b-305b-5c21-3ea25d5af346"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Marketing & Werbung" },
                    { new Guid("e13e0398-7450-685c-28eb-7712afda0c21"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Architektur & Planung" },
                    { new Guid("e183a3b9-f2cc-a7d5-5d35-ecc18c96761a"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Film, TV & Rundfunk" },
                    { new Guid("e4687531-43de-7492-9714-f3713e193903"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Facility Management" },
                    { new Guid("ec24f9b4-3b45-fa0b-1583-b6036f7201d3"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hotellerie & Beherbergung" },
                    { new Guid("f2d9f386-2630-7b9a-9b8e-40adeba9d510"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chemie & Pharma" },
                    { new Guid("fac5ef3b-31af-7ee2-87d2-c204ce7e8b7d"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lebensmittelherstellung" },
                    { new Guid("fcd616ac-7895-d1e0-a332-47ef62741328"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Papier & Druck" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiTokens_TokenHash",
                table: "ApiTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiTokens_UserId",
                table: "ApiTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DateLastChecked",
                table: "Companies",
                column: "DateLastChecked");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_PostalCode",
                table: "Companies",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SectorId",
                table: "Companies",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CompanyId",
                table: "Contacts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainsToSkip_Domain",
                table: "DomainsToSkip",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_Name",
                table: "Sectors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Software_WebsiteId",
                table: "Software",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_WebsiteId",
                table: "Tools",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectCompanies_CompanyId",
                table: "UserProjectCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectCompanies_ProjectId_CompanyId",
                table: "UserProjectCompanies",
                columns: new[] { "ProjectId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId",
                table: "UserProjects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_CompanyId",
                table: "Websites",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_DateLastChecked",
                table: "Websites",
                column: "DateLastChecked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "DomainsToSkip");

            migrationBuilder.DropTable(
                name: "PostalCodes");

            migrationBuilder.DropTable(
                name: "Software");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "UserProjectCompanies");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Websites");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Sectors");
        }
    }
}
