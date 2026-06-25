// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;
using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.DataServices
{
    public class BwsDatabase(IBwsDatabaseService iClient, ISettings iSettings) : IDatabase
    {
        private readonly IBwsDatabaseService client = iClient;
        private readonly ISettings settings = iSettings;

        // ============================
        // Prepare the database for use
        // ============================
        public string Initialize(string pathToDatabase)  // Called from main form when path to database is set
        {
            InitializeResponse initializeReturnMessage = client.Initialize(new InitializeRequest() { 
                PathToDatabase = pathToDatabase,
                DefaultShowTraveller = settings.DefaultShowTraveller,
                DefaultShowPercentage = settings.DefaultShowPercentage,
                DefaultEnterLeadCard = settings.DefaultEnterLeadCard,
                DefaultValidateLeadCard = settings.DefaultValidateLeadCard,
                DefaultShowRanking = settings.DefaultShowRanking,
                DefaultEnterResultsMethod = settings.DefaultEnterResultsMethod,
                DefaultShowHandRecord = settings.DefaultShowHandRecord,
                DefaultNumberEntryEachRound = settings.DefaultNumberEntryEachRound,
                DefaultNameSource = settings.DefaultNameSource,
                DefaultManualHandRecordEntry = settings.DefaultManualHandRecordEntry
            });
            if (initializeReturnMessage.ErrorMessage == string.Empty)
            {
                settings.IsIndividual = initializeReturnMessage.IsIndividual;
                GetDatabaseSettings();
            }
            return initializeReturnMessage.ErrorMessage;
        }

        public void WebappInitialize()  // Called from webapp StartScreen and run just once.  After this point, changing the TabletsMove setting will have no effect
        {
            GetDatabaseSettings();    // Refresh setting as these can be changed by the scoring program
            ErrorResponse errorResponse = client.WebappInitialize();
            if (errorResponse.ErrorMessage != string.Empty) throw new Exception(errorResponse.ErrorMessage);
        }

        public bool IsDatabaseConnectionOK()
        {
            ErrorResponse errorResponse = client.CheckDatabaseConnection();
            return errorResponse.ErrorMessage == string.Empty;
        }

        // ========================================
        // Implement methods to access the database
        // ========================================

        // SECTION
        public Section GetSection(int sectionId)
        {
            return client.GetSection(new SectionRequest() { SectionId = sectionId });
        }

        public List<Section> GetSectionsList()
        {
            return client.GetSectionsList();
        }

        // TABLE
        public void RegisterTable(int sectionId, int tableNumber)
        {
            client.RegisterTable(new SectionTableRequest() { SectionId = sectionId, TableNumber = tableNumber });
        }

        // ROUND
        public int GetNumberOfRoundsInSection(int sectionId, bool forceDatabaseRead = false)
        {
            if (forceDatabaseRead)
            {
                ErrorResponse errorResponse = client.UpdateNumberOfRoundsInSection(new SectionRequest() { SectionId = sectionId });
                if (errorResponse.ErrorMessage != string.Empty) throw new Exception(errorResponse.ErrorMessage);
            }
            return client.GetSection(new SectionRequest() { SectionId = sectionId }).NumberOfRounds;
        }

        public int GetLastRoundWithResultsForTable(int sectionId, int tableNumber)
        {
            RoundNumberResponse response = client.GetLastRoundWithResultsForTable(new SectionTableRequest() { SectionId = sectionId, TableNumber = tableNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.RoundNumber;
        }

        public Location GetLastLocationWithResultsForContestant(int sectionId, int contestantNumber)
        {
            LocationResponse response = client.GetLastLocationWithResultsForContestant(
              new SectionContestantRequest() { SectionId = sectionId, ContestantNumber = contestantNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return new() 
            { 
                TableNumber = response.TableNumber, 
                RoundNumber = response.RoundNumber,
                Direction = response.Direction switch
                {
                    "North" => Direction.North,
                    "East" => Direction.East,
                    "South" => Direction.South,
                    "West" => Direction.West,
                    _ => Direction.Null,
                }
            };
        }

        public Location GetStartLocationForContestant(int sectionId, int contestantNumber)
        {
            LocationResponse response = client.GetStartLocationForContestant(new SectionContestantRequest() { SectionId = sectionId, ContestantNumber = contestantNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return new()
            {
                TableNumber = response.TableNumber,
                RoundNumber = response.RoundNumber,
                Direction = response.Direction switch
                {
                    "North" => Direction.North,
                    "East" => Direction.East,
                    "South" => Direction.South,
                    "West" => Direction.West,
                    _ => Direction.Null,
                }
            };
        }

        public List<Round> GetRoundsList(int sectionId, int roundNumber)
        {
            RoundsListResponse response = client.GetRoundsList(new SectionRoundRequest() { SectionId = sectionId, RoundNumber = roundNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Rounds;
        }

        public List<Round> GetRoundsList(int sectionId)
        {
            RoundsListResponse response = client.GetRoundsList(new SectionRequest() { SectionId = sectionId });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Rounds;
        }

        public Round GetRound(int sectionId, int tableNumber, int roundNumber)
        {
            RoundResponse response = client.GetRound(new SectionTableRoundRequest { SectionId = sectionId, TableNumber = tableNumber, RoundNumber = roundNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Round;
        }

        // RECEIVEDDATA
        public Result GetResult(int sectionId, int tableNumber, int roundNumber, int boardNumber)
        {
            ResultResponse response = client.GetResult(new SectionTableRoundBoardRequest { SectionId = sectionId, TableNumber = tableNumber, RoundNumber = roundNumber, BoardNumber = boardNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Result;
        }

        public void SetResult(Result result)
        {
            ErrorResponse errorResponse = client.SetResult(result);
            if (errorResponse.ErrorMessage != string.Empty) throw new Exception(errorResponse.ErrorMessage);
        }

        public List<Result> GetResultsList(int sectionId = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0)
        {
            ResultsListResponse response = client.GetResultsList(new ResultsListRequest() { SectionId = sectionId, LowBoard = lowBoard, HighBoard = highBoard, TableNumber = tableNumber, RoundNumber = roundNumber });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Results;
        }

        // PLAYERNAMES
        public string GetInternalPlayerName(string PlayerId)
        {
            PlayerNameResponse response = client.GetInternalPlayerName(new PlayerRequest() { PlayerId = PlayerId });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            string name = response.PlayerName;
            if (name == "Unknown")
            {
                return "#" + PlayerId;
            }
            else
            {
                return name;
            }
        }

        // PLAYERNUMBERS
        public NamesForRound GetNamesForTableRound(int sectionId, int roundNumber, int numberNorth, int numberEast, int numberSouth, int numberWest)
        {
            NamesForTableRoundResponse response = client.GetNamesForTableRound(new NamesForRoundRequest { SectionId = sectionId, RoundNumber = roundNumber, ContestantNumberNorth = numberNorth, ContestantNumberEast = numberEast, ContestantNumberSouth = numberSouth, ContestantNumberWest = numberWest });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return new() {
                NameNorth = response.NameNorth,
                NameEast = response.NameEast,
                NameSouth = response.NameSouth,
                NameWest = response.NameWest,
                GotAllNames = (numberNorth == 0 || response.NameNorth != string.Empty && response.NameSouth != string.Empty) 
                  && (numberEast == 0 || response.NameEast != string.Empty && response.NameWest != string.Empty),
            };
        }

        public void UpdatePlayer(int sectionId, int tableNumber, int roundNumber, string directionLetter, int pairNumber, string playerId, string playerName)
        {
            client.UpdatePlayer(new UpdatePlayerRequest() { SectionId = sectionId, TableNumber = tableNumber, RoundNumber = roundNumber, DirectionLetter = directionLetter, ContestantNumber = pairNumber, PlayerId = playerId, PlayerName = playerName });
        }

        // HANDRECORD
        public int GetHandsCount() 
        {
            return client.GetHandsCount().HandsCount;
        }

        public List<Hand> GetHandsList()
        {
            return client.GetHandsList();
        }

        public Hand GetHand(int sectionId, int boardNumber)
        {
            return client.GetHand(new SectionBoardRequest { SectionId = sectionId, BoardNumber = boardNumber });
        }

        public void AddHand(Hand hand)
        {
            if (hand.NorthSpades == "###") return;
            client.AddHand(hand);
        }

        public void AddHands(List<Hand> newHandsList)
        {
            client.AddHands(newHandsList);
        }

        // SETTINGS
        public void GetDatabaseSettings(int sectionId = 1, int roundNumber = 0)
        { 
            DatabaseSettings databaseSettings = client.GetDatabaseSettings(new SectionRoundRequest() { SectionId = sectionId, RoundNumber = roundNumber });
            if (databaseSettings.UpdateRequired)
            {
                settings.ShowTraveller = databaseSettings.ShowTraveller;
                settings.ShowPercentage = databaseSettings.ShowPercentage;
                settings.EnterLeadCard = databaseSettings.EnterLeadCard;
                settings.ValidateLeadCard = databaseSettings.ValidateLeadCard;
                settings.ShowRanking = databaseSettings.ShowRanking;
                settings.ShowHandRecord = databaseSettings.ShowHandRecord;
                settings.NumberEntryEachRound = databaseSettings.NumberEntryEachRound;
                settings.NameSource = databaseSettings.NameSource;
                settings.EnterResultsMethod = databaseSettings.EnterResultsMethod;
                settings.ManualHandRecordEntry = databaseSettings.ManualHandRecordEntry;
            }
        }

        public void SetDatabaseSettings()
        {
            DatabaseSettings databaseSettings = new()
            {
                ShowTraveller = settings.ShowTraveller,
                ShowPercentage = settings.ShowPercentage,
                EnterLeadCard = settings.EnterLeadCard,
                ValidateLeadCard = settings.ValidateLeadCard,
                ShowRanking = settings.ShowRanking,
                ShowHandRecord = settings.ShowHandRecord,
                NumberEntryEachRound = settings.NumberEntryEachRound,
                NameSource = settings.NameSource,
                EnterResultsMethod = settings.EnterResultsMethod,
                ManualHandRecordEntry = settings.ManualHandRecordEntry
            };
            client.SetDatabaseSettings(databaseSettings);
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(int sectionId)
        {
            RankingListResponse response = client.GetRankingList(new SectionRequest() { SectionId = sectionId });
            if (response.ErrorMessage != string.Empty) throw new Exception(response.ErrorMessage);
            return response.Rankings;
        }
    }
}
