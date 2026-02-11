using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSupp.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchTextToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. SearchText kolonunu ekle
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Customers",
                type: "citext",
                maxLength: 200,
                nullable: true);

            // 2. Mevcut kayıtlar için SearchText'i doldur
            migrationBuilder.Sql(@"
                UPDATE ""Customers"" 
                SET ""SearchText"" = LOWER(
                    REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                        ""Name"", 
                        'İ', 'i'), 
                        'I', 'i'), 
                        'ı', 'i'), 
                        'Ö', 'o'), 
                        'ö', 'o'), 
                        'Ü', 'u'), 
                        'ü', 'u'), 
                        'Ş', 's'), 
                        'ş', 's'), 
                        'Ğ', 'g')
                );
            ");

            // 3. Index ekle (performans için - opsiyonel)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_customers_searchtext 
                ON ""Customers"" (""SearchText"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Customers");
        }
    }
}