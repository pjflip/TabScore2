// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;
using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;

namespace TabScore2.DataServices
{
    // IDatabase is the interface for a service that allows access to data that is normally stored in the scoring database
    public interface IDatabase
    {
        // GENERAL
        string Initialize(string pathToDatabase);
        void WebappInitialize();
        public bool IsDatabaseConnectionOK();

        // SECTION
        List<Section> GetSectionsList();
        Section GetSection(int sectionId);

        // TABLE
        void RegisterTable(int sectionId, int tableNumber);

        // ROUND
        int GetNumberOfRoundsInSection(int sectionId, bool forceDatabaseRead = false);
        int GetLastRoundWithResultsForTable(int sectionId, int tableNumber);
        Location GetLastLocationWithResultsForContestant(int sectionId, int contestantNumber);
        Location GetStartLocationForContestant(int sectionId, int contestantNumber);
        public List<Round> GetRoundsListForSectionRound(int sectionId, int roundNumber);  // Specific round
        public List<Round> GetRoundsListForSection(int sectionId);  // All rounds
        public Round GetRound(int sectionId, int tableNumber, int roundNumber);

        // RESULT = RECEIVEDDATA
        Result GetResult(int sectionId, int tableNumber, int roundNumber, int boardNumber);
        void SetResult(Result result);
        List<Result> GetResultsList(int sectionId = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0);

        // PLAYERNAMES
        string GetInternalPlayerName(string playerId);

        // PLAYERNUMBERS
        void UpdatePlayer(int sectionId, int tableNumber, int roundNumber, string directionLetter, int pairNumber, string playerId, string playerName);
        NamesForRound GetNamesForTableRound(int sectionId, int roundNumber, int numberNorth, int numberEast, int numberSouth, int numberWest);

        // HANDRECORD
        int GetHandsCount();
        List<Hand> GetHandsList();
        Hand GetHand(int sectionId, int boardNumber);
        public void AddHand(Hand hand);
        void AddHands(List<Hand> newHandsList);

        // SETTINGS
        void GetDatabaseSettings(int sectionId = 1, int roundNumber = 0);
        void SetDatabaseSettings();

        // RANKINGLIST
        List<Ranking> GetRankingList(int sectionId);
    }
}
