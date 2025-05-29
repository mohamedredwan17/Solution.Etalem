using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etalem.Migrations
{
    /// <inheritdoc />
    public partial class StuIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_AspNetUsers_StudentId1",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_StudentId1",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "QuizAttempts");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "QuizAttempts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentId",
                table: "QuizAttempts",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_AspNetUsers_StudentId",
                table: "QuizAttempts",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_AspNetUsers_StudentId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_StudentId",
                table: "QuizAttempts");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "StudentId1",
                table: "QuizAttempts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentId1",
                table: "QuizAttempts",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_AspNetUsers_StudentId1",
                table: "QuizAttempts",
                column: "StudentId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
