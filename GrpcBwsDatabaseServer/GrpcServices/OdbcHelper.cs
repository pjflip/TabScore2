// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Data.Odbc;

namespace GrpcBwsDatabaseServer.GrpcServices
{
    public static class OdbcHelper
    {
        // A class of helper methods for executing ODBC commands with retry logic and other utilities
        
        private const int CommandTimeoutSeconds = 5;
        
        // Returns "YES" or "NO" for use in Access SQL statements that set a YESNO field
        public static string YesNo(bool value) => value ? "YES" : "NO";

        // Executes the given command with retry logic for transient ODBC errors
        public static void OdbcRetry(Action cmd)
        {
            int attempts = 3;
            while (true)
            {
                try
                {
                    attempts--;
                    cmd();
                    break;
                }
                catch (OdbcException e)
                {
                    // Don't retry if single error is that table, column or field does not exist, or an attempt to add a field that already exists
                    if (e.Errors.Count == 1 && (e.Errors[0].SQLState == "42S02" || e.Errors[0].SQLState == "42S22" || e.Errors[0].SQLState == "07002" 
                      || e.Errors[0].SQLState == "HYS21")) throw;
                    if (attempts <= 0) throw;
                    Random r = new();
                    Thread.Sleep(r.Next(300, 500));
                }
            }
        }

        // Executes a non-query SQL command (like INSERT, UPDATE, DELETE) with retry logic
        public static void ExecuteNonQuery(OdbcConnection connection, string sqlString)
        {
            using OdbcCommand cmd = new(sqlString, connection) { CommandTimeout = CommandTimeoutSeconds };
            OdbcRetry(() => cmd.ExecuteNonQuery());
        }

        // Executes a SQL command that returns a single value (like COUNT(*)) with retry logic
        public static object? ExecuteScalar(OdbcConnection connection, string sqlString)
        {
            using OdbcCommand cmd = new(sqlString, connection) { CommandTimeout = CommandTimeoutSeconds };
            object? result = null;
            OdbcRetry(() => result = cmd.ExecuteScalar());
            return result;
        }

        // Executes a query and invokes processRow for each row of the result set
        public static void ExecuteReader(OdbcConnection connection, string sqlString, Action<OdbcDataReader> processRow)
        {
            using OdbcCommand cmd = new(sqlString, connection) { CommandTimeout = CommandTimeoutSeconds };
            OdbcRetry(() =>
            {
                using OdbcDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    processRow(reader);
                }
            });
        }

        // Executes a query and invokes processRow only for the first row of the result set
        public static void ExecuteReaderOnce(OdbcConnection connection, string sqlString, Action<OdbcDataReader> processRow)
        {
            using OdbcCommand cmd = new(sqlString, connection) { CommandTimeout = CommandTimeoutSeconds };
            OdbcRetry(() =>
            {
                using OdbcDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    processRow(reader);
                    break;
                }
            });
        }

        // Adds a column to a table if it doesn't already exist (Access reports SQLState HYS21 if it does)
        public static void AddColumnIfNotExists(OdbcConnection connection, string tableName, string columnName, string columnType, string? initialValue = null)
        {
            string SQLString = $"ALTER TABLE {tableName} ADD {columnName} {columnType}";
            try
            {
                ExecuteNonQuery(connection, SQLString);
                if (initialValue != null)
                {
                    SQLString = $"UPDATE {tableName} SET {columnName}={initialValue}";
                    ExecuteNonQuery(connection, SQLString);
                }
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }
        }
    }
}
