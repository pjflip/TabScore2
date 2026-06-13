// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using System.ServiceModel;

namespace GrpcSharedContracts
{
    [ServiceContract]
    public interface IBwsDatabaseService
    {
        // GENERAL
        [OperationContract] InitializeReturnMessage Initialize(InitializeMessage message);
        [OperationContract] void WebappInitialize();
        [OperationContract] IsDatabaseConnectionOKMessage IsDatabaseConnectionOK();

        // SECTION
        [OperationContract] List<Section> GetSectionsList();
        [OperationContract] Section GetSection(SectionIdMessage message);


        // TABLE
        [OperationContract] void RegisterTable(SectionTableMessage message);

        // ROUND
        [OperationContract] void UpdateNumberOfRoundsInSection(SectionIdMessage message);
        [OperationContract] NumberOfLastRoundWithResultsMessage GetNumberOfLastRoundWithResults(SectionTableMessage message);
        [OperationContract] List<Round> GetRoundsList(SectionRoundMessage message);
        [OperationContract] Round GetRound(SectionTableRoundMessage message);

        // RESULT = RECEIVEDDATA
        [OperationContract] Result GetResult(SectionTableRoundBoardMessage message);
        [OperationContract] void SetResult(Result result);
        [OperationContract] List<Result> GetResultsList(ResultsListMessage message);

        // PLAYERNAMES
        [OperationContract] PlayerNameMessage GetInternalPlayerName(PlayerMessage message);

        // PLAYERNUMBERS
        [OperationContract] void UpdatePlayer(UpdatePlayerNumberMessage message);
        [OperationContract] Names GetNamesForRound(NamesForRoundMessage message);

        // HANDRECORD
        [OperationContract] HandsCountMessage GetHandsCount();
        [OperationContract] List<Hand> GetHandsList();
        [OperationContract] Hand GetHand(SectionBoardMessage message);
        [OperationContract] void AddHand(Hand hand);
        [OperationContract] void AddHands(List<Hand> newHandsList);

        // SETTINGS
        [OperationContract] public DatabaseSettings GetDatabaseSettings(SectionRoundMessage message);

        [OperationContract] void SetDatabaseSettings(DatabaseSettings databaseSettings);

        // RANKINGLIST
        [OperationContract] List<Ranking> GetRankingList(SectionIdMessage message);
    }
}