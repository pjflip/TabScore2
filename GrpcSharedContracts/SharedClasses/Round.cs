// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace GrpcSharedContracts.SharedClasses
{
    [DataContract]
    public class Round
    {
        [DataMember(Order = 1)] public int TableNumber { get; set; }
        [DataMember(Order = 2)] public int ContestantNumberNorth { get; set; }   // Applies to NS pair in pairs and teams
        [DataMember(Order = 3)] public int ContestantNumberEast { get; set; }    // Applies to EW pair in pairs and teams
        [DataMember(Order = 4)] public int ContestantNumberSouth { get; set; }
        [DataMember(Order = 5)] public int ContestantNumberWest { get; set; }
        [DataMember(Order = 6)] public string NameNorth { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string NameSouth { get; set; } = string.Empty;
        [DataMember(Order = 8)] public string NameEast { get; set; } = string.Empty;
        [DataMember(Order = 9)] public string NameWest { get; set; } = string.Empty;
        [DataMember(Order = 10)] public bool GotAllNames { get; set; } = false;
        [DataMember(Order = 11)] public int LowBoard { get; set; }
        [DataMember(Order = 12)] public int HighBoard { get; set; }
    }
}