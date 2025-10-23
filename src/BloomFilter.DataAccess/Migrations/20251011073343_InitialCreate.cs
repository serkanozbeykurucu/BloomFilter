using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BloomFilter.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloomFilterDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FilterName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FilterSize = table.Column<int>(type: "integer", nullable: false),
                    HashFunctionCount = table.Column<int>(type: "integer", nullable: false),
                    BitArray = table.Column<byte[]>(type: "bytea", nullable: false),
                    ElementCount = table.Column<int>(type: "integer", nullable: false),
                    ExpectedFalsePositiveRate = table.Column<double>(type: "double precision", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloomFilterDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    LastReportedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmailAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    LastReportedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReporterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReporterEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReportType = table.Column<int>(type: "integer", nullable: false),
                    ReportedValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReporterIpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SuspiciousDomainId = table.Column<int>(type: "integer", nullable: true),
                    SuspiciousEmailId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_SuspiciousDomains_SuspiciousDomainId",
                        column: x => x.SuspiciousDomainId,
                        principalTable: "SuspiciousDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserReports_SuspiciousEmails_SuspiciousEmailId",
                        column: x => x.SuspiciousEmailId,
                        principalTable: "SuspiciousEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloomFilterDatas_FilterName",
                table: "BloomFilterDatas",
                column: "FilterName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousDomains_DomainName",
                table: "SuspiciousDomains",
                column: "DomainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousEmails_DomainName",
                table: "SuspiciousEmails",
                column: "DomainName");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousEmails_EmailAddress",
                table: "SuspiciousEmails",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_CreatedDate",
                table: "UserReports",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedValue",
                table: "UserReports",
                column: "ReportedValue");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportType",
                table: "UserReports",
                column: "ReportType");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_Status",
                table: "UserReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_SuspiciousDomainId",
                table: "UserReports",
                column: "SuspiciousDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_SuspiciousEmailId",
                table: "UserReports",
                column: "SuspiciousEmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloomFilterDatas");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "SuspiciousDomains");

            migrationBuilder.DropTable(
                name: "SuspiciousEmails");
        }
    }
}
