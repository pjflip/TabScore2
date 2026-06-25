// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;
using System.Data.Odbc;

namespace GrpcBwsDatabaseServer.GrpcServices
{
    public class ExternalNamesDatabaseService : IExternalNamesDatabaseService
    {
        public PlayerNameResponse GetExternalPlayerName(PlayerRequest request)
        {
            string name = "Unknown";
            OdbcConnectionStringBuilder externalDB = new() { Driver = "Microsoft Access Driver (*.mdb)" };
            externalDB.Add("Dbq", @"C:\Bridgemate\BMPlayerDB.mdb");
            externalDB.Add("Uid", "Admin");
            using (OdbcConnection connection = new(externalDB.ToString()))
            {
                try
                {
                    connection.Open();
                    string SQLString = $"SELECT Name FROM PlayerNameDatabase WHERE ID={request.PlayerId}";
                    object? queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                    if (queryResult != null)
                    {
                        string? tempName = queryResult.ToString();
                        if (tempName != null) name = tempName;
                    }
                }
                catch (OdbcException) { } // If we can't read the external database for whatever reason, just return "Unknown"
            }
            return new PlayerNameResponse() { PlayerName = name };
        }
    }
}
