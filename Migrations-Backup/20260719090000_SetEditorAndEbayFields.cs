using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBSCardManager.Migrations
{
    public partial class SetEditorAndEbayFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(name: "StockImageUrl", table: "ChecklistItems", type: "nvarchar(1000)", maxLength: 1000, nullable: true, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500, oldNullable: true);
            migrationBuilder.AddColumn<string>(name: "Parallel", table: "ChecklistItems", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Position", table: "ChecklistItems", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PrintRun", table: "ChecklistItems", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "SerialNumber", table: "ChecklistItems", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "StockBackImageUrl", table: "ChecklistItems", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Variation", table: "ChecklistItems", type: "nvarchar(200)", maxLength: 200, nullable: true);

            migrationBuilder.AddColumn<bool>(name: "BestOfferEnabled", table: "Cards", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "EbayCategoryId", table: "Cards", type: "nvarchar(80)", maxLength: 80, nullable: true);
            migrationBuilder.AddColumn<string>(name: "EbayCondition", table: "Cards", type: "nvarchar(80)", maxLength: 80, nullable: true);
            migrationBuilder.AddColumn<string>(name: "EbayDescription", table: "Cards", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "EbaySku", table: "Cards", type: "nvarchar(80)", maxLength: 80, nullable: true);
            migrationBuilder.AddColumn<string>(name: "EbayTitle", table: "Cards", type: "nvarchar(160)", maxLength: 160, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ListingFormat", table: "Cards", type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "FixedPrice");
            migrationBuilder.AddColumn<decimal>(name: "PackageHeightIn", table: "Cards", type: "decimal(10,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "PackageLengthIn", table: "Cards", type: "decimal(10,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "PackageWeightOz", table: "Cards", type: "decimal(10,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "PackageWidthIn", table: "Cards", type: "decimal(10,2)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentPolicyName", table: "Cards", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ReturnPolicyName", table: "Cards", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ShippingPolicyName", table: "Cards", type: "nvarchar(150)", maxLength: 150, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var name in new[] { "Parallel", "Position", "PrintRun", "SerialNumber", "StockBackImageUrl", "Variation" })
                migrationBuilder.DropColumn(name: name, table: "ChecklistItems");
            migrationBuilder.AlterColumn<string>(name: "StockImageUrl", table: "ChecklistItems", type: "nvarchar(500)", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "nvarchar(1000)", oldMaxLength: 1000, oldNullable: true);
            foreach (var name in new[] { "BestOfferEnabled", "EbayCategoryId", "EbayCondition", "EbayDescription", "EbaySku", "EbayTitle", "ListingFormat", "PackageHeightIn", "PackageLengthIn", "PackageWeightOz", "PackageWidthIn", "PaymentPolicyName", "ReturnPolicyName", "ShippingPolicyName" })
                migrationBuilder.DropColumn(name: name, table: "Cards");
        }
    }
}
