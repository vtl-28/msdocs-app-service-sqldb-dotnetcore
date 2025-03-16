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
                migrationBuilder.Sql("BEGIN TRANSACTION");
                
                try
                {
                    // Step 1: Add the new column
                    migrationBuilder.AddColumn<string>(
                        name: "AssitentNameBackup",
                        table: "Todo",
                        type: "text",
                        nullable: true);

                    // Step 2: Verify the column was created successfully
                    migrationBuilder.Sql(@"
                        IF NOT EXISTS (
                            SELECT 1 
                            FROM sys.columns 
                            WHERE object_id = OBJECT_ID('Todo') 
                            AND name = 'AssitentNameBackup'
                        )
                        BEGIN
                            THROW 51000, 'New column was not created successfully', 1;
                        END");

                    // Step 3: Transfer the data with verification
                    migrationBuilder.Sql(@"
                        DECLARE @RowCount INT;
                        
                        -- Copy the data
                        UPDATE Todo 
                        SET AssitentNameBackup = AssitentName
                        WHERE AssitentName IS NOT NULL;
                        
                        -- Store the number of updated rows
                        SET @RowCount = @@ROWCOUNT;
                        
                        -- Verify data transfer
                        IF (
                            SELECT COUNT(*) 
                            FROM Todo 
                            WHERE AssitentName IS NOT NULL 
                            AND (AssitentNameBackup IS NULL OR AssitentNameBackup != AssitentName)
                        ) > 0
                        BEGIN
                            THROW 51000, 'Data transfer was incomplete or incorrect', 1;
                        END");

                    // Step 4: Only if previous steps succeeded, drop the old column
                    migrationBuilder.Sql(@"
                        IF EXISTS (
                            SELECT 1 
                            FROM sys.columns 
                            WHERE object_id = OBJECT_ID('Todo') 
                            AND name = 'AssitentNameBackup'
                        )
                        BEGIN
                            ALTER TABLE Todo DROP COLUMN AssitentName;
                        END");

                    // Commit the transaction if all steps completed successfully
                    migrationBuilder.Sql("COMMIT TRANSACTION");
                }
                catch
                {
                    // If any step fails, roll back all changes
                    migrationBuilder.Sql("IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION");
                    throw; // Re-throw the exception to alert EF Core that the migration failed
                }
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
