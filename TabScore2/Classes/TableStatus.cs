// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;

namespace TabScore2.Classes
{
    public class TableStatus()
    {
        public required int SectionId { get; set; }
        public required int TableNumber { get; set; }
        public required int RoundNumber { get; set; }
        public required Round RoundData { get; set; }
    }
}
