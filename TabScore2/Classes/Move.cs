// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class Move
    {
        public int NewTableNumber { get; set; }
        public string DirectionString { get; set; } = string.Empty;
        public Direction NewDirection { get; set; }
        public string NewDirectionString { get; set; } = string.Empty;
        public bool Stay { get; set; }
        public bool NewTableIsSitout { get; set; } = false;
        public int ContestantNumber { get; set; }
    }
}
