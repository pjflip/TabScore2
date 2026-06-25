// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;

namespace TabScore2.DataServices
{
    public class ExternalNamesDatabase(IExternalNamesDatabaseService iClient) : IExternalNamesDatabase
    {
        private readonly IExternalNamesDatabaseService client = iClient;

        public string GetExternalPlayerName(string playerId)
        {
            return client!.GetExternalPlayerName(new PlayerRequest() { PlayerId = playerId }).PlayerName;
        }
    }
}
