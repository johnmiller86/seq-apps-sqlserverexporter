using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Seq.Apps.SQLServerExporter
{
    [SeqApp(Constants.AppMetadata.AppName, Description = Constants.AppMetadata.AppDescription)]
    public class SQLServerExporter : SeqApp, ISubscribeTo<LogEventData>
    {
        #region App Settings
        [SeqAppSetting(DisplayName = Constants.AppFields.ConnectionStringDisplayName, HelpText = Constants.AppFields.ConnectionStringHelpText, InputType = SettingInputType.Password, IsOptional = false)]
        public string ConnectionString { get; set; }
        [SeqAppSetting(DisplayName = Constants.AppFields.SchemaNameDisplayName, HelpText = Constants.AppFields.SchemaNameHelpText, InputType = SettingInputType.Text, IsOptional = true)]
        public string SchemaName { get; set; }
        [SeqAppSetting(DisplayName = Constants.AppFields.TableNameDisplayName, HelpText = Constants.AppFields.TableNameHelpText, InputType = SettingInputType.Text, IsOptional = true)]
        public string TableName { get; set; }
        #endregion

        #region Overrides/Interface Implementations
        protected override void OnAttached()
        {
            ValidateConnection();
            PerformInitialSetup();
        }

        public void On(Event<LogEventData> evt)
        {
            // Process the event
            ProcessEvent(evt);
        }
        #endregion

        #region Private Methods
        private void ValidateConnection()
        {
            try
            {
                // Initializing Connection
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    // Opening Connection
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                Log.ForContext(Constants.LogContextConstants.ConnectionString, ConnectionString)
                    .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.ConnectionError, ex.ToString()));
                throw new SeqAppException(Constants.SeqAppExceptions.ConnectionException);
            }
        }

        private void PerformInitialSetup()
        {
            SetDefaults();
            CreateSchema();
            CreateTable();
        }

        private void SetDefaults()
        {
            SchemaName = !string.IsNullOrEmpty(SchemaName) ? SchemaName : Constants.DatabaseConstants.DefaultSchemaName;
            TableName = !string.IsNullOrEmpty(TableName) ? TableName : Constants.DatabaseConstants.DefaultTableName;
        }

        private void CreateSchema()
        {
            // Not Necessary to Create Default dbo Schema
            if (!SchemaName.Equals(Constants.DatabaseConstants.DefaultSchemaName))
            {
                try
                {
                    // Initializing Connection
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        // Opening Connection
                        conn.Open();

                        // Initializing Command
                        using (var cmd = new SqlCommand(Scripts.CreateSchema, conn))
                        {
                            // Adding Parameters
                            cmd.Parameters.AddWithValue(Constants.SqlParameters.SchemaNameParameter, SchemaName);

                            // Executing Query
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ForContext(Constants.LogContextConstants.SchemaName, SchemaName)
                    .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.SchemaCreationError, ex.ToString()));
                    throw new SeqAppException(Constants.SeqAppExceptions.SchemaCreationException);
                }
            }
        }

        private void CreateTable()
        {
            try
            {
                // Initializing Connection
                using (var conn = new SqlConnection(ConnectionString))
                {
                    // Opening Connection
                    conn.Open();

                    // Initializing Command
                    using (var cmd = new SqlCommand(Scripts.CreateTable, conn))
                    {
                        // Adding Parameters
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.SchemaNameParameter, SchemaName);
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.TableNameParameter, TableName);

                        // Executing Query
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ForContext(Constants.LogContextConstants.SchemaName, SchemaName)
                    .ForContext(Constants.LogContextConstants.TableName, TableName)
                    .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.TableCreationError, ex.ToString()));
                throw new SeqAppException(string.Format(Constants.SeqAppExceptions.TableCreationException, !TableName.Equals(Constants.DatabaseConstants.DefaultTableName) ? ", please check Table Name parameter" : string.Empty));
            }
        }

        private List<string> GetAllTabCols()
        {
            try
            {
                var colList = new List<string>();

                // Initializing Connection
                using (var conn = new SqlConnection(ConnectionString))
                {
                    // Opening Connection
                    conn.Open();

                    // Initializing Command
                    using (var cmd = new SqlCommand(Scripts.SelectAllColumns, conn))
                    {
                        // Adding Parameters
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.SchemaNameParameter, SchemaName);
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.TableNameParameter, TableName);

                        // Executing Query
                        using (var rdr = cmd.ExecuteReader())
                        {
                            // Processing Results
                            while (rdr.Read())
                            {
                                colList.Add(rdr.GetString(0));
                            }
                        }
                    }
                }
                return colList;
            }
            catch (Exception ex)
            {
                Log.ForContext(Constants.LogContextConstants.SchemaName, SchemaName)
                    .ForContext(Constants.LogContextConstants.TableName, TableName)
                    .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.TableMetadataRetrievalError, ex.ToString()));
                throw new SeqAppException(Constants.SeqAppExceptions.TableMetadataRetrievalException);
            }
        }

        private void ProcessEvent(Event<LogEventData> evt)
        {
            // Generate the columns and values
            var colVals = GenerateColumnsAndValues(evt);

            // Add any necessary columns
            AddColumns(colVals.Keys);

            // Insert the event
            InsertEvent(colVals);
        }

        private void AddColumns(IEnumerable<string> cols)
        {
            var colsToAdd = cols.Where(col => !GetAllTabCols().Contains(col, StringComparer.CurrentCultureIgnoreCase));

            // Not necessary to add columns if event contains no new properties
            if (colsToAdd.Any())
            {
                try
                {
                    // Initializing Connection
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        // Opening Connection
                        conn.Open();

                        // Initializing Command
                        using (var cmd = new SqlCommand(Scripts.CreateColumns, conn))
                        {
                            // Adding Parameters
                            cmd.Parameters.AddWithValue(Constants.SqlParameters.SchemaNameParameter, SchemaName);
                            cmd.Parameters.AddWithValue(Constants.SqlParameters.TableNameParameter, TableName);
                            cmd.Parameters.AddWithValue(Constants.SqlParameters.ColumnsParameter, colsToAdd.Select(col => $"{col.FormatColumnWithBrackets()} NVARCHAR(MAX)").JoinEnumerableWithCommas());  // TODO - May make parameters for column size growth so not everything is MAX

                            // Executing Query
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ForContext(Constants.LogContextConstants.SchemaName, SchemaName)
                       .ForContext(Constants.LogContextConstants.TableName, TableName)
                       .ForContext(Constants.LogContextConstants.Columns, colsToAdd.JoinEnumerableWithCommas())
                       .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.ColumnCreationError, ex.ToString()));
                    throw new SeqAppException(Constants.SeqAppExceptions.ColumnCreationException);
                }
            }
        }

        private void InsertEvent(Dictionary<string, string> colVals)
        {
            try
            {
                // Initializing Connection
                using (var conn = new SqlConnection(ConnectionString))
                {
                    // Opening Connection
                    conn.Open();

                    // Initializing Command
                    using (var cmd = new SqlCommand(Scripts.InsertEvent, conn))
                    {
                        // Adding Parameters
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.SchemaNameParameter, SchemaName);
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.TableNameParameter, TableName);
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.ColumnsParameter, colVals.Keys.Select(col => col.FormatColumnWithBrackets()).JoinEnumerableWithCommas());
                        cmd.Parameters.AddWithValue(Constants.SqlParameters.ValuesParameter, colVals.Values.Select(val => val.FormatColumnValueWithSingleQuotes()).JoinEnumerableWithCommas());

                        // Executing Query
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ForContext(Constants.LogContextConstants.SchemaName, SchemaName)
                       .ForContext(Constants.LogContextConstants.TableName, TableName)
                       .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.ColumnCreationError, ex.ToString()));
                throw new SeqAppException(Constants.SeqAppExceptions.EventInsertionException);
            }
        }

        private Dictionary<string, string> GenerateColumnsAndValues(Event<LogEventData> evt)
        {
            try
            {
                // Initializing Dictionary with Pre-Defined Columns
                var colVals = new Dictionary<string, string>()
                {
                    { Constants.InitialColumns.SeqEventId, evt.Id },
                    { Constants.InitialColumns.SeqEventIngestionTimestamp, evt.TimestampUtc.ToString() },
                    { Constants.InitialColumns.SeqEventLocalTimestamp, evt.Data.LocalTimestamp.ToString() },
                    { Constants.InitialColumns.SeqEventLevel, evt.Data.Level.ToString() },
                    { Constants.InitialColumns.SeqEventMessage, evt.Data.RenderedMessage }
                };

                // Adding Event Properties
                var initialColumns = typeof(Constants.InitialColumns).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Select(p => p.Name);
                foreach (var kvp in evt.Data.Properties)
                {
                    // In the unlikely event the event data has properties matching the pre-defined columns, skip adding them
                    if (initialColumns.Contains(kvp.Key, StringComparer.CurrentCultureIgnoreCase))
                    {
                        Log.ForContext(Constants.LogContextConstants.PropertyName, kvp.Key)
                            .Warning(Constants.LogMessageConstants.SkippedAddingColumnWarning);
                    }
                    else
                    {
                        // Merge Duplicate Properties
                        if (colVals.Select(cv => cv.Key).Contains(kvp.Key, StringComparer.CurrentCultureIgnoreCase))
                        {
                            // Get the Actual Dictionary Key
                            var key = colVals.FirstOrDefault(cv => cv.Key.ToUpper().Equals(kvp.Key.ToUpper())).Key;

                            // Merge the data into the value
                            colVals[key] = !colVals[key].Contains("MERGED DATA =>") ? $"MERGED DATA => {key}: {colVals[key]}; {kvp.Key}: {kvp.Value}" : $"{colVals[key]}; {kvp.Key}: {kvp.Value}";
                        }
                        // Simply Add the Property
                        else
                        {
                            colVals.Add(kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                return colVals;
            }
            catch (Exception ex)
            {
                Log.ForContext(Constants.LogContextConstants.EventId, evt.Id)
                       .Error(string.Format(Constants.LogMessageConstants.ErrorFormat, Constants.LogMessageConstants.ColumnCreationError, ex.ToString()));
                throw new SeqAppException(Constants.SeqAppExceptions.InsertStatementGenerationException);
            }
        }
        #endregion
    }
}