// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class PlayerEntry
    {
        public required string DisplayName { get; set; }
        public int ContestantNumber { get; set; }
        public Direction Direction { get; set; }
    }
}