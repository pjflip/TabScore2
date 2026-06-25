// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.ServiceModel;

namespace GrpcSharedContracts
{
    [ServiceContract]
    public interface IExternalNamesDatabaseService
    {
        [OperationContract] PlayerNameResponse GetExternalPlayerName(PlayerRequest request);
    }
}