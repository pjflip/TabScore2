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
        [OperationContract] InitializeResponse Initialize(InitializeRequest request);
        [OperationContract] ErrorResponse WebappInitialize();
        [OperationContract] ErrorResponse CheckDatabaseConnection();

        // SECTION
        [OperationContract] List<Section> GetSectionsList();
        [OperationContract] Section GetSection(SectionRequest request);


        // TABLE
        [OperationContract] void RegisterTable(SectionTableRequest request);

        // ROUND
        [OperationContract] ErrorResponse UpdateNumberOfRoundsInSection(SectionRequest request);
        [OperationContract] RoundNumberResponse GetLastRoundWithResultsForTable(SectionTableRequest request);
        [OperationContract] LocationResponse GetLastLocationWithResultsForContestant(SectionContestantRequest request);
        [OperationContract] LocationResponse GetStartLocationForContestant(SectionContestantRequest request);
        [OperationContract] RoundsListResponse GetRoundsListForSectionRound(SectionRoundRequest request);  // Specific round
        [OperationContract] RoundsListResponse GetRoundsListForSection(SectionRequest request);  // All rounds for section
        [OperationContract] RoundResponse GetRound(SectionTableRoundRequest request);

        // RESULT = RECEIVEDDATA
        [OperationContract] ResultResponse GetResult(SectionTableRoundBoardRequest request);
        [OperationContract] ErrorResponse SetResult(Result result);
        [OperationContract] ResultsListResponse GetResultsList(ResultsListRequest request);

        // PLAYERNAMES
        [OperationContract] PlayerNameResponse GetInternalPlayerName(PlayerRequest request);

        // PLAYERNUMBERS
        [OperationContract] ErrorResponse UpdatePlayer(UpdatePlayerRequest request);
        [OperationContract] NamesForTableRoundResponse GetNamesForTableRound(NamesForRoundRequest request);

        // HANDRECORD
        [OperationContract] HandsCountResponse GetHandsCount();
        [OperationContract] List<Hand> GetHandsList();
        [OperationContract] Hand GetHand(SectionBoardRequest request);
        [OperationContract] ErrorResponse AddHand(Hand hand);
        [OperationContract] ErrorResponse AddHands(List<Hand> newHandsList);

        // SETTINGS
        [OperationContract] DatabaseSettings GetDatabaseSettings(SectionRoundRequest request);
        [OperationContract] ErrorResponse SetDatabaseSettings(DatabaseSettings databaseSettings);

        // RANKINGLIST
        [OperationContract] RankingListResponse GetRankingList(SectionRequest request);
    }
}