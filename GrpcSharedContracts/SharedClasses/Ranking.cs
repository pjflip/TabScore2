// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace GrpcSharedContracts.SharedClasses
{
    [DataContract]
    public class Ranking
    {
        [DataMember(Order = 1)] public string Orientation { get; set; } = string.Empty;
        [DataMember(Order = 2)] public int ContestantNumber { get; set; }  // Pair number for pairs, player number for individuals, team number for teams
        [DataMember(Order = 3)] public string Score { get; set; } = string.Empty;
        [DataMember(Order = 4)] public double ScoreDecimal { get; set; }
        [DataMember(Order = 5)] public string Rank { get; set; } = string.Empty;
        [DataMember(Order = 6)] public double MP { get; set; }
        [DataMember(Order = 7)] public int MPMax { get; set; }
    }
}