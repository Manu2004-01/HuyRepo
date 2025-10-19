using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EatIT.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ini : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    TagImg = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    UserImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "varchar(250)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    UserAddress = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    GoogleId = table.Column<string>(type: "varchar(100)", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "varchar(500)", nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserLatitude = table.Column<double>(type: "float", nullable: true),
                    UserLongitude = table.Column<double>(type: "float", nullable: true),
                    LastLocationUpdate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    ResName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    RestaurantImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResAddress = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    OpeningHours = table.Column<string>(type: "nvarchar(150)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Restaurants_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Restaurants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResId = table.Column<int>(type: "int", nullable: false),
                    DishName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    DishDescription = table.Column<string>(type: "text", nullable: false),
                    DishPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DishImage = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVegan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dishes_Restaurants_ResId",
                        column: x => x.ResId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    Star = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratings_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ratings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DishId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RestaurantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Favorites_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "TagImg", "TagName" },
                values: new object[,]
                {
                    { 1, "http", "Cơm tấm" },
                    { 2, "http", "Món nước" },
                    { 3, "http", "Trà sữa" },
                    { 4, "http", "Thức ăn nhanh" },
                    { 5, "http", "Bánh mì" },
                    { 6, "http", "Chè & Tráng miệng" },
                    { 7, "http", "Cà phê & Nước uống" },
                    { 8, "http", "Hải sản" },
                    { 9, "http", "Nướng & BBQ" },
                    { 10, "http", "Lẩu" },
                    { 11, "http", "Quán nhậu/Bia hơi" },
                    { 12, "http", "Ăn vặt/Vỉa hè" },
                    { 13, "http", "Cơm văn phòng" },
                    { 14, "http", "Chay" },
                    { 15, "http", "Ẩm thực miền Bắc" },
                    { 16, "http", "Ẩm thực miền Trung" },
                    { 17, "http", "Ẩm thực miền Nam" },
                    { 18, "http", "Ẩm thực Tây Âu" },
                    { 19, "http", "Ẩm thực Nhật/Hàn/Trung" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Người dùng" },
                    { 3, "Nhà hàng" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreateAt", "Email", "GoogleId", "IsActive", "LastLocationUpdate", "Password", "PhoneNumber", "ResetPasswordToken", "ResetPasswordTokenExpiry", "RoleId", "UpdateAt", "UserAddress", "UserImg", "UserLatitude", "UserLongitude", "UserName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@eatit.com", null, true, null, "123456", "0382727683", null, null, 1, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thành phố Hồ Chí Minh", "http", null, null, "Admin Huy Che" },
                    { 2, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "user@eatit.com", null, true, null, "654321", "0912345678", null, null, 2, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hà Nội", "http", null, null, "Huy Che" },
                    { 3, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "restaurant@eatit.com", null, true, null, "111111", "0912345678", null, null, 3, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đà Nẵng", "http", null, null, "Nhà hàng đồ ăn" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_ResId",
                table: "Dishes",
                column: "ResId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_DishId",
                table: "Favorites",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_RestaurantId",
                table: "Favorites",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId",
                table: "Favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RestaurantId",
                table: "Ratings",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId",
                table: "Ratings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_TagId",
                table: "Restaurants",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_UserId",
                table: "Restaurants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}
