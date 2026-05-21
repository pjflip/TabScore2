// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class DeviceStatus(int sectionId, string sectionLetter, int tableNumber, int pairNumber, int roundNumber, Direction direction)
    {
        public int SectionId { get; private set; } = sectionId;
        public string SectionLetter { get; set; } = sectionLetter;
        public int TableNumber { get; set; } = tableNumber;
        public int PairNumber { get; set; } = pairNumber;
        public Direction Direction { get; set; } = direction;
        public string? Location { get; set; }
        public int RoundNumber { get; set; } = roundNumber;
        public int DevicesPerTable { get; set; } = 1;
        public bool NamesUpdateRequired { get; set; } = true;
        public bool AtSitoutTable { get; set; }
        public Result ResultData { get; set; } = new() { BoardNumber = 0, ContractLevel = -999, TricksTaken = -1 };
    }
}
