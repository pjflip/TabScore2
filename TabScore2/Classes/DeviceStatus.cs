// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class DeviceStatus
    {
        public required int SectionId { get; set; }
        public required string SectionLetter { get; set; }
        public required int DevicesPerTable { get; set; }
        public required int TableNumber { get; set; }
        public required int ContestantNumber { get; set; }
        public required Direction Direction { get; set; }
        public required string Location { get; set; }
        public required int RoundNumber { get; set; }
        public bool ReadyForNextRound { get; set; } = false;
        public bool AtSitoutTable { get; set; } = false;
        public bool Scoring { get; set; } = false;
        public Result ResultData { get; set; } = new() { BoardNumber = 0, ContractLevel = -999, TricksTaken = -1 };
    }
}
