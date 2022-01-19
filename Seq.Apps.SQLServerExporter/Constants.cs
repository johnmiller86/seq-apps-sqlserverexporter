namespace Seq.Apps.SQLServerExporter
{
    internal class Constants
    {
        internal struct AppFields
        {
            internal const string ConnectionStringDisplayName = "Connection String";
            internal const string ConnectionStringHelpText = "The connection string for the database in which to export Seq logs (ex. data source=SERVER;initial catalog=DATABASE;trusted_connection=true) (ex. data source=SERVER;initial catalog=DATABASE;User id=USERNAME;Password=PASSWORD)";
            internal const string SchemaNameDisplayName = "Schema Name";
            internal const string SchemaNameHelpText = "The desired schema for the Seq log export table to be created in ([dbo] if not supplied)";
            internal const string TableNameDisplayName = "Table Name";
            internal const string TableNameHelpText = "The desired table name for the Seq log export table ([EventLog] if not supplied)";
        }

        internal struct AppMetadata
        {
            internal const string AppName = "SQL Server Exporter";
            internal const string AppDescription = "Exports Seq events to MS SQL Server";
        }

        internal struct InitialColumns
        {
            internal const string SeqEventId = "SeqEventId";
            internal const string SeqEventIngestionTimestamp = "SeqEventIngestionTimestamp";
            internal const string SeqEventLocalTimestamp = "SeqEventLocalTimestamp";
            internal const string SeqEventLevel = "SeqEventLevel";
            internal const string SeqEventMessage = "SeqEventMessage";
        }

        internal struct DatabaseConstants
        {
            internal const string DefaultSchemaName = "dbo";
            internal const string DefaultTableName = "EventLog";
        }

        internal struct LogContextConstants
        {
            internal const string Columns = "Columns";
            internal const string ConnectionString = "ConnectionString";
            internal const string EventId = "EventId";
            internal const string PropertyName = "PropertyName";
            internal const string SchemaName = "SchemaName";
            internal const string TableName = "TableName";
        }

        internal struct LogMessageConstants
        {
            internal const string ColumnCreationError = "Column Creation Error";
            internal const string ConnectionError = "Connection Error";
            internal const string ErrorFormat = "{0}: {1}";
            internal const string EventInsertionError = "Event Insertion Error";
            internal const string InsertStatementGenerationErrror = "Insert Statement Generation Error";
            internal const string SchemaCreationError = "Schema Creation Error";
            internal const string SkippedAddingColumnWarning = "Skipped adding property due to conflict with pre-defined column name";
            internal const string TableCreationError = "Table Creation Error";
            internal const string TableMetadataRetrievalError = "Table Metadata Retrieval Error";
        }

        internal struct SeqAppExceptions
        {
            internal const string ColumnCreationException = "An error occurred while adding columns to the log table";
            internal const string ConnectionException = "Unable to connect to the database, please check Connection String parameter";
            internal const string EventInsertionException = "An error occurred while inserting the event into the log table";
            internal const string InsertStatementGenerationException = "An error occurred while generating the insert statement for the event";
            internal const string SchemaCreationException = "Unable to create schema, please check Schema Name parameter";
            internal const string TableCreationException = "Unable to create table{0}";
            internal const string TableMetadataRetrievalException = "Unable to retrieve log table metadata";
        }

        internal struct SqlParameters
        {
            internal const string ColumnsParameter = "@Columns";
            internal const string SchemaNameParameter = "@SchemaName";
            internal const string TableNameParameter = "@TableName";
            internal const string ValuesParameter = "@Values";
        }
    }
}