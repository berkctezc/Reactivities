﻿#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations;

public partial class IdentityAdded : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            "Venue",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            "Title",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            "Description",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<DateTime>(
            "Date",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            "City",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            "Category",
            "Activities",
            "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.CreateTable(
            "AspNetRoles",
            table => new
            {
                Id = table.Column<string>("TEXT", nullable: false),
                Name = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetUsers",
            table => new
            {
                Id = table.Column<string>("TEXT", nullable: false),
                DisplayName = table.Column<string>("TEXT", nullable: false),
                Bio = table.Column<string>("TEXT", nullable: false),
                UserName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                Email = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>("INTEGER", nullable: false),
                PasswordHash = table.Column<string>("TEXT", nullable: true),
                SecurityStamp = table.Column<string>("TEXT", nullable: true),
                ConcurrencyStamp = table.Column<string>("TEXT", nullable: true),
                PhoneNumber = table.Column<string>("TEXT", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>("INTEGER", nullable: false),
                TwoFactorEnabled = table.Column<bool>("INTEGER", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>("TEXT", nullable: true),
                LockoutEnabled = table.Column<bool>("INTEGER", nullable: false),
                AccessFailedCount = table.Column<int>("INTEGER", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetRoleClaims",
            table => new
            {
                Id = table.Column<int>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RoleId = table.Column<string>("TEXT", nullable: false),
                ClaimType = table.Column<string>("TEXT", nullable: true),
                ClaimValue = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserClaims",
            table => new
            {
                Id = table.Column<int>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>("TEXT", nullable: false),
                ClaimType = table.Column<string>("TEXT", nullable: true),
                ClaimValue = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetUserClaims_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserLogins",
            table => new
            {
                LoginProvider = table.Column<string>("TEXT", nullable: false),
                ProviderKey = table.Column<string>("TEXT", nullable: false),
                ProviderDisplayName = table.Column<string>("TEXT", nullable: true),
                UserId = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new {x.LoginProvider, x.ProviderKey});
                table.ForeignKey(
                    "FK_AspNetUserLogins_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserRoles",
            table => new
            {
                UserId = table.Column<string>("TEXT", nullable: false),
                RoleId = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new {x.UserId, x.RoleId});
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserTokens",
            table => new
            {
                UserId = table.Column<string>("TEXT", nullable: false),
                LoginProvider = table.Column<string>("TEXT", nullable: false),
                Name = table.Column<string>("TEXT", nullable: false),
                Value = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new {x.UserId, x.LoginProvider, x.Name});
                table.ForeignKey(
                    "FK_AspNetUserTokens_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_AspNetRoleClaims_RoleId",
            "AspNetRoleClaims",
            "RoleId");

        migrationBuilder.CreateIndex(
            "RoleNameIndex",
            "AspNetRoles",
            "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_AspNetUserClaims_UserId",
            "AspNetUserClaims",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserLogins_UserId",
            "AspNetUserLogins",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserRoles_RoleId",
            "AspNetUserRoles",
            "RoleId");

        migrationBuilder.CreateIndex(
            "EmailIndex",
            "AspNetUsers",
            "NormalizedEmail");

        migrationBuilder.CreateIndex(
            "UserNameIndex",
            "AspNetUsers",
            "NormalizedUserName",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "AspNetRoleClaims");

        migrationBuilder.DropTable(
            "AspNetUserClaims");

        migrationBuilder.DropTable(
            "AspNetUserLogins");

        migrationBuilder.DropTable(
            "AspNetUserRoles");

        migrationBuilder.DropTable(
            "AspNetUserTokens");

        migrationBuilder.DropTable(
            "AspNetRoles");

        migrationBuilder.DropTable(
            "AspNetUsers");

        migrationBuilder.AlterColumn<string>(
            "Venue",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            "Title",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            "Description",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            "Date",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            "City",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            "Category",
            "Activities",
            "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);
    }
}