// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using System.Runtime.Serialization;

namespace GrpcSharedContracts
{
    // ================
    // REQUEST MESSAGES
    // ================

    [DataContract]
    public class InitializeRequest
    {
        [DataMember(Order = 1)] public string PathToDatabase { get; set; } = string.Empty;
        [DataMember(Order = 2)] public bool DefaultShowTraveller { get; set; }
        [DataMember(Order = 3)] public bool DefaultShowPercentage { get; set; }
        [DataMember(Order = 4)] public bool DefaultEnterLeadCard { get; set; }
        [DataMember(Order = 5)] public bool DefaultValidateLeadCard { get; set; }
        [DataMember(Order = 6)] public int DefaultShowRanking { get; set; }
        [DataMember(Order = 7)] public int DefaultEnterResultsMethod { get; set; }
        [DataMember(Order = 8)] public bool DefaultShowHandRecord { get; set; }
        [DataMember(Order = 9)] public bool DefaultNumberEntryEachRound { get; set; }
        [DataMember(Order = 10)] public int DefaultNameSource { get; set; }
        [DataMember(Order = 11)] public bool DefaultManualHandRecordEntry { get; set; }
    }

    [DataContract]
    public class SectionRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
    }

    [DataContract]
    public class PlayerRequest
    {
        [DataMember(Order = 1)] public string PlayerId { get; set; } = string.Empty;
    }

    [DataContract]
    public class SectionTableRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
    }

    [DataContract]
    public class SectionContestantRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int ContestantNumber { get; set; }
    }

    [DataContract]
    public class SectionRoundRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; }
    }

    [DataContract]
    public class SectionBoardRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class SectionTableRoundRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
    }

    [DataContract]
    public class SectionTableRoundBoardRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class ResultsListRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int LowBoard { get; set; }
        [DataMember(Order = 3)] public int HighBoard { get; set; }
        [DataMember(Order = 4)] public int TableNumber { get; set; }
        [DataMember(Order = 5)] public int RoundNumber { get; set; }
    }

    [DataContract]
    public class UpdatePlayerRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public string DirectionLetter { get; set; } = string.Empty;
        [DataMember(Order = 5)] public int ContestantNumber { get; set; }
        [DataMember(Order = 6)] public string PlayerId { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract]
    public class NamesForRoundRequest
    {
        [DataMember(Order = 1)] public int SectionId { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; }
        [DataMember(Order = 3)] public int ContestantNumberNorth { get; set; }
        [DataMember(Order = 4)] public int ContestantNumberEast { get; set; }
        [DataMember(Order = 5)] public int ContestantNumberSouth { get; set; }
        [DataMember(Order = 6)] public int ContestantNumberWest { get; set; }
    }


    // =================
    // RESPONSE MESSAGES
    // =================

    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
    }

    [DataContract]
    public class InitializeResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public bool IsIndividual { get; set; }
    }

    [DataContract]
    public class RoundNumberResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public int RoundNumber { get; set; } = 0;
    }
    
    [DataContract]
    public class LocationResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public int TableNumber { get; set; } = 0;
        [DataMember(Order = 3)] public int RoundNumber { get; set; } = 0;
        [DataMember(Order = 4)] public string Direction { get; set; } = string.Empty;
    }

    [DataContract]
    public class RoundsListResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public List<Round> Rounds { get; set; } = [];
    }

    [DataContract]
    public class RoundResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public Round Round { get; set; } = new();
    }

    [DataContract]
    public class ResultsListResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public List<Result> Results { get; set; } = [];
    }

    [DataContract]
    public class ResultResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public Result Result { get; set; } = new();
    }

    [DataContract]
    public class PlayerNameResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract]
    public class HandsCountResponse
    {
        [DataMember(Order = 1)] public int HandsCount { get; set; }
    }

    [DataContract]
    public class NamesForTableRoundResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public string NameNorth { get; set; } = string.Empty;
        [DataMember(Order = 3)] public string NameSouth { get; set; } = string.Empty;
        [DataMember(Order = 4)] public string NameEast { get; set; } = string.Empty;
        [DataMember(Order = 5)] public string NameWest { get; set; } = string.Empty;
    }

    [DataContract]
    public class RankingListResponse
    {
        [DataMember(Order = 1)] public string ErrorMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public List<Ranking> Rankings { get; set; } = [];
    }

    // =========================
    // DATABASE SETTINGS MESSAGE
    // =========================

    [DataContract]
    public class DatabaseSettings
    {
        [DataMember(Order = 1)] public bool UpdateRequired { get; set; }
        [DataMember(Order = 2)] public bool ShowTraveller { get; set; }
        [DataMember(Order = 3)] public bool ShowPercentage { get; set; }
        [DataMember(Order = 4)] public bool EnterLeadCard { get; set; }
        [DataMember(Order = 5)] public bool ValidateLeadCard { get; set; }
        [DataMember(Order = 6)] public int ShowRanking { get; set; }
        [DataMember(Order = 7)] public int EnterResultsMethod { get; set; }
        [DataMember(Order = 8)] public bool ShowHandRecord { get; set; }
        [DataMember(Order = 9)] public bool NumberEntryEachRound { get; set; }
        [DataMember(Order = 10)] public int NameSource { get; set; }
        [DataMember(Order = 11)] public bool ManualHandRecordEntry { get; set; }
    }
}