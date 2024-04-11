﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace GrpcMessageClasses
{
    [DataContract]
    public class InitializeMessage
    {
        [DataMember(Order = 1)] public string PathToDatabase { get; set; } = string.Empty;
    }

    [DataContract]
    public class InitializeReturnMessage
    {
        [DataMember(Order = 1)] public string ReturnMessage { get; set; } = string.Empty;
        [DataMember(Order = 2)] public bool IsIndividual { get; set; }
    }

    [DataContract]
    public class WebappInitializeMessage
    {
        [DataMember(Order = 1)] public bool TabletsMove { get; set; }
    }

    [DataContract]
    public class IsIndividualMessage
    {
        [DataMember(Order = 1)] public bool IsIndividual { get; set; }
    }

    [DataContract]
    public class IsDatabaseConnectionOKMessage
    {
        [DataMember(Order = 1)] public bool IsDatabaseConnectionOK { get; set; }
    }

    [DataContract]
    public class SectionIDMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
    }

    [DataContract]
    public class NumberOfRoundsInSectionMessage
    {
        [DataMember(Order = 1)] public int NumberOfRoundsInSection { get; set; }
    }

    [DataContract]
    public class NumberOfLastRoundWithResultsMessage
    {
        [DataMember(Order = 1)] public int NumberOfLastRoundWithResults { get; set; }
    }

    [DataContract]
    public class PlayerMessage
    {
        [DataMember(Order = 1)] public string PlayerID { get; set; } = string.Empty;
    }

    [DataContract]
    public class PlayerNameMessage
    {
        [DataMember(Order = 1)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract]
    public class HandsCountMessage
    {
        [DataMember(Order = 1)] public int HandsCount { get; set; }
    }

    [DataContract]
    public class SectionTableMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
    }

    [DataContract]
    public class SectionRoundMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; }
    }
    [DataContract]
    public class SectionBoardMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class SectionTableRoundMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
    }
    [DataContract]
    public class SectionTableRoundBoardMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class ResultsListMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int LowBoard { get; set; }
        [DataMember(Order = 3)] public int HighBoard { get; set; }
        [DataMember(Order = 4)] public int TableNumber { get; set; }
        [DataMember(Order = 5)] public int RoundNumber { get; set; }
    }

    [DataContract]
    public class UpdatePlayerNumberMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public string DirectionLetter { get; set; } = string.Empty;
        [DataMember(Order = 5)] public int PairNumber { get; set; }
        [DataMember(Order = 6)] public string PlayerID { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract] 
    public class NamesForRoundMessage
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; }
        [DataMember(Order = 3)] public int NumberNorth { get; set; }
        [DataMember(Order = 4)] public int NumberEast { get; set; }
        [DataMember(Order = 5)] public int NumberSouth { get; set; }
        [DataMember(Order = 6)] public int NumberWest { get; set; }
    }
}