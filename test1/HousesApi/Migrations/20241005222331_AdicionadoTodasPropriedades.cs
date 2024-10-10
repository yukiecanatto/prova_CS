using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HousesApi.Migrations
{
    /// <inheritdoc />
    public partial class AdicionadoTodasPropriedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Words",
                table: "Houses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Houses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Houses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "CoatOfArms",
                table: "Houses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "AncestralWeapons",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CadetBranches",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentLord",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiedOut",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Founded",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Founder",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Heir",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverLord",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Seats",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwornMembers",
                table: "Houses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Titles",
                table: "Houses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AncestralWeapons",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "CadetBranches",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "CurrentLord",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "DiedOut",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Founded",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Founder",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Heir",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "OverLord",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Seats",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "SwornMembers",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Titles",
                table: "Houses");

            migrationBuilder.AlterColumn<string>(
                name: "Words",
                table: "Houses",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Houses",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Houses",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoatOfArms",
                table: "Houses",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
