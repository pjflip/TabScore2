// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class SelectDirectionModel
    {
        public int SectionId { get; set; }
        public string SectionLetter { get; set; } = "A";
        public int TableNumber { get; set; }
        public Direction Direction { get; set; }
        public int RoundNumber { get; set; }
        public bool NorthSouthMissing { get; set; }
        public bool EastWestMissing { get; set; }
        public bool Confirm { get; set; }
    }
}