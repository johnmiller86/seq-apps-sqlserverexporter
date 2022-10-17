namespace Seq.Apps.SQLServerExporter
{
    internal class Scripts
    {
        internal static string CreateColumns = @"
            DECLARE @SQL VARCHAR(MAX) = 'ALTER TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' ADD ' + @Columns;
            EXEC(@SQL);";

        internal static string CreateSchema = @"
            IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @SchemaName)
            BEGIN
	            DECLARE @SQL VARCHAR(MAX) = 'CREATE SCHEMA ' + QUOTENAME(@SchemaName) + ' AUTHORIZATION [dbo]';
	            EXEC(@SQL);
            END";

        internal static string CreateTable = @"
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName)
            BEGIN
	            DECLARE @SQL VARCHAR(MAX) = '
	            CREATE TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' (
		            [EventLogId] BIGINT PRIMARY KEY IDENTITY (1, 1),
		            [SeqEventId] NVARCHAR(50) NOT NULL,
		            [SeqEventIngestionTimestamp] NVARCHAR(30) NOT NULL,
                    [SeqEventLocalTimestamp] NVARCHAR(30) NOT NULL,
                    [SeqEventLevel] NVARCHAR(15) NOT NULL,
                    [SeqEventMessage] NVARCHAR(MAX) NOT NULL
	            )';
	            EXEC(@SQL);
            END
            ELSE
            BEGIN
                DECLARE @RenameSQL VARCHAR(MAX) = '
	            -- Try and update to the new PK name
	            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''' + @SchemaName + ''' AND TABLE_NAME = ''' + @TableName + ''' AND COLUMN_NAME = ''EventLogId'') AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''' + @SchemaName + ''' AND TABLE_NAME = ''' + @TableName + ''' AND COLUMN_NAME = ''Id'')
	            BEGIN
		            -- Update Id column to EventLogId
		            EXEC sp_rename ''' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + '.Id'', ''EventLogId'', ''COLUMN''
	            END';
                EXEC(@RenameSQL);
            END";

        internal static string InsertEvent = @"
            DECLARE @params NVARCHAR(100) = '@S NVARCHAR(128), @T NVARCHAR(128), @C NVARCHAR(MAX), @V NVARCHAR(MAX)';
            DECLARE @S NVARCHAR(128) = @SchemaName;
            DECLARE @T NVARCHAR(128)= @TableName;
            DECLARE @C NVARCHAR(MAX) = @Columns;
            DECLARE @V NVARCHAR(MAX) = @Values;
            DECLARE @SQL NVARCHAR(MAX) = 'INSERT INTO ' + QUOTENAME(@S) + '.' + QUOTENAME(@T) + '(' + @C + ') VALUES(' + @V + ')';
            EXEC sp_executesql @SQL, @params, @S = @SchemaName, @T = @TableName, @C = @Columns, @V = @Values;";

        internal static string SelectAllColumns = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName;";
    }
}