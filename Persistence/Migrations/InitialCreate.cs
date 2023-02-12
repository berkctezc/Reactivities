using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Activities",
            table => new
            {
                Id = table.Column<Guid>("TEXT", nullable: false),
                Title = table.Column<string>("TEXT", nullable: false),
                Date = table.Column<DateTime>("TEXT", nullable: false),
                Description = table.Column<string>("TEXT", nullable: false),
                Category = table.Column<string>("TEXT", nullable: false),
                City = table.Column<string>("TEXT", nullable: false),
                Venue = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Activities", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Activities");
    }
}