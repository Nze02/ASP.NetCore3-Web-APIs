using Microsoft.EntityFrameworkCore.Migrations;

namespace ASP.NetCore3_Web_APIs.Migrations
{
    public partial class AddedRolesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "fb54d6f5-788a-4075-bdcb-7d1b88184ea2", "ff4568db-edb2-41f5-9ea9-b2a5cf6e67a2", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d096f512-b165-43d5-8ecc-0f64beb1812b", "414f5b67-7a8e-4dbc-a429-54bd1aa7d92b", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d096f512-b165-43d5-8ecc-0f64beb1812b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fb54d6f5-788a-4075-bdcb-7d1b88184ea2");
        }
    }
}
