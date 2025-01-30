using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicBin.Data.Migrations
{
    /// <inheritdoc />
    public partial class extractZipFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Comics");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Comics");

            migrationBuilder.DropColumn(
                name: "NeedsMetaData",
                table: "Comics");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Comics");

            migrationBuilder.RenameColumn(
                name: "UnableToOpen",
                table: "Comics",
                newName: "CBZFileId");

            migrationBuilder.CreateTable(
                name: "CBZFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    UnableToOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    NeedsMetaData = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasSubdirectories = table.Column<bool>(type: "INTEGER", nullable: false),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CBZFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comics_CBZFileId",
                table: "Comics",
                column: "CBZFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comics_CBZFiles_CBZFileId",
                table: "Comics",
                column: "CBZFileId",
                principalTable: "CBZFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comics_CBZFiles_CBZFileId",
                table: "Comics");

            migrationBuilder.DropTable(
                name: "CBZFiles");

            migrationBuilder.DropIndex(
                name: "IX_Comics_CBZFileId",
                table: "Comics");

            migrationBuilder.RenameColumn(
                name: "CBZFileId",
                table: "Comics",
                newName: "UnableToOpen");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Comics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Comics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsMetaData",
                table: "Comics",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Comics",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
