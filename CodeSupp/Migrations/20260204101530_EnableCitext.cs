using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSupp.Migrations
{
    /// <inheritdoc />
    public partial class EnableCitext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "citext",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Sales",
                type: "citext",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalReference",
                table: "Sales",
                type: "citext",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Products",
                type: "citext",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "citext",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Products",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Products",
                type: "citext",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "citext",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductPurchaseHistories",
                type: "citext",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Payments",
                type: "citext",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "citext",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "citext",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "citext",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "InstagramHandle",
                table: "Customers",
                type: "citext",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "citext",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "citext",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "citext",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AspNetUserTokens",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "AspNetUsers",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "DashboardSettings",
                table: "AspNetUsers",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderDisplayName",
                table: "AspNetUserLogins",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                table: "AspNetUserClaims",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "AspNetUserClaims",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AspNetRoles",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetRoles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                table: "AspNetRoleClaims",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "AspNetRoleClaims",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "tr_TR_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:tr_TR_ci", "tr-TR,tr-TR,icu,False")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Sales",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalReference",
                table: "Sales",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Products",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Products",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductPurchaseHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "InstagramHandle",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AspNetUserTokens",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DashboardSettings",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderDisplayName",
                table: "AspNetUserLogins",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                table: "AspNetUserClaims",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "AspNetUserClaims",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AspNetRoles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetRoles",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                table: "AspNetRoleClaims",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "AspNetRoleClaims",
                type: "text",
                nullable: true,
                collation: "tr_TR_ci",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);
        }
    }
}
