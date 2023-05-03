using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSales.Migrations
{
    /// <inheritdoc />
    public partial class UnifyContentLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION pg_temp.converter(text)
                RETURNS text
                AS
                $$
                DECLARE
                   fValue ALIAS FOR $1;
                BEGIN
                   IF array_length(regexp_matches(fValue, 'ru', 'i'),1) > 0 THEN
   	                return 'ru-RU';
                   ELSEIF array_length(regexp_match(fValue, 'en', 'i'), 1) > 0 THEN
   	                return 'en-US';
                   END IF;
                   return fValue;
                END;
                $$
                LANGUAGE plpgsql 
                   STABLE;
                UPDATE content SET language = pg_temp.converter(language)
            ");
        }
#pragma warning disable
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
#pragma warning restore
    }
}
