#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations;

public partial class ActivityAttendeeAndCancelled : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
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

		migrationBuilder.AddColumn<bool>(
			"IsCancelled",
			"Activities",
			"INTEGER",
			nullable: false,
			defaultValue: false);

		migrationBuilder.CreateTable(
			"ActivityAttendees",
			table => new
			{
				AppUserId = table.Column<string>("TEXT", nullable: false),
				ActivityId = table.Column<Guid>("TEXT", nullable: false),
				IsHost = table.Column<bool>("INTEGER", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ActivityAttendees", x => new {x.AppUserId, x.ActivityId});
				table.ForeignKey(
					"FK_ActivityAttendees_Activities_ActivityId",
					x => x.ActivityId,
					"Activities",
					"Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					"FK_ActivityAttendees_AspNetUsers_AppUserId",
					x => x.AppUserId,
					"AspNetUsers",
					"Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			"IX_ActivityAttendees_ActivityId",
			"ActivityAttendees",
			"ActivityId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			"ActivityAttendees");

		migrationBuilder.DropColumn(
			"IsCancelled",
			"Activities");

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
	}
}