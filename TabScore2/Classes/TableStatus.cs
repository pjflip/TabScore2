﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;
using TabScore2.SharedClasses;

namespace TabScore2.Classes
{
    public class TableStatus(int sectionID, int tableNumber, int roundNumber)
    {
        public int SectionID { get; set; } = sectionID;
        public int TableNumber { get; set; } = tableNumber;
        public int RoundNumber { get; set; } = roundNumber;
        public Round RoundData { get; set; } = new();
        public Result ResultData { get; set; } = new() { BoardNumber = 0, ContractLevel = -999, TricksTaken = -1 };
        public LeadValidationOptions LeadValidation { get; set; }
        public bool ReadyForNextRoundNorth { get; set; } = false;
        public bool ReadyForNextRoundSouth { get; set; } = false;
        public bool ReadyForNextRoundEast { get; set; } = false;
        public bool ReadyForNextRoundWest { get; set; } = false;
    }
}
