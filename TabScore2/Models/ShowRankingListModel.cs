// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;

namespace TabScore2.Models
{
    public class ShowRankingListModel : List<Ranking>
    {
        public int RoundNumber { get; set; }
        public int ContestantNumberNorth { get; set; } = 0;
        public int ContestantNumberEast { get; set; } = 0;
        public int ContestantNumberSouth { get; set; } = 0;
        public int ContestantNumberWest { get; set; } = 0;
        public bool FinalRankingList { get; set; } = false;
    }
}
