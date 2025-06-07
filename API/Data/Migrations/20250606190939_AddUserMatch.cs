using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhotoApprovalStats",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedPhotos = table.Column<int>(type: "int", nullable: false),
                    UnapprovedPhotos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "UserMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceUserId = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMatches_AspNetUsers_SourceUserId",
                        column: x => x.SourceUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMatches_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsersWithoutMainPhoto",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMatches_SourceUserId",
                table: "UserMatches",
                column: "SourceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMatches_TargetUserId",
                table: "UserMatches",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoApprovalStats");

            migrationBuilder.DropTable(
                name: "UserMatches");

            migrationBuilder.DropTable(
                name: "UsersWithoutMainPhoto");
        }
    }
}
