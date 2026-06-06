using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddSharingAndDocumentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_AssignedToUserId",
                table: "TaskItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentSharings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    SharedWithUserId = table.Column<int>(type: "int", nullable: false),
                    SharedByUserId = table.Column<int>(type: "int", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSharings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSharings_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentSharings_Users_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentSharings_Users_SharedWithUserId",
                        column: x => x.SharedWithUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSharings_DocumentId",
                table: "DocumentSharings",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSharings_SharedByUserId",
                table: "DocumentSharings",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSharings_SharedWithUserId",
                table: "DocumentSharings",
                column: "SharedWithUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_AssignedToUserId",
                table: "TaskItems",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_AssignedToUserId",
                table: "TaskItems");

            migrationBuilder.DropTable(
                name: "DocumentSharings");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Documents");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_AssignedToUserId",
                table: "TaskItems",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
