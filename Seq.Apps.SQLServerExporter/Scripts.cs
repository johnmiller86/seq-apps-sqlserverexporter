namespace Seq.Apps.SQLServerExporter
{
    internal class Scripts
    {
        internal static string CreateColumns = @"
            DECLARE @SQL VARCHAR(MAX) = 'ALTER TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' ADD ' + @Columns;
            EXEC(@SQL);";

        internal static string CreateSchema = @"
            IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = QUOTENAME(@SchemaName))
            BEGIN
	            DECLARE @SQL VARCHAR(MAX) = 'CREATE SCHEMA ' + QUOTENAME(@SchemaName) + ' AUTHORIZATION [dbo]';
	            EXEC(@SQL);
            END";

        internal static string CreateTable = @"
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = QUOTENAME(@SchemaName) AND TABLE_NAME = QUOTENAME(@TableName))
            BEGIN
	            DECLARE @SQL VARCHAR(MAX) = '
	            CREATE TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' (
		            [Id] BIGINT PRIMARY KEY IDENTITY (1, 1),
		            [SeqEventId] NVARCHAR(50) NOT NULL,
		            [SeqEventIngestionTimestamp] NVARCHAR(30) NOT NULL,
                    [SeqEventLocalTimestamp] NVARCHAR(30) NOT NULL,
                    [SeqEventLevel] NVARCHAR(15) NOT NULL,
                    [SeqEventMessage] NVARCHAR(MAX) NOT NULL
	            )';
				EXEC(@SQL);
            END";

        internal static string InsertEvent = @"
            DECLARE @SQL VARCHAR(MAX) = 'INSERT INTO ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + '(' + @Columns + ') VALUES(' + @Values + ')';
            EXEC(@SQL)";

        internal static string SelectAllColumns = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName;";
    }
}