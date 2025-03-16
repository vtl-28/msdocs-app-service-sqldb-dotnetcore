using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssistentNameProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                                // Step 1: Add the new column with EF Core (this is outside any SQL transaction)
                migrationBuilder.AddColumn<string>(
                    name: "AssistentNameBackup",
                    table: "Todo",
                    type: "text",
                    nullable: true);
                    
                // Steps 2-4: Verification, data transfer, and column drop in a single DO block
                migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    updated_rows INTEGER;
                    incomplete_transfers INTEGER;
                BEGIN
                    -- Step 2: Verify the column was created successfully
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'Todo' 
                        AND column_name = 'AssistentNameBackup'
                    ) THEN
                        RAISE EXCEPTION 'New column was not created successfully';
                    END IF;
                    
                    -- Step 3: Transfer the data with verification
                    WITH updated AS (
                        UPDATE ""Todo"" 
                        SET ""AssistentNameBackup"" = ""AssistentName""
                        WHERE ""AssistentName"" IS NOT NULL
                        RETURNING *
                    )
                    SELECT COUNT(*) INTO updated_rows FROM updated;
                    
                    -- Verify data transfer
                    SELECT COUNT(*) INTO incomplete_transfers
                    FROM ""Todo""
                    WHERE ""AssistentName"" IS NOT NULL 
                    AND (""AssistentNameBackup"" IS NULL OR ""AssistentNameBackup"" != ""AssistentName"");
                    
                    IF incomplete_transfers > 0 THEN
                        RAISE EXCEPTION 'Data transfer was incomplete or incorrect. % records not transferred correctly', incomplete_transfers;
                    END IF;
                    
                    RAISE NOTICE 'Successfully transferred data for % records', updated_rows;
                    
                    -- Step 4: Only if previous steps succeeded, drop the old column
                    ALTER TABLE ""Todo"" DROP COLUMN ""AssistentName"";
                    RAISE NOTICE 'Original column dropped successfully';
                END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssistentName",
                table: "Todo",
                type: "text",
                nullable: true);
        }
    }
}
