using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesMadeEasy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserStreamingServiceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StreamingService_StreamingServicesId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StreamingService_StreamingServicesId",
                table: "AspNetUsers",
                column: "StreamingServicesId",
                principalTable: "StreamingService",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StreamingService_StreamingServicesId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StreamingService_StreamingServicesId",
                table: "AspNetUsers",
                column: "StreamingServicesId",
                principalTable: "StreamingService",
                principalColumn: "Id");
        }
    }
}
