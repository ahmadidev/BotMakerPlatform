using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BotMakerPlatform.Web.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BotInstanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    BotUsername = table.Column<string>(nullable: true),
                    BotUniqueName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    WebhookSecret = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotInstanceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    BotInstanceRecordId = table.Column<int>(nullable: false),
                    BotInstanceRecordId1 = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.BotInstanceRecordId);
                    table.ForeignKey(
                        name: "FK_Settings_BotInstanceRecords_BotInstanceRecordId1",
                        column: x => x.BotInstanceRecordId1,
                        principalTable: "BotInstanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreAdminRecords",
                columns: table => new
                {
                    ChatId = table.Column<long>(nullable: false),
                    BotInstanceRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAdminRecords", x => new { x.BotInstanceRecordId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_StoreAdminRecords_BotInstanceRecords_BotInstanceRecordId",
                        column: x => x.BotInstanceRecordId,
                        principalTable: "BotInstanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreProductRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Price = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    BotInstanceRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreProductRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreProductRecords_BotInstanceRecords_BotInstanceRecordId",
                        column: x => x.BotInstanceRecordId,
                        principalTable: "BotInstanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    ChatId = table.Column<long>(nullable: false),
                    BotInstanceRecordId = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => new { x.BotInstanceRecordId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_Subscribers_BotInstanceRecords_BotInstanceRecordId",
                        column: x => x.BotInstanceRecordId,
                        principalTable: "BotInstanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageFileRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageFileId = table.Column<string>(nullable: true),
                    StoreProductRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFileRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageFileRecords_StoreProductRecords_StoreProductRecordId",
                        column: x => x.StoreProductRecordId,
                        principalTable: "StoreProductRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageFileRecords_StoreProductRecordId",
                table: "ImageFileRecords",
                column: "StoreProductRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_BotInstanceRecordId1",
                table: "Settings",
                column: "BotInstanceRecordId1");

            migrationBuilder.CreateIndex(
                name: "IX_StoreProductRecords_BotInstanceRecordId",
                table: "StoreProductRecords",
                column: "BotInstanceRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageFileRecords");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "StoreAdminRecords");

            migrationBuilder.DropTable(
                name: "Subscribers");

            migrationBuilder.DropTable(
                name: "StoreProductRecords");

            migrationBuilder.DropTable(
                name: "BotInstanceRecords");
        }
    }
}
