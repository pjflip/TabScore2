// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class TableStatusDisplay
    {
        public required string SectionLetter { get; set; }
        public required int TableNumber { get; set; }
        public required int RoundNumber { get; set; }
        public bool Devices { get; set; }
        public bool ReadyForNextRound { get; set; }
    }
}
